using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;

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
            return View();
        }

        [HttpPost]
        public JsonResult GetEmployee(int employeeId)
        {
            var employee = Helper.GetEmployeeFromDB(employeeId);

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
                int holydays = Helper.GetHolidays(_startDate, _endDate);
                
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

                return Json(new { result = "200", message = requestedDays });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "startDate:" + startDate + " -- endDate:" + endDate);

                return Json(new { result = "500", message = ex.Message });
            }
        }
    }
}