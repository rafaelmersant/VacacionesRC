using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.Models;
using VacacionesRC.ViewModels;

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
                List<EmployeeOnVacationModel> employees = new List<EmployeeOnVacationModel>();

                var outsourcing = db.GetEmployeeOnVacation().ToList();

                foreach(var employee in outsourcing)
                {
                    employees.Add(new EmployeeOnVacationModel
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.EmployeeName,
                        EmployeeDepto = employee.EmployeeDepto,
                        EmployeeLocation = employee.Location,
                        Year = employee.Year,
                        DaysAvailable = employee.DaysAvailable
                    });
                }

                return View(outsourcing);
            }
        }
    }
}