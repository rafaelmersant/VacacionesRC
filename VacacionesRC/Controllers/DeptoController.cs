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

            using (var db = new VacacionesRCEntities())
            {
                List<DeptoModel> deptoModels = new List<DeptoModel>();

                var deptos = db.GetDeptos();

                foreach(var item in deptos)
                {
                    deptoModels.Add(new DeptoModel
                    {
                        DeptoCode = item.DeptoCode,
                        DeptoName = item.DeptoName,
                        OwnerId = item.DeptoOwner ?? 0,
                        OwnerName = item.EmployeeName
                    });
                }

                return View(deptoModels);
            }
        }

        [HttpPost]
        public JsonResult Save(int DeptoCode, string DeptoName, int OwnerDeptoId)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Department department = db.Departments.FirstOrDefault(d => d.DeptoCode == DeptoCode);

                    if (department != null)
                    {
                        department.DeptoName = DeptoName;
                        department.DeptoOwner = OwnerDeptoId;
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
    }
}