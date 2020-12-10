using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;

namespace VacacionesRC.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            using (var db = new VacacionesRCEntities())
            {
                var notifications = db.Notifications.OrderByDescending(o => o.CreatedDate).ToList();

                return View(notifications);
            }
        }

        [HttpPost]
        public JsonResult SendNotification(string receiverEmail, string message)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Helper.SendRawEmail(receiverEmail, "Notificación Sistema de Vacaciones", message);

                    Notification notification = new Notification
                    {
                        ReceiverEmail = receiverEmail,
                        Message = message,
                        SenderEmail = Session["email"].ToString(),
                        CreatedDate = DateTime.Now
                    };

                    db.Notifications.Add(notification);
                    db.SaveChanges();
                }

                return Json(new { result = "200", message = "Sent" });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "receiverEmail:" + receiverEmail + "| message:" + message);

                return Json(new { result = "500", message = ex.Message });
            }
        }
    }
}