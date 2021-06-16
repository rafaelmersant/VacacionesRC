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
    public class DeptoController : Controller
    {
        // GET: Depto
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                ViewBag.Roles = GetRolesDepto();

                using (var db = new VacacionesRCEntities())
                {
                    List<DeptoModel> deptoModels = new List<DeptoModel>();

                    var deptos = db.GetDeptos();

                    foreach (var item in deptos)
                    {
                        deptoModels.Add(new DeptoModel
                        {
                            DeptoCode = item.DeptoCode,
                            DeptoName = item.DeptoName,
                            OwnerId = item.DeptoOwner,
                            OwnerName = item.EmployeeName,
                            UserRole = item.UserRole
                        });
                    }

                    return View(deptoModels);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        [HttpPost]
        public JsonResult Save(int DeptoCode, string DeptoName, int OwnerDeptoId, string UserRoleId)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Department department = db.Departments.FirstOrDefault(d => d.DeptoCode == DeptoCode && d.DeptoOwner == OwnerDeptoId && d.UserRole == UserRoleId);

                    if (department != null)
                    {
                        department.DeptoName = DeptoName;
                        //department.DeptoOwner = OwnerDeptoId;
                        //department.UserRole = UserRoleId;
                        db.SaveChanges();

                        return new JsonResult { Data = new { result = "200", message = "Updated" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        db.Departments.Add(new Department
                        {
                            DeptoCode = DeptoCode,
                            DeptoName = DeptoName,
                            DeptoOwner = OwnerDeptoId,
                            UserRole = UserRoleId,
                            CreatedDate = DateTime.Now
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
        public JsonResult GetDepartamentByCode(int DeptoCode)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Department department = db.Departments.FirstOrDefault(d => d.DeptoCode == DeptoCode);

                    if (department != null)
                        return new JsonResult { Data = new { result = "200", message = department.DeptoName }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            return new JsonResult { Data = new { result = "400", message = "" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult DeleteRecord(int DeptoCode, int OwnerCode, string UserRole)
        {
            try
            {
                if (Session["employeeID"] == null)
                    throw new Exception("(501) Intente salir y entrar del sistema para realizar esta acción.");

                if (DeptoCode > 0 && OwnerCode > 0 && !string.IsNullOrEmpty(UserRole))
                {
                    using (var db = new VacacionesRCEntities())
                    {
                        var record = db.Departments.FirstOrDefault(d => d.DeptoCode == DeptoCode && d.DeptoOwner == OwnerCode && d.UserRole == UserRole);

                        if (record != null)
                        {
                            db.Departments.Remove(record);
                            db.SaveChanges();
                        }
                    }

                    return Json(new { result = "200", message = "Rejected" });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "deptoCode:" + DeptoCode);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
        }

        private IList<SelectListItem> GetRolesDepto()
        {
            IList<SelectListItem> roles = new List<SelectListItem>
            {
                new SelectListItem() { Text="APROBADOR", Value="APROBADOR"},
                new SelectListItem() { Text="APOYO", Value="APOYO"},
                new SelectListItem() { Text="DIRECTOR", Value="DIRECTOR"}
            };
            return roles;
        }
    }
}