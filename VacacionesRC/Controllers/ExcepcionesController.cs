using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;
using VacacionesRC.ViewModels;

namespace VacacionesRC.Controllers
{
    public class ExcepcionesController : Controller
    {
        // GET: Excepciones
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    List<ExceptionModel> exceptionModels = new List<ExceptionModel>();

                    var excepciones = (from x in db.ExceptionsVacations
                                       join e in db.Employees on x.EmployeeId equals e.EmployeeId
                                       select new
                                       {
                                           Id = x.Id,
                                           EmployeeId = x.EmployeeId,
                                           EmployeeName = e.EmployeeName,
                                           Year = x.Year,
                                           CreatedDate = x.CreatedDate,
                                           CreatedBy = x.CreatedBy
                                       });

                    foreach (var item in excepciones)
                    {
                        exceptionModels.Add(new ExceptionModel
                        {
                            Id = item.Id,
                            EmployeeId = item.EmployeeId,
                            EmployeeName = item.EmployeeName,
                            Year = item.Year,
                            CreatedDate = item.CreatedDate,
                            CreatedBy = item.CreatedBy
                        });
                    }

                    return View(exceptionModels);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        [HttpPost]
        public JsonResult Save(int Id, int EmployeeId, int Year)
        {
            try
            {
                if (Year < DateTime.Today.Year)
                    throw new Exception("El año no puede ser menor al año en curso.");

                if (Year.ToString().Length > 4)
                    throw new Exception("El año digitado parece no válido.");

                using (var db = new VacacionesRCEntities())
                {
                    ExceptionsVacation exceptVacation = db.ExceptionsVacations.FirstOrDefault(e => e.Id == Id);

                    if (exceptVacation != null)
                    {
                        exceptVacation.EmployeeId = EmployeeId;
                        exceptVacation.Year = Year;
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Updated" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        db.ExceptionsVacations.Add(new ExceptionsVacation
                        {
                            EmployeeId = EmployeeId,
                            Year = Year,
                            CreatedDate = DateTime.Now,
                            CreatedBy = int.Parse(Session["employeeID"].ToString())
                        });
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Added" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    ExceptionsVacation exceptionsVacation = db.ExceptionsVacations.FirstOrDefault(e => e.Id == id);
                    if (exceptionsVacation != null)
                    {
                        db.ExceptionsVacations.Remove(exceptionsVacation);
                        db.SaveChanges();
                    }
                }
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