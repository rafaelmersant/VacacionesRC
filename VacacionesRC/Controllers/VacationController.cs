using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;
using VacacionesRC.ViewModels;

namespace VacacionesRC.Controllers
{
    public class VacationController : Controller
    {
        // GET: Vacation
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            using (var db = new VacacionesRCEntities())
            {
                List<VacationModel> vacationModels = new List<VacationModel>();
                                
                int employeeId = int.Parse(Session["employeeID"].ToString());
                Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == employeeId);

                Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);

                //Admin users
                if (Session["role"] != null && Session["role"].ToString() == "Admin")
                {
                    var vacations = db.GetVacacionesByDeptoOwner(0, DateTime.Now.Year).ToList();

                    foreach (var item in vacations)
                    {
                        vacationModels.Add(new VacationModel
                        {
                            Id = item.Id,
                            IdHash = item.IdHash,
                            EmployeeId = item.EmployeeId,
                            DeptoId = item.DeptoId.Value,
                            Status = item.Status,
                            DaysTaken = item.DaysTaken,
                            DaysAvailable = item.DaysAvailable,
                            DaysRequested = item.DaysRequested,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            ReturnDate = item.ReturnDate,
                            Note = item.Note,
                            AcceptedDate = item.AcceptedDate,
                            AcceptedBy = item.AcceptedBy,
                            RejectedDate = item.RejectedDate,
                            RejectedBy = item.RejectedBy,
                            CreatedDate = item.CreatedDate,
                            CreatedBy = item.CreatedBy,
                            EmployeeName = item.EmployeeName,
                            DeptoName = item.EmployeeDepto,
                            EmployeePosition = item.EmployeePosition,
                            Year = item.Year
                            
                        });
                    }
                }

                //Depto owner
                if (department != null)
                {
                    var vacations = db.GetVacacionesByDeptoOwner(employeeId, DateTime.Now.Year).ToList();

                    foreach (var item in vacations)
                    {
                        vacationModels.Add(new VacationModel
                        {
                            Id = item.Id,
                            IdHash = item.IdHash,
                            EmployeeId = item.EmployeeId,
                            DeptoId = item.DeptoId.Value,
                            Status = item.Status,
                            DaysTaken = item.DaysTaken,
                            DaysAvailable = item.DaysAvailable,
                            DaysRequested = item.DaysRequested,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            ReturnDate = item.ReturnDate,
                            Note = item.Note,
                            AcceptedDate = item.AcceptedDate,
                            AcceptedBy = item.AcceptedBy,
                            RejectedDate = item.RejectedDate,
                            RejectedBy = item.RejectedBy,
                            CreatedDate = item.CreatedDate,
                            CreatedBy = item.CreatedBy,
                            EmployeeName = item.EmployeeName,
                            DeptoName = item.EmployeeDepto,
                            EmployeePosition = item.EmployeePosition
                        });
                    }
                }

                //End user
                if (Session["role"] != null && Session["role"].ToString() != "Admin" && department == null)
                {
                    var vacations = db.Vacations.Where(v => v.EmployeeId == employeeId).OrderByDescending(o => o.CreatedDate).ToList();

                    foreach (var item in vacations)
                    {
                        vacationModels.Add(new VacationModel
                        {
                            Id = item.Id,
                            IdHash = item.IdHash,
                            EmployeeId = item.EmployeeId,
                            DeptoId = item.DeptoId.Value,
                            Status = item.Status,
                            DaysTaken = item.DaysTaken,
                            DaysAvailable = item.DaysAvailable,
                            DaysRequested = item.DaysRequested,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            ReturnDate = item.ReturnDate,
                            Note = item.Note,
                            AcceptedDate = item.AcceptedDate,
                            AcceptedBy = item.AcceptedBy,
                            RejectedDate = item.RejectedDate,
                            RejectedBy = item.RejectedBy,
                            CreatedDate = item.CreatedDate,
                            CreatedBy = item.CreatedBy
                            //EmployeeName = item.EmployeeName,
                            //DeptoName = item.EmployeeDepto,
                            //EmployeePosition = item.EmployeePosition
                        });
                    }
                }

                return View(vacationModels);
            }
        }

        public ActionResult Formulario(Guid? id)
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpGet]
        public JsonResult GetCorrespondingDays(int employeeId, int vacationId = 0)
        {
            EmployeeDay employeeDay;
            Vacation vacation = null;
            string status = "";

            try
            {
                employeeDay = HelperDays.GetDaysForEmployee(employeeId);

                if (vacationId > 0)
                {
                    using (var db = new VacacionesRCEntities())
                    {
                        vacation = db.Vacations.FirstOrDefault(v => v.Id == vacationId);
                        if (vacation != null)
                            status = vacation.Status.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            DateTime avaiableFrom = employeeDay.RenovationDate.Value.AddMonths(-6);

            var employeeDaySerialized = JsonConvert.SerializeObject(employeeDay);
            return new JsonResult { Data = new { result = "200", message = employeeDaySerialized, status, availableFrom = avaiableFrom.ToShortDateString()}, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult GetEmployee(int employeeId)
        {
            var employee = Helper.GetEmployee(employeeId);

            try
            {
                if (employee != null)
                {
                    using(var db = new VacacionesRCEntities())
                    {
                        int empId = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                        
                        if (Session["role"] != null && Session["role"].ToString() == "Consulta" && empId != employeeId)
                            throw new Exception("(403) Usted no esta autorizado para consultar este colaborador.");

                        if (Session["role"] != null && Session["role"].ToString() == "Depto")
                        {
                            int ownerId = int.Parse(Session["employeeID"].ToString());
                            Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == ownerId && d.DeptoCode == employee.EmployeeDeptoId);

                            if (department == null)
                                throw new Exception("(403) Usted no esta autorizado para consultar este colaborador.");
                        }
                    }
                    
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
            try
            {
                if (Session["employeeID"] == null) throw new Exception("Por favor intente hacer saliendo y entrando nuevamente al sistema.");

                using (var db = new VacacionesRCEntities())
                {
                    Vacation vacationEdit = db.Vacations.FirstOrDefault(v => v.IdHash == vacation.IdHash);

                    if (vacationEdit != null)
                    {
                        int oldTakenDays = vacationEdit.DaysRequested;

                        Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == vacation.EmployeeId);

                        vacationEdit.StartDate = vacation.StartDate;
                        vacationEdit.EndDate = vacation.EndDate;
                        vacationEdit.DeptoId = employee != null ? employee.EmployeeDeptoId : 0;
                        vacationEdit.DaysTaken = vacation.DaysTaken;
                        vacationEdit.DaysAvailable = vacation.DaysAvailable - vacation.DaysRequested;
                        vacationEdit.DaysRequested = vacation.DaysRequested;
                        vacationEdit.ReturnDate = vacation.ReturnDate;
                        vacationEdit.Note = vacation.Note;
                        vacationEdit.ModifiedDate = DateTime.Now;
                        vacationEdit.ModifiedBy = Session["employeeID"].ToString();

                        db.SaveChanges();
                        HelperDays.UpdateTakenDays(vacationEdit.EmployeeId, vacationEdit.DaysRequested, oldTakenDays);
                    }
                    else
                    {
                        Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == vacation.EmployeeId);

                        Vacation newVacation = new Vacation
                        {
                            IdHash = Guid.NewGuid(),
                            EmployeeId = vacation.EmployeeId,
                            DeptoId = employee != null ? employee.EmployeeDeptoId : 0,
                            DaysTaken = vacation.DaysTaken,
                            DaysAvailable = vacation.DaysAvailable - vacation.DaysRequested,
                            DaysRequested = vacation.DaysRequested,
                            StartDate = vacation.StartDate,
                            EndDate = vacation.EndDate,
                            ReturnDate = vacation.ReturnDate,
                            Note = vacation.Note,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Session["employeeID"].ToString(),
                            Status = "En proceso"
                        };

                        try
                        {
                            var employeeDay = HelperDays.UpdateTakenDays(vacation.EmployeeId, newVacation.DaysRequested);
                            if (employeeDay != null)
                            {
                                newVacation.Year = employeeDay.CurrentYear;
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.SendException(ex);
                        }

                        db.Vacations.Add(newVacation);
                        db.SaveChanges();

                        //Send notification to depto owner
                        try
                        {
                            Department department = db.Departments.FirstOrDefault(d => d.DeptoCode == newVacation.DeptoId);
                            if (department != null)
                            {
                                Employee ownerDepto = db.Employees.FirstOrDefault(o => o.EmployeeId == department.DeptoOwner);
                                if (ownerDepto != null)
                                {
                                    string redirectTo = ConfigurationManager.AppSettings["RedirectTo"].Replace("##vacationForm##", newVacation.IdHash.ToString());

                                    Helper.SendEmailVacationNotification(ownerDepto.Email, employee.EmployeeName,
                                        employee.EmployeePosition, newVacation.StartDate.ToString("dd/MM/yyyy"), 
                                        newVacation.EndDate.ToString("dd/MM/yyyy"), newVacation.ReturnDate.Value.ToString("dd/MM/yyyy"), redirectTo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.SendException(ex);
                        }
                    }
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "employeeId:" + vacation.EmployeeId);

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RejectRequest(int Id)
        {
            try
            {
                if (Session["employeeID"] == null)
                    throw new Exception("(501) Intente salir y entrar del sistema para realizar esta acción.");

                if (Id > 0)
                {
                    using (var db = new VacacionesRCEntities())
                    {
                        var vacation = db.Vacations.FirstOrDefault(v => v.Id == Id);

                        if (vacation != null)
                        {
                            EmployeeDay employeeDay = db.EmployeeDays.Where(d => d.EmployeeId == vacation.EmployeeId).OrderByDescending(o => o.CreatedDate).FirstOrDefault();

                            vacation.RejectedDate = DateTime.Now;
                            vacation.RejectedBy = Session["employeeID"].ToString();
                            vacation.Status = "Rechazada";

                            employeeDay.TakenDays -= vacation.DaysRequested;

                            db.SaveChanges();
                        }
                    }

                    return Json(new { result = "200", message = "Rejected" });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "vacationId:" + Id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
        }

        [HttpPost]
        public JsonResult AcceptRequest(int Id)
        {
            try
            {
                if (Session["employeeID"] == null)
                    throw new Exception("(501) Intente salir y entrar del sistema para realizar esta acción.");

                if (Id > 0)
                {
                    using (var db = new VacacionesRCEntities())
                    {
                        var vacation = db.Vacations.FirstOrDefault(v => v.Id == Id);

                        if (vacation != null)
                        {
                            vacation.AcceptedDate = DateTime.Now;
                            vacation.AcceptedBy = Session["employeeID"].ToString();
                            vacation.Status = "Aprobada";
                            vacation.DaysTaken = vacation.DaysRequested;
                            db.SaveChanges();
                        }
                    }

                    return Json(new { result = "200", message = "Rejected" });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "vacationId:" + Id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
        }

        [HttpPost]
        public JsonResult GetVacation(string idHash)
        {
            Vacation vacation = null;

            try
            {
                using(var db = new VacacionesRCEntities())
                {
                    Guid IdHash = Guid.Parse(idHash);
                    vacation = db.Vacations.FirstOrDefault(v => v.IdHash == IdHash);

                    if (vacation != null)
                    {
                        var vacationSerialized = JsonConvert.SerializeObject(vacation);
                        return new JsonResult { Data = new { result = "200", message = vacationSerialized }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
        public JsonResult PrintForm(Guid IdHash, string FechaSolicitud, string Codigo, string Nombre,
                                       string FechaIngreso, string Puesto, string Departamento,
                                       string DiasCorrespondientes, string DiasRestantes, string DiasSolicitados,
                                       string FechaDesde, string FechaHasta, string FechaRetorno, string Observacion)
        {
            try
            {
                using(var db = new VacacionesRCEntities())
                {
                    Vacation vacation = db.Vacations.FirstOrDefault(v => v.IdHash == IdHash);
                    if (vacation != null)
                    {
                        FechaSolicitud = String.Format("{0}", vacation.CreatedDate.ToString("dd/MM/yyyy"));
                        FechaRetorno = String.Format("{0}", vacation.ReturnDate.Value.ToString("dd/MM/yyyy"));
                    }

                    string urlServer = Request.Url.Authority;
                        
                    string content = Helper.ShowVacationForm(FechaSolicitud, Codigo, Nombre, FechaIngreso, Puesto, Departamento,
                    DiasCorrespondientes, DiasRestantes, DiasSolicitados, FechaDesde, FechaHasta, FechaRetorno, Observacion, urlServer);

                    return new JsonResult { Data = new { result = "200", message = content }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult PrintConstancia(Guid IdHash, int Codigo, string Nombre, string FechaSolicitud,
                                      string FechaIngreso, string Puesto, string Departamento, string FechaDesde, string FechaHasta)
        {
            try
            {
                string Cedula = "";
                string TiempoTrabajando = "";
                string SalarioMensual = "";
                string Localidad = "";
                string CuentaBanco = "";
                string MontoPagado = "";
                string FirmadoPor = "";
                string DiasPagados = "";

                using (var db = new VacacionesRCEntities())
                {
                    Vacation vacation = db.Vacations.FirstOrDefault(v => v.IdHash == IdHash);
                    if (vacation != null)
                    {
                        FechaSolicitud = String.Format("{0}", vacation.CreatedDate.ToString("dd/MM/yyyy"));
                    }

                    Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == Codigo);
                    if (employee != null)
                    {
                        var rules = db.Rules.ToList();
                        DiasPagados = rules.FirstOrDefault(r => r.Id == 1).Value;
                        FirmadoPor = rules.FirstOrDefault(r => r.Id == 2).Value;

                        //Elapsed Time
                        //int years, months, days, hours, minutes, seconds, milliseconds;
                        Helper.GetElapsedTime(employee.AdmissionDate.Value, DateTime.Today, out int years, out int months, out int days, out int hours, out int minutes, out int seconds, out int milliseconds);

                        if (years > 0)
                            TiempoTrabajando += years + " años ";
                        if (months > 0)
                            TiempoTrabajando += months + " meses ";
                        if (months == 0 && days > 0)
                            TiempoTrabajando += " y " + days + " días";

                        if (years > 5)
                            DiasPagados = "18";

                        Cedula = employee.Identification;
                        SalarioMensual = "RD" + string.Format("{0:c}", employee.Salary);
                        Localidad = employee.Location;
                        CuentaBanco = employee.BankAccount;

                        decimal montoPagado = employee.Salary.Value / 23.83M;
                        montoPagado = montoPagado * int.Parse(DiasPagados);
                        MontoPagado = "RD" + string.Format("{0:c}", montoPagado);
                    }

                    string urlServer = Request.Url.Authority;

                    string content = Helper.ShowConstancia(FechaSolicitud, Codigo.ToString(), Nombre, FechaIngreso, Puesto, Departamento,
                                                            FechaDesde, FechaHasta, Cedula, TiempoTrabajando, SalarioMensual, Localidad, 
                                                            CuentaBanco, MontoPagado, FirmadoPor, DiasPagados, urlServer);

                    return new JsonResult { Data = new { result = "200", message = content }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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