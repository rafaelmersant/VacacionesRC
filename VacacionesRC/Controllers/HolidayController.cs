using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class HolidayController : Controller
    {
        // GET: Holiday
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            using (var db = new VacacionesRCEntities())
            {
                var days = db.Holidays.OrderByDescending(o => o.Date).ToList();
                return View(days);
            }
        }

        [HttpPost]
        public JsonResult Create(string date)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    DateTime _date = DateTime.Parse(date);

                    Holiday day = new Holiday
                    {
                        Date = _date,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Session["email"].ToString()
                    };

                    db.Holidays.Add(day);
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                Helper.SendException(e);

                string validErrors = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        validErrors += string.Format("- Property: \"{0}\", Error: \"{1}\" <br/>", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                return Json(new { result = "500", message = validErrors });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Holiday _day = db.Holidays.FirstOrDefault(h => h.Id == id);
                    if (_day != null)
                    {
                        db.Holidays.Remove(_day);
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