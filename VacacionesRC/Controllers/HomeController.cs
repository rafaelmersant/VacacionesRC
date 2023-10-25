using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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