using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacacionesRC.App_Start;
using VacacionesRC.Models;
using VacacionesRC.ViewModels;
using static VacacionesRC.App_Start.HelperPayroll;

namespace VacacionesRC.Controllers
{
    public class VacationController : Controller
    {
        // GET: Vacation
        public ActionResult Index(int id = 0)
        {
            int _year = id; //== 0 ? DateTime.Today.Year : id;

            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    List<VacationModel> vacationModels = new List<VacationModel>();

                    int employeeId = int.Parse(Session["employeeID"].ToString());
                    Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == employeeId);

                    //Employee employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.Type == "I");

                    //Admin users
                    if (Session["role"] != null && Session["role"].ToString() == "Admin")
                    {
                        var vacations = db.GetVacacionesByDeptoOwner(0, _year).ToList();

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
                    if (department != null && vacationModels.Count() == 0)
                    {
                        var vacations = db.GetVacacionesByDeptoOwner(employeeId, _year).ToList();

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

                    //End user
                    if (Session["role"] != null && Session["role"].ToString() != "Admin" && department == null)
                    {
                        var vacations = db.Vacations.Where(v => v.EmployeeId == employeeId && (v.CreatedDate.Year == _year || _year == 0)).OrderByDescending(o => o.CreatedDate).ToList();

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
                                Year = item.Year
                                //EmployeeName = item.EmployeeName,
                                //DeptoName = item.EmployeeDepto,
                                //EmployeePosition = item.EmployeePosition
                            });
                        }
                    }

                    ViewBag.Years = Helper.GetYears();
                    
                    return View(vacationModels);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult Formulario(Guid? id)
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            return View();
        }

        public ActionResult Suspender(Guid? id)
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

                DateTime avaiableFrom = employeeDay.RenovationDate.Value.AddMonths(-6);
                DateTime? period = HelperPayroll.GetPayrollPeriodByRenovationDate(employeeDay.RenovationDate.Value);
                string cycle_period = period != null ? period.Value.ToShortDateString() : "";

                var employeeDaySerialized = JsonConvert.SerializeObject(employeeDay);

                return new JsonResult
                {
                    Data = new
                    {
                        result = "200",
                        message = employeeDaySerialized,
                        status,
                        availableFrom = avaiableFrom.ToShortDateString(),
                        previousConstancia = cycle_period
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult GetEmployee(int employeeId)
        {
            var employee = Helper.GetEmployee(employeeId);

            try
            {
                if (employee != null && employee.Type == "I")
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
            string _detail_ = JsonConvert.SerializeObject(vacation);

            try
            {
                if (Session["employeeID"] == null) throw new Exception("Por favor intente saliendo y entrando nuevamente al sistema (su sesión expiró)");

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

                        try
                        {
                            var employeeDay = HelperDays.UpdateTakenDays(vacationEdit.EmployeeId, vacationEdit.DaysRequested, oldTakenDays, vacationEdit.EndDate);
                            if (employeeDay != null)
                            {
                                vacationEdit.Year = employeeDay.CurrentYear;
                            }
                            else
                            {
                                //Check if the person is trying to request more than 7 days and the renovation date is not reached yet
                                throw new Exception("No puede exceder la cantidad de 7 días antes de la fecha de renovación de las vacaciones.");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("exceder"))
                                throw ex;

                            Helper.SendException(ex, "UpdatingTakenDates -- details:" + _detail_);
                        }

                        db.SaveChanges();
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
                            var employeeDay = HelperDays.UpdateTakenDays(vacation.EmployeeId, newVacation.DaysRequested, endDate: newVacation.EndDate);
                            if (employeeDay != null)
                            {
                                newVacation.Year = employeeDay.CurrentYear;
                            }
                            else
                            {
                                //Check if the person is trying to request more than 7 days and the renovation date is not reached yet
                                throw new Exception("No puede exceder la cantidad de 7 días antes de la fecha de renovación de las vacaciones");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("exceder"))
                                throw ex;

                            Helper.SendException(ex, "(2) Updating TakenDays -- details:" + _detail_);
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
                            Helper.SendException(ex, "SendEmailVacationNotificacion -- details:" + _detail_);
                        }
                    }
                }

                return Json(new { result = "200", message = "success" });
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("exceder") && !ex.Message.Contains("intente"))
                    Helper.SendException(ex, "details:" + _detail_);

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SuspendVacation(string vacationId, string suspendDate, string suspendReason)
        {
            try
            {
                if (Session["employeeID"] == null)
                    throw new Exception("(501) Intente salir y entrar del sistema para realizar esta acción.");

                if (!string.IsNullOrEmpty(vacationId))
                {
                    using (var db = new VacacionesRCEntities())
                    {
                        var _suspendDate = DateTime.Parse(suspendDate);
                        
                        var id = Guid.Parse(vacationId);
                        var vacation = db.Vacations.FirstOrDefault(v => v.IdHash == id);

                        if (vacation != null)
                        {
                            var suspendedDays = (vacation.EndDate - _suspendDate).Days;

                            //Update current vacation record
                            vacation.ModifiedDate = DateTime.Now;
                            vacation.ModifiedBy = Session["employeeID"].ToString();
                            vacation.DaysTaken = vacation.DaysTaken - suspendedDays;
                            vacation.DaysRequested = vacation.DaysRequested - suspendedDays;
                            vacation.DaysAvailable = vacation.DaysAvailable + suspendedDays;
                            vacation.EndDate = _suspendDate;
                            db.SaveChanges();

                            //Suspend vacation
                            var suspendVacation = new Vacation
                            {
                                IdHash = Guid.NewGuid(),
                                EmployeeId = vacation.EmployeeId,
                                DeptoId = vacation.DeptoId,
                                Status = "Suspendida",
                                DaysTaken = 0,
                                DaysAvailable = 0,
                                DaysRequested = suspendedDays,
                                StartDate = _suspendDate,
                                EndDate = vacation.EndDate,
                                ReturnDate = vacation.ReturnDate,
                                Note = suspendReason,
                                Year = vacation.Year,
                                CreatedDate = DateTime.Now,
                                CreatedBy = Session["employeeID"].ToString(),
                            };
                            db.Vacations.Add(suspendVacation);
                            db.SaveChanges();

                            //Rollback days taken
                            var employeeDays = db.EmployeeDays.FirstOrDefault(d => d.EmployeeId == vacation.EmployeeId && d.CurrentYear == vacation.Year);
                            employeeDays.TakenDays = employeeDays.TakenDays - suspendedDays;
                            db.SaveChanges();
                        }
                    }

                    return Json(new { result = "200", message = "Suspended" });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "vacationId:" + vacationId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
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
                Helper.SendException(ex, "vacationIDHASH:" + idHash);
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
                Helper.SendException(ex, "vacationIDHASH:" + IdHash);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult PrintConstancia
            (
            Guid IdHash, 
            int Codigo, 
            string Nombre, 
            string FechaSolicitud,
            string FechaIngreso, 
            string Puesto, 
            string Departamento, 
            string FechaDesde, 
            string FechaHasta, 
            string FechaRetorno, 
            string DiasSolicitados
            )
        {
            try
            {
                string Cedula = "";
                string TiempoTrabajando = "";
                string SalarioMensual = "";
                string Localidad = "";
                string CuentaBanco = "";
                string MontoPagado = "";
                string DiasPagados = "";

                //format days requested
                DiasSolicitados = int.Parse(DiasSolicitados) > 1 ? (DiasSolicitados + " días") : (DiasSolicitados + " día");

                using (var db = new VacacionesRCEntities())
                {
                    Vacation vacation = db.Vacations.FirstOrDefault(v => v.IdHash == IdHash);
                    if (vacation != null)
                    {
                        FechaSolicitud = String.Format("{0}", vacation.CreatedDate.ToString("dd/MM/yyyy"));
                    }

                    Employee employee = Helper.GetEmployee(Codigo, true);
                    if (employee != null)
                    {
                        var rules = db.Rules.ToList();
                        DiasPagados = rules.FirstOrDefault(r => r.Id == 1).Value;
                        //FirmadoPor = rules.FirstOrDefault(r => r.Id == 2).Value;

                        //Elapsed Time
                        //int years, months, days, hours, minutes, seconds, milliseconds;
                        Helper.GetElapsedTime(employee.AdmissionDate.Value, vacation.EndDate, out int years, out int months, out int days, out int hours, out int minutes, out int seconds, out int milliseconds);

                        if (years > 0)
                            TiempoTrabajando += years + (years > 1 ? " años " : " año ");
                        if (months > 0)
                            TiempoTrabajando += months + (months > 1 ? " meses " : " mes ");
                        if (months == 0 && days > 0)
                            TiempoTrabajando += " y " + days + (days > 1 ? " días" : " día");

                        DiasPagados = years >= 5 ? "18" : "14"; // rules.FirstOrDefault(r => r.Id == 2).Value;

                        Cedula = employee.Identification;
                        SalarioMensual = "RD" + string.Format("{0:c}", employee.Salary);
                        Localidad = employee.Location;
                        CuentaBanco = employee.BankAccount;

                        //Get Constancia Monto
                        string environmentVACACIONES = ConfigurationManager.AppSettings["EnvironmentVacaciones"];
                        if (environmentVACACIONES != "DEV")
                        {
                            string cycle = HelperPayroll.GetPayrollPeriodByAdmissionDate(employee.AdmissionDate.Value);
                            DataSet payroll = Helper.GetPayrollDetailForEmployee(vacation.EmployeeId.ToString(), cycle, "N"); //paytype = CETIPOPAGO
                            PayrollDetailHeader? payrollDetail = HelperPayroll.GetPayrollDetail(payroll);
                            if (payrollDetail != null)
                                MontoPagado = "RD" + string.Format("{0:c}", payrollDetail.Value.total);
                            else
                                throw new Exception("No puede imprimir la constancia porque el pago de vacaciones no se encontró presente para el periodo correspondiente.");
                        } 
                    }

                    string urlServer = Request.Url.Authority;

                    string content = Helper.ShowConstancia(FechaSolicitud, Codigo.ToString(), Nombre, FechaIngreso, Puesto, Departamento,
                                                            FechaDesde, FechaHasta, Cedula, TiempoTrabajando, SalarioMensual, Localidad, 
                                                            CuentaBanco, MontoPagado, "", DiasPagados, FechaRetorno, DiasSolicitados, urlServer);

                    return new JsonResult { Data = new { result = "200", message = content }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("correspondiente"))
                    Helper.SendException(ex, "vacationIDHASH:" + IdHash);

                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
    }
}