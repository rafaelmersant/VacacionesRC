using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class ReglasController : Controller
    {
        // GET: Reglas
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            using (var db = new VacacionesRCEntities())
            {
                var rules = db.Rules.OrderByDescending(o => o.CreatedDate).ToList();

                return View(rules);
            }
        }

        public ActionResult Outsourcing()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            using (var db = new VacacionesRCEntities())
            {
                var outsourcing = db.Employees.Where(e => e.Type == "E").OrderByDescending(o => o.CreatedDate).ToList();

                return View(outsourcing);
            }
        }

        [HttpPost]
        public JsonResult UpdateEmployee(int employeeId)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                    if (employee != null)
                    {
                        db.Employees.Remove(employee);
                        db.SaveChanges();

                        Helper.GetEmployee(employeeId);

                        return new JsonResult { Data = new { result = "200", message = "Updated" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        return new JsonResult { Data = new { result = "404", message = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + employeeId);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult UpdateRule(int id, string value)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Rule rule = db.Rules.FirstOrDefault(r => r.Id == id);

                    if (rule != null)
                    {
                        rule.Value = value;
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Updated" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        return new JsonResult { Data = new { result = "404", message = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult SaveOutsourcing(int employeeId, string employeeName, string employeeEmail)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = db.Employees.FirstOrDefault(d => d.EmployeeId == employeeId);

                    if (employee != null)
                    {
                        employee.EmployeeName = employeeName;
                        employee.Email = employeeEmail;
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Updated" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        db.Employees.Add(new Employee
                        {
                            EmployeeId = employeeId,
                            EmployeeName = employeeName,
                            Email = employeeEmail,
                            EmployeePosition = "",
                            Type ="E",
                            CreatedDate = DateTime.Now
                        });
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Added" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + employeeId + " |employeeName:" + employeeName + "|employeeEmail:" + employeeEmail);

                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
    }
}