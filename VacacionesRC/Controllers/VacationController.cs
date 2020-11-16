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
    }
}