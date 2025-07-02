using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return RedirectToAction("About");
            if (Session["role"] == null) return RedirectToAction("Login", "User");

            ViewBag.SelectRole = false;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var employeId = Session["employeeID"].ToString();
                    var _user = db.Users.FirstOrDefault(u => u.EmployeeID == employeId && u.Role == "Admin");
                    if (_user != null) return View();

                    int empId = int.Parse(employeId);
                    var departments = db.Departments.Where(d => d.DeptoOwner == empId).ToList();

                    if (departments.Count > 1 
                        && departments.FirstOrDefault(d => d.UserRole.Contains("APOYO")) != null
                        && departments.FirstOrDefault(d => d.UserRole.Contains("APROBADOR")) != null)
                        ViewBag.SelectRole = true;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult TestEmailAdmin()
        {
            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            try
            {
                string content = "Su nueva contraseña es: <b>" + "testingpassword" + "</b>";

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
                message.To.Add(new MailAddress("rafaelmersant@sagaracorp.com"));

                string address = ConfigurationManager.AppSettings["EMail"];
                string displayName = ConfigurationManager.AppSettings["EMailName"];
                message.From = new MailAddress(address, displayName);

                smtp.Send(message);

                return Content("Email Enviado!");
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return Content(ex.ToString());
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            //UserController userController = new UserController();
            
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult SetRole(string value)
        {
            try
            {
                Session["role"] = value;
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }
    }
}