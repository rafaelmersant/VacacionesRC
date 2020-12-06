using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using VacacionesRC.Models;

namespace VacacionesRC.App_Start
{
    public class Helper
    {
        public static void SendRawEmail(string emailto, string emailfrom, string firstname, string lastname, string subject, string body)
        {
            var mail = new MailMessage();
            var smtp = new SmtpClient();

            mail.From = new MailAddress(emailfrom, firstname + " " + lastname);

            if (emailto.Contains(";"))
            {
                var emails = emailto.Split(';');
                foreach (var email in emails)
                    mail.To.Add(email);
            }
            else
                mail.To.Add(emailto);

            mail.Subject = subject;

            mail.Body = body;
            mail.IsBodyHtml = true;

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }

        public static void SendException(Exception ex, string extraInfo = "")
        {
            string _sentry = ConfigurationManager.AppSettings["sentry_dsn"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;

            var exception = new SharpRaven.Data.SentryEvent(ex);

            if (!string.IsNullOrEmpty(extraInfo))
                exception.Extra = extraInfo;

            ravenClient.Capture(exception);
        }

        public static void SendException(string message)
        {
            string _sentry = ConfigurationManager.AppSettings["sentry"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;
            ravenClient.Capture(new SharpRaven.Data.SentryEvent(message));
        }

        public static string SHA256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));

            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("X2"));
            }
            return hash.ToString();
        }

        public static bool SendRecoverPasswordEmail(string newPassword, string email)
        {
            string content = "Su nueva contraseña es: <b>" + newPassword + "</b>";

            SmtpClient smtp = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["smtpClient"],
                Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                EnableSsl = true,
            };

            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.Body = content;
            message.Subject = "NUEVA CONTRASEÑA SISTEMA DE GESTION Y ADMINISTRACION DE VACACIONES";
            message.To.Add(new MailAddress(email));

            string address = ConfigurationManager.AppSettings["EMail"];
            string displayName = ConfigurationManager.AppSettings["EMailName"];
            message.From = new MailAddress(address, displayName);

            try
            {
                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return false;
            }
        }
      
        public static Employee GetEmployee(int employeeId)
        {
            Employee employee = GetEmployeeFromDB(employeeId);

            try
            {
                if (employee == null)
                {
                    var data = GetEmployeeFromAS400(employeeId.ToString());
                    if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                    {
                        DateTime? admissionDate = null;
                        if (data.Tables[0].Rows[0].ItemArray[4].ToString().Length >= 8)
                        {
                            int year = int.Parse(data.Tables[0].Rows[0].ItemArray[4].ToString().Substring(0, 4));
                            int month = int.Parse(data.Tables[0].Rows[0].ItemArray[4].ToString().Substring(4,2));
                            int day = int.Parse(data.Tables[0].Rows[0].ItemArray[4].ToString().Substring(6, 2));

                            admissionDate = new DateTime(year, month, day);
                        }

                        DateTime? terminateDate = null;
                        if (data.Tables[0].Rows[0].ItemArray[5].ToString().Length >= 8)
                        {
                            int year = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(0, 4));
                            int month = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(4, 2));
                            int day = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(6, 2));

                            terminateDate = new DateTime(year, month, day);
                        }

                        employee = new Employee
                        {
                            EmployeeId = employeeId,
                            EmployeeName = data.Tables[0].Rows[0].ItemArray[1].ToString(),
                            EmployeePosition = data.Tables[0].Rows[0].ItemArray[2].ToString(),
                            EmployeeDepto = data.Tables[0].Rows[0].ItemArray[3].ToString(),
                            EmployeeDeptoId = int.Parse(data.Tables[0].Rows[0].ItemArray[6].ToString()),
                            AdmissionDate = admissionDate,
                            TerminateDate = terminateDate,
                            CreatedDate = DateTime.Now
                        };

                        using (var db = new VacacionesRCEntities())
                        {
                            db.Employees.Add(employee);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return employee;
        }

        public static Employee GetEmployeeFromDB(int employeeId)
        {
            string environmentID = ConfigurationManager.AppSettings["EnvironmentVacaciones"];

            using(var db = new VacacionesRCEntities())
            {
                return db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
            }
        }

        public static DataSet GetEmployeeFromAS400(string employeeId)
        {
            string sQuery = string.Empty;

            //SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO FROM QS36F.RCNOCE00
            //SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO FROM QS36F.RCNOCE00 WHERE CECICLOPAG='20200816' and CEINGDEDUC='I'

            sQuery = "SELECT TOP 1 CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO FROM [QS36F.RCNOCE00] WHERE CECODEMPLE = " + employeeId +
            " ORDER BY CECICLOPAG DESC";

            if (ConfigurationManager.AppSettings["EnvironmentVolante"] == "PROD")
                sQuery = sQuery.Replace("[", "").Replace("]", "");

            return ExecuteDataSetODBC(sQuery, null);
        }

        public static DataSet ExecuteDataSetODBC(string query, OdbcParameter[] parameters = null)
        {
            string sConn = ConfigurationManager.AppSettings["sConnSQLODBC"];

            OdbcConnection oconn = new OdbcConnection(sConn);
            OdbcCommand ocmd = new OdbcCommand(query, oconn);
            OdbcDataAdapter adapter;
            DataSet dsData = new DataSet();

            ocmd.CommandType = CommandType.Text;

            if (parameters != null)
            {
                ocmd.Parameters.Clear();

                foreach (OdbcParameter param in parameters)
                    ocmd.Parameters.Add(param);
            }

            adapter = new OdbcDataAdapter(ocmd);
            adapter.Fill(dsData);

            return dsData;
        }
    }
}