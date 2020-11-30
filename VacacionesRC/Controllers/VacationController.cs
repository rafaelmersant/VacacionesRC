using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class VacationController : Controller
    {
        // GET: Vacation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Formulario()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public JsonResult GetCorrespondingDays(int employeeId)
        {
            EmployeeDay employeeDay;

            try
            {
                employeeDay = HelperDays.GetDaysForEmployee(employeeId);
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            var employeeDaySerialized = JsonConvert.SerializeObject(employeeDay);
            return new JsonResult { Data = new { result = "200", message = employeeDaySerialized }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult GetEmployee(int employeeId)
        {
            var employee = Helper.GetEmployee(employeeId);

            try
            {
                if (employee != null)
                {
                    var employeeSerialized = JsonConvert.SerializeObject(employee);
                    return Json(new { result = "200", message = employeeSerialized });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + employeeId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
        }

        [HttpPost]
        public JsonResult RequestedDays(string startDate, string endDate)
        {
            try
            {
                int requestedDays = 0;

                DateTime _startDate = DateTime.Parse(startDate);
                DateTime _endDate = DateTime.Parse(endDate);

                DateTime currentDay = _startDate;
                int holydays = HelperDays.GetHolidays(_startDate, _endDate);
                
                while (currentDay < _endDate.AddDays(1))
                {
                    if (currentDay.DayOfWeek == DayOfWeek.Monday ||
                        currentDay.DayOfWeek == DayOfWeek.Tuesday ||
                        currentDay.DayOfWeek == DayOfWeek.Wednesday ||
                        currentDay.DayOfWeek == DayOfWeek.Thursday ||
                        currentDay.DayOfWeek == DayOfWeek.Friday)
                    {
                        requestedDays += 1;
                    }

                    currentDay = currentDay.AddDays(1);
                }

                requestedDays -= holydays;

                var returnDate = HelperDays.GetReturnDate(_endDate);

                return new JsonResult { Data = new { result = "200", requestedDays, returnDate = returnDate.Value.ToShortDateString() }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "startDate:" + startDate + " -- endDate:" + endDate);

                return new JsonResult { Data = new { result = "500", requestedDays = ex.Message, returnDate = ""}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult Save(Vacation vacation)
        {
            Vacation vacaciontEdit = null;

            try
            {
                if (Session["employeeID"] == null) throw new Exception("Por favor intente hacer saliendo y entrando nuevamente al sistema.");

                using (var db = new VacacionesRCEntities())
                {
                    if (vacation.Id > 0)
                        vacaciontEdit = db.Vacations.FirstOrDefault(v => v.Id == vacation.Id);

                    Vacation newVacation = new Vacation
                    {
                        IdHash = Guid.NewGuid(),
                        EmployeeId = vacation.EmployeeId,
                        DaysTaken = vacation.DaysTaken,
                        DaysAvailable = vacation.DaysAvailable,
                        DaysRequested = vacation.DaysRequested,
                        StartDate = vacation.StartDate,
                        EndDate = vacation.EndDate,
                        ReturnDate = vacation.ReturnDate,
                        Note = vacation.Note,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Session["employeeID"].ToString(),
                        Status = "En proceso"
                    };

                    db.Vacations.Add(newVacation);
                    db.SaveChanges();

                    HelperDays.UpdateTakenDays(int.Parse(vacation.EmployeeId), newVacation.DaysRequested);
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + vacation.EmployeeId);

                return Json(new { result = "500", message = ex.Message });
            }
        }
    }
}