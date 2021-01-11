using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;

namespace VacacionesRC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            if (Session["role"] == null) return RedirectToAction("Login", "User");

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
    }
}