using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    }
}