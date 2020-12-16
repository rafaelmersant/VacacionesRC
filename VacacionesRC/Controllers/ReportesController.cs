using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class ReportesController : Controller
    {
        // GET: Reportes
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            return View();
        }

        public ActionResult EnVacaciones()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            using (var db = new VacacionesRCEntities())
            {
                var outsourcing = db.GetEmployeeOnVacation().ToList();

                return View(outsourcing);
            }
        }
    }
}