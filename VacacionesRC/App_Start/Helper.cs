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
        public static bool SendRawEmail(string emailto, string subject, string body)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
                };

                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.Body = body;
                message.Subject = subject;
                message.To.Add(new MailAddress(emailto));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);


                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return false;
            }
        }

        public static void SendException(Exception ex, string extraInfo = "")
        {
            try
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
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
            }
        }

        public static void SendException(string message)
        {
            try
            {
                string _sentry = ConfigurationManager.AppSettings["sentry"];
                string _environment = ConfigurationManager.AppSettings["sentry_environment"];

                var ravenClient = new SharpRaven.RavenClient(_sentry);
                ravenClient.Environment = _environment;
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(message));

            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
            }
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
            try
            {
                string content = "Su nueva contraseña es: <b>" + newPassword + "</b>";

                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
                };

                MailMessage message = new MailMessage();
                message.IsBodyHtml = true;
                message.Body = content;
                message.Subject = "NUEVA CONTRASEÑA SISTEMA DE GESTION Y ADMINISTRACION DE VACACIONES";
                message.To.Add(new MailAddress(email));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return false;
            }
        }

        public static bool UpdateWithAllEmployeesFromAS400()
        {
            try
            {
                string environmentVACACIONES = ConfigurationManager.AppSettings["EnvironmentVacaciones"];

                if (environmentVACACIONES != "DEV")
                {
                    string cycle = HelperPayroll.GetPayrollPeriodByAdmissionDate(DateTime.Today.AddDays(-15), DateTime.Today.AddDays(-15).Year);

                    Helper.SendRawEmail("rafaelmersant@yahoo.com", "Employee Massive Upload Starts " + DateTime.Now.ToString(), "EmployeesToUpdate ciclo:" + cycle);
                    
                    var data = GetAllEmployeesFromAS400(cycle);

                    Helper.SendRawEmail("rafaelmersant@yahoo.com", "Employee Massive Upload " + DateTime.Now.ToString(), "EmployeesToUpdate:" + data.Tables[0].Rows.Count + " Ciclo:" + cycle);

                    if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow emp in data.Tables[0].Rows)
                        {
                            int _employeeId = 0;

                            try
                            {
                                _employeeId = int.Parse(emp.ItemArray[0].ToString());
                                var _employee = Helper.GetEmployee(_employeeId);

                                if (_employee == null)
                                {
                                    DateTime? admissionDate = null;
                                    if (emp.ItemArray[4].ToString().Length >= 8)
                                    {
                                        int year = int.Parse(emp.ItemArray[4].ToString().Substring(0, 4));
                                        int month = int.Parse(emp.ItemArray[4].ToString().Substring(4, 2));
                                        int day = int.Parse(emp.ItemArray[4].ToString().Substring(6, 2));

                                        admissionDate = new DateTime(year, month, day);
                                    }

                                    DateTime? terminateDate = null;
                                    if (emp.ItemArray[5].ToString().Length >= 8)
                                    {
                                        int year = int.Parse(emp.ItemArray[5].ToString().Substring(0, 4));
                                        int month = int.Parse(emp.ItemArray[5].ToString().Substring(4, 2));
                                        int day = int.Parse(emp.ItemArray[5].ToString().Substring(6, 2));

                                        terminateDate = new DateTime(year, month, day);
                                    }

                                    if (terminateDate != null) continue; // do not add people cancelled

                                    Employee employee = new Employee
                                    {
                                        EmployeeId = _employee.EmployeeId,
                                        EmployeeName = emp.ItemArray[1].ToString(),
                                        EmployeePosition = emp.ItemArray[2].ToString(),
                                        EmployeeDepto = emp.ItemArray[3].ToString(),
                                        EmployeeDeptoId = int.Parse(emp.ItemArray[6].ToString()),
                                        Email = emp.ItemArray[7].ToString(),
                                        Salary = decimal.Parse(emp.ItemArray[8].ToString()),
                                        Location = emp.ItemArray[9].ToString(),
                                        BankAccount = emp.ItemArray[10].ToString(),
                                        Identification = emp.ItemArray[11].ToString(),
                                        AdmissionDate = admissionDate,
                                        TerminateDate = terminateDate,
                                        Type = "I",
                                        CreatedDate = DateTime.Now
                                    };

                                    using (var db = new VacacionesRCEntities())
                                    {
                                        db.Employees.Add(employee);
                                        db.SaveChanges();
                                    }
                                }
                                else
                                {
                                    using (var db = new VacacionesRCEntities())
                                    {
                                        var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == _employee.EmployeeId);

                                        DateTime? terminateDate = null;
                                        if (emp.ItemArray[5].ToString().Length >= 8)
                                        {
                                            int year = int.Parse(emp.ItemArray[5].ToString().Substring(0, 4));
                                            int month = int.Parse(emp.ItemArray[5].ToString().Substring(4, 2));
                                            int day = int.Parse(emp.ItemArray[5].ToString().Substring(6, 2));

                                            terminateDate = new DateTime(year, month, day);
                                        }

                                        employee.EmployeePosition = emp.ItemArray[2].ToString();
                                        employee.EmployeeDepto = emp.ItemArray[3].ToString();
                                        employee.EmployeeDeptoId = int.Parse(emp.ItemArray[6].ToString());
                                        employee.Email = emp.ItemArray[7].ToString();
                                        employee.Salary = decimal.Parse(emp.ItemArray[8].ToString()) * 2;
                                        employee.Location = emp.ItemArray[9].ToString();
                                        employee.BankAccount = emp.ItemArray[10].ToString();
                                        employee.TerminateDate = terminateDate;

                                        db.SaveChanges();
                                    }

                                }
                            }
                            catch (Exception exEmp)
                            {
                                Helper.SendException(exEmp, "EmployeeID:" + _employeeId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return false;
            }

            Helper.SendRawEmail("rafaelmersant@yahoo.com", "Employee Massive Upload Ends " + DateTime.Now.ToString(), "EmployeesToUpdate ends");

            return true;
        }

        public static Employee GetEmployee(int employeeId, bool onlyLocal = false)
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
                            Email = data.Tables[0].Rows[0].ItemArray[7].ToString(),
                            Salary = decimal.Parse(data.Tables[0].Rows[0].ItemArray[8].ToString()) * 2,
                            Location = data.Tables[0].Rows[0].ItemArray[9].ToString(),
                            BankAccount = data.Tables[0].Rows[0].ItemArray[10].ToString(),
                            Identification = data.Tables[0].Rows[0].ItemArray[11].ToString(),
                            AdmissionDate = admissionDate,
                            TerminateDate = terminateDate,
                            Type = "I",
                            CreatedDate = DateTime.Now
                        };

                        using (var db = new VacacionesRCEntities())
                        {
                            db.Employees.Add(employee);
                            db.SaveChanges();
                        }
                    }
                }
                else if (!onlyLocal)
                {
                    string environmentVACACIONES = ConfigurationManager.AppSettings["EnvironmentVacaciones"];

                    if (environmentVACACIONES != "DEV")
                    {
                        using (var db = new VacacionesRCEntities())
                        {
                            employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);

                            var data = GetEmployeeFromAS400(employeeId.ToString());
                            if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
                            {
                                DateTime? terminateDate = null;
                                if (data.Tables[0].Rows[0].ItemArray[5].ToString().Length >= 8)
                                {
                                    int year = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(0, 4));
                                    int month = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(4, 2));
                                    int day = int.Parse(data.Tables[0].Rows[0].ItemArray[5].ToString().Substring(6, 2));

                                    terminateDate = new DateTime(year, month, day);
                                }

                                employee.EmployeePosition = data.Tables[0].Rows[0].ItemArray[2].ToString();
                                employee.EmployeeDepto = data.Tables[0].Rows[0].ItemArray[3].ToString();
                                employee.EmployeeDeptoId = int.Parse(data.Tables[0].Rows[0].ItemArray[6].ToString());
                                employee.Email = data.Tables[0].Rows[0].ItemArray[7].ToString();
                                employee.Salary = decimal.Parse(data.Tables[0].Rows[0].ItemArray[8].ToString()) * 2;
                                employee.Location = data.Tables[0].Rows[0].ItemArray[9].ToString();
                                employee.BankAccount = data.Tables[0].Rows[0].ItemArray[10].ToString();
                                employee.TerminateDate = terminateDate;

                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + employeeId);
            }

            return employee;
        }

        public static Employee GetEmployeeFromDB(int employeeId)
        {
            try
            {
                string environmentID = ConfigurationManager.AppSettings["EnvironmentVacaciones"];

                using (var db = new VacacionesRCEntities())
                {
                    return db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + employeeId);
            }

            return null;
        }

        public static DataSet GetEmployeeFromAS400(string employeeId)
        {
            try
            {
                string sQuery = string.Empty;

                //SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO FROM QS36F.RCNOCE00
                //SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO FROM QS36F.RCNOCE00 WHERE CECICLOPAG='20200816' and CEINGDEDUC='I'

                sQuery = "SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO, CECORREOEL, " +
                    "CEVALTRANS, CEDESCSUCU, CECUEBANCO, CENUMCEDUL FROM QS36F.RCNOCE00 WHERE CECODEMPLE = " + employeeId + " AND CEINGDEDUC = 'I' ORDER BY CECICLOPAG DESC";

                if (ConfigurationManager.AppSettings["EnvironmentVacaciones"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        public static DataSet GetAllEmployeesFromAS400(string cycle)
        {
            try
            {
                string sQuery = string.Empty;

                sQuery = "SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO, CECORREOEL, " +
                    "CEVALTRANS, CEDESCSUCU, CECUEBANCO, CENUMCEDUL FROM QS36F.RCNOCE00 WHERE CEINGDEDUC = 'I' AND CEFRETIRO = 0 ORDER BY CECICLOPAG DESC"; //AND CETIPTRANS = 1

                //sQuery = "SELECT CECODEMPLE, CENOMEMPLE, CENOMCARGO, CENOMDEPTO, CEFINGRESO, CEFRETIRO, CECODDEPTO, CECORREOEL, " +
                //"CEVALTRANS, CEDESCSUCU, CECUEBANCO, CENUMCEDUL FROM QS36F.RCNOCE00 WHERE CECICLOPAG = '" + cycle + "' AND CEINGDEDUC = 'I' ORDER BY CECODEMPLE DESC"; //AND CETIPTRANS = 1

                if (ConfigurationManager.AppSettings["EnvironmentVacaciones"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        public static DataSet GetPayrollDetailForEmployee(string employeeId, string cycle, string paytype)
        {
            try
            {
                string sQuery = string.Empty;

                sQuery = "SELECT CECODIRE, CETIPOPAGO, CEDESCPAGO, CEINGDEDUC, CETIPTRANS, CEDESCTRAN, CEVALTRANS, CECICLOPAG," +
                " CECODEMPLE, CENOMEMPLE, CENOMDEPTO, CENOMCARGO, CECORREOEL, CECUEBANCO, CENUSEGSOC, CENUMCEDUL, CEDESDIREC," +
                "  CEDESCFPAG, CEDESCTCUE, CEBALAACTU, CETIPONOM, CECANTIDAD FROM [QS36F.RCNOCE00] WHERE CECODEMPLE = " + employeeId +
                " AND CECICLOPAG = '" + cycle + "' AND CETIPOPAGO IN ('N','V') ORDER BY CEINGDEDUC DESC";


                if (ConfigurationManager.AppSettings["EnvironmentVacaciones"] != "DEV")
                    sQuery = sQuery.Replace("[", "").Replace("]", "");

                return ExecuteDataSetODBC(sQuery, null);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
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

        public static void GetElapsedTime(DateTime from_date, DateTime to_date,
        out int years, out int months, out int days, out int hours,
        out int minutes, out int seconds, out int milliseconds)
        {
            // If from_date > to_date, switch them around.
            if (from_date > to_date)
            {
                GetElapsedTime(to_date, from_date,
                    out years, out months, out days, out hours,
                    out minutes, out seconds, out milliseconds);
                years = -years;
                months = -months;
                days = -days;
                hours = -hours;
                minutes = -minutes;
                seconds = -seconds;
                milliseconds = -milliseconds;
            }
            else
            {
                // Handle the years.
                years = to_date.Year - from_date.Year;

                // See if we went too far.
                DateTime test_date = from_date.AddMonths(12 * years);
                if (test_date > to_date)
                {
                    years--;
                    test_date = from_date.AddMonths(12 * years);
                }
                // Add months until we go too far.
                months = 0;
                while (test_date <= to_date)
                {
                    months++;
                    test_date = from_date.AddMonths(12 * years + months);
                }
                months--;

                // Subtract to see how many more days,
                // hours, minutes, etc. we need.
                from_date = from_date.AddMonths(12 * years + months);
                TimeSpan remainder = to_date - from_date;
                days = remainder.Days;
                hours = remainder.Hours;
                minutes = remainder.Minutes;
                seconds = remainder.Seconds;
                milliseconds = remainder.Milliseconds;
            }
        }

        public static string ShowVacationForm(string FechaSolicitud, string Codigo, string Nombre, 
                                       string FechaIngreso, string Puesto, string Departamento, 
                                       string DiasCorrespondientes, string DiasRestantes, string DiasSolicitados,
                                       string FechaDesde, string FechaHasta, string FechaRetorno, string Observacion, string server)
        {
            string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
            formTemplate = System.IO.Path.Combine(formTemplate, "FormularioVacaciones.html");

            string content = System.IO.File.ReadAllText(formTemplate);

            content = content.Replace("##FechaSolicitud##", FechaSolicitud);
            content = content.Replace("##Codigo##", Codigo);
            content = content.Replace("##Nombre##", Nombre);
            content = content.Replace("##FechaIngreso##", FechaIngreso);

            content = content.Replace("##Puesto##", Puesto);
            content = content.Replace("##Departamento##", Departamento);
            content = content.Replace("##DiasCorrespondientes##", DiasCorrespondientes);
            content = content.Replace("##DiasRestantes##", DiasRestantes);
            content = content.Replace("##DiasSolicitados##", DiasSolicitados);
            content = content.Replace("##FechaDesde##", FechaDesde);
            content = content.Replace("##FechaHasta##", FechaHasta);
            content = content.Replace("##FechaRetorno##", FechaRetorno);
            content = content.Replace("##Observacion##", Observacion);

            content = content.Replace("##server##", server);

            return content;
        }

        public static string ShowConstancia(string FechaSolicitud, string Codigo, string Nombre, string FechaIngreso, string Puesto, string Departamento,
                                            string FechaDesde, string FechaHasta, string Cedula, string TiempoTrabajando, string SalarioMensual, string Localidad,
                                            string CuentaBanco, string MontoPagado, string FirmadoPor, string DiasPagados, string server)
        {
            string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
            formTemplate = System.IO.Path.Combine(formTemplate, "ConstanciaVacaciones.html");

            string content = System.IO.File.ReadAllText(formTemplate);

            content = content.Replace("##NombreColaborador##", Nombre);
            content = content.Replace("##CedulaColaborador##", Cedula);
            content = content.Replace("##FechaIngreso##", FechaIngreso);
            content = content.Replace("##PuestoColaborador##", Puesto);

            content = content.Replace("##DeptoColaborador##", Departamento);
            content = content.Replace("##Fecha##", FechaSolicitud);
            content = content.Replace("##TiempoTrabajando##", TiempoTrabajando);
            content = content.Replace("##SalarioMensual##", SalarioMensual);
            content = content.Replace("##Localidad##", Localidad);
            content = content.Replace("##FechaDesde##", FechaDesde);
            content = content.Replace("##FechaHasta##", FechaHasta);
            content = content.Replace("##CuentaBanco##", CuentaBanco);
            content = content.Replace("##MontoPagado##", MontoPagado);
            content = content.Replace("##DiasPagados##", DiasPagados);
            content = content.Replace("##FirmadoPor##", FirmadoPor);

            content = content.Replace("##server##", server);


            return content;
        }

        public static void SendEmailVacationNotification(string EmailResponsableDepto, string NombreColaborador, string PuestoColaborador, 
                                                         string FechaInicio, string FechaHasta, string FechaRetorno, string RedirectURL)
        {
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["smtpClient"],
                    Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                    EnableSsl = false,
                };

                string formTemplate = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/");
                formTemplate = System.IO.Path.Combine(formTemplate, "NotificacionSolicitud.html");

                string content = System.IO.File.ReadAllText(formTemplate);

                content = content.Replace("##NombreColaborador##", NombreColaborador);
                content = content.Replace("##PuestoColaborador##", PuestoColaborador);
                content = content.Replace("##FechaInicio##", FechaInicio);
                content = content.Replace("##FechaHasta##", FechaHasta);
                content = content.Replace("##FechaRetorno##", FechaRetorno);
                content = content.Replace("##RedirectTo##", RedirectURL);

                MailMessage message = new MailMessage();

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content, null, "text/html");
                LinkedResource theEmailImage = new LinkedResource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content/Images/", "RCLogo.PNG"));
                theEmailImage.ContentId = "logoID";
                htmlView.LinkedResources.Add(theEmailImage);

                message.AlternateViews.Add(htmlView);
                message.IsBodyHtml = true;
                //message.Body = content;
                message.Subject = "Nueva Solicitud de Vacaciones";
                message.To.Add(new MailAddress(EmailResponsableDepto));
                
                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }
    }

}