using Newtonsoft.Json;
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
    public class ReportesController : Controller
    {
        // GET: Reportes
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin" 
                && !Session["role"].ToString().Contains("APROBADOR")
                && !Session["role"].ToString().Contains("APOYO")
                && !Session["role"].ToString().Contains("DIRECTOR")) return RedirectToAction("Index", "Home");

            try
            {
                ViewBag.EnVacaciones = GetEnVacaciones().Count();
                ViewBag.VacacionesPendientes = GetVacacionesPendientes().Count();
                ViewBag.VacacionesSolicitadas = GetVacacionesSolicitadas().Count();
                ViewBag.VacacionesVencidas = GetVacacionesVencidas().Count();
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult EnVacaciones()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin"
                && !Session["role"].ToString().Contains("APROBADOR")
                && !Session["role"].ToString().Contains("APOYO")
                && !Session["role"].ToString().Contains("DIRECTOR")) return RedirectToAction("Index", "Home");

            List<EmployeeOnVacationModel> employees = GetEnVacaciones();

            return View(employees);
        }

        public ActionResult VacacionesPendientes()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin"
                && !Session["role"].ToString().Contains("APROBADOR")
                && !Session["role"].ToString().Contains("APOYO")
                && !Session["role"].ToString().Contains("DIRECTOR")) return RedirectToAction("Index", "Home");

            List<EmployeePendingVacationModel> employees = GetVacacionesPendientes();

            return View(employees);
        }

        public ActionResult VacacionesSolicitadas()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin"
                && !Session["role"].ToString().Contains("APROBADOR")
                && !Session["role"].ToString().Contains("APOYO")
                && !Session["role"].ToString().Contains("DIRECTOR")) return RedirectToAction("Index", "Home");

            List<EmployeeOnVacationModel> employees = GetVacacionesSolicitadas();

            return View(employees);
        }

        public ActionResult VacacionesVencidas()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin"
                && !Session["role"].ToString().Contains("APROBADOR")
                && !Session["role"].ToString().Contains("APOYO")
                && !Session["role"].ToString().Contains("DIRECTOR")) return RedirectToAction("Index", "Home");

            List<EmployeePendingVacationModel> employees = GetVacacionesVencidas();
            
            return View(employees);
        }

        public ActionResult ReporteGeneral(int year = 0, string location = "", int department = 0, string nombre = "")
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    List<VacationModel> vacationModels = new List<VacationModel>();

                    //Admin users
                    if (Session["role"] != null && (Session["role"].ToString() == "Admin" || Session["role"].ToString() == "AdminConsulta"))
                        vacationModels = GetReportGeneralData(year, location, department, nombre);
                    
                    ViewBag.Years = Helper.GetYears();
                    ViewBag.Departments = Helper.GetDepartments();

                    ViewBag.Year = year;
                    ViewBag.department = department;
                    ViewBag.location = location.Replace(" ","_");
                    ViewBag.nombre = nombre;

                    return View(vacationModels);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        [HttpPost]
        public string GetReportGeneralDataToExcel(int year = 0, string location = "", int department = 0, string nombre = "")
        {
            string detailed = "<table width='100%' border=1>";
            var data = GetReportGeneralData(year, location, department, nombre);

            //HEADER for table
            detailed += "<tr>";
            detailed += "<td><b>Fecha Solicitud</b></td>";
            detailed += "<td><b>Codigo</b></td>";
            detailed += "<td><b>Nombre</b></td>";
            detailed += "<td><b>Puesto</b></td>";
            detailed += "<td><b>Departamento</b></td>";
            detailed += "<td><b>Localidad</b></td>";
            detailed += "<td><b>Fecha Inicio</b></td>";
            detailed += "<td><b>Fecha Fin</b></td>";
            detailed += "<td><b>Dias Solicitados</b></td>";
            detailed += "<td><b>Fecha Regreso</b></td>";
            detailed += "<td><b>Cumplidas en el Año</b></td>";
            detailed += "<td><b>Estatus</b></td>";

            foreach (var item in data)
            {
                detailed += "</tr>";

                detailed += "<td>" + item.CreatedDate.ToString("dd/MM/yyyy") + "</td>";
                detailed += "<td>" + item.EmployeeId + "</td>";
                detailed += "<td>" + item.EmployeeName + "</td>";
                detailed += "<td>" + item.EmployeePosition + "</td>";
                detailed += "<td>" + item.DeptoName + "</td>";
                detailed += "<td>" + item.Location + "</td>";
                detailed += "<td>" + item.StartDate.ToString("dd/MM/yyyy") + "</td>";
                detailed += "<td>" + item.EndDate.ToString("dd/MM/yyyy") + "</td>";
                detailed += "<td>" + item.DaysRequested + "</td>";
                detailed += "<td>" + item.ReturnDate?.ToString("dd/MM/yyyy") + "</td>";
                detailed += "<td>" + item.Year + "</td>";
                detailed += "<td>" + item.Status + "</td>";
                
                detailed += "</tr>";
            }

            detailed += "</table>";

            return detailed;
        }

        public List<VacationModel> GetReportGeneralData(int year = 0, string location = "", int department = 0, string nombre = "")
        {
            List<VacationModel> vacationModels = new List<VacationModel>();
            location = location.Replace("_", " ");

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var vacations = db.GetVacacionesReporteGeneral(year, location, department, nombre).ToList();

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
                            Year = item.Year,
                            Location = item.Location
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return vacationModels;
        }

        [HttpPost]
        public JsonResult GetEmployeeOnVacation()
        {
            try
            {
                var employees = GetEnVacaciones();

                string detailed = "<table width='100%' border=1>";

                //HEADER for table
                detailed += "<tr>";
                detailed += "<td><b>Código</b></td>";
                detailed += "<td><b>Nombre</b></td>";
                detailed += "<td><b>Departamento</b></td>";
                detailed += "<td><b>Localidad</b></td>";
                detailed += "<td><b>Días en Disfrute</b></td>";
                detailed += "<td><b>Fecha Inicio</b></td>";
                detailed += "<td><b>Fecha Fin</b></td>";
                detailed += "<td><b>Fecha Retorno</b></td>";
                detailed += "<td><b>Días Disponibles</b></td>";
                detailed += "<td><b>Cumplidas en el Año</b></td>";

                foreach (var employee in employees)
                {
                    detailed += "<tr>";
                    detailed += "<td>" + employee.EmployeeId + "</td>";
                    detailed += "<td>" + employee.EmployeeName + "</td>";
                    detailed += "<td>" + employee.EmployeeDepto + "</td>";
                    detailed += "<td>" + employee.EmployeeLocation + "</td>";    
                    detailed += "<td>" + employee.DaysTaken + "</td>";
                    detailed += "<td>" + employee.StartDate.ToString("dd/MM/yyyy") + " </td>";
                    detailed += "<td>" + employee.EndDate.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.ReturnDate.Value.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.DaysAvailable + "</td>";
                    detailed += "<td>" + employee.Year + "</td>";
                    detailed += "</tr>";
                }

                detailed += "</table>";

                return new JsonResult { Data = new { result = "200", message = detailed }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult GetEmployeeVacationRequested()
        {
            try
            {
                var employees = GetVacacionesSolicitadas();

                string detailed = "<table width='100%' border=1>";

                //HEADER for table
                detailed += "<tr>";
                detailed += "<td><b>Código</b></td>";
                detailed += "<td><b>Nombre</b></td>";
                detailed += "<td><b>Departamento</b></td>";
                detailed += "<td><b>Localidad</b></td>";
                detailed += "<td><b>Días Solicitados</b></td>";
                detailed += "<td><b>Días Disponibles</b></td>";
                detailed += "<td><b>Fecha Inicio</b></td>";
                detailed += "<td><b>Fecha Fin</b></td>";
                detailed += "<td><b>Fecha Retorno</b></td>";
                detailed += "<td><b>Cumplidas en el Año</b></td>";
                detailed += "<td><b>Estatus</b></td>";

                foreach (var employee in employees)
                {
                    detailed += "<tr>";
                    detailed += "<td>" + employee.EmployeeId + "</td>";
                    detailed += "<td>" + employee.EmployeeName + "</td>";
                    detailed += "<td>" + employee.EmployeeDepto + "</td>";
                    detailed += "<td>" + employee.EmployeeLocation + "</td>";
                    detailed += "<td>" + employee.DaysTaken + "</td>";
                    detailed += "<td>" + employee.DaysAvailable + "</td>";
                    detailed += "<td>" + employee.StartDate.ToString("dd/MM/yyyy") + " </td>";
                    detailed += "<td>" + employee.EndDate.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.ReturnDate.Value.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.Year + "</td>";
                    detailed += "<td>" + employee.Status + "</td>";
                    detailed += "</tr>";
                }

                detailed += "</table>";

                return new JsonResult { Data = new { result = "200", message = detailed }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult GetEmployeePendingVacation(bool vencidas = false)
        {
            try
            {
                var employees = GetVacacionesPendientes();

                if (vencidas)
                    employees = GetVacacionesVencidas();

                string detailed = "<table width='100%' border=1>";

                //HEADER for table
                detailed += "<tr>";
                detailed += "<td><b>Código</b></td>";
                detailed += "<td><b>Nombre</b></td>";
                detailed += "<td><b>Departamento</b></td>";
                detailed += "<td><b>Localidad</b></td>";
                detailed += "<td><b>Fecha de Ingreso</b></td>";
                detailed += "<td><b>Fecha de Renovación</b></td>";
                detailed += "<td><b>Fecha de Vencimiento</b></td>";

                if (!vencidas)
                    detailed += "<td><b>Días restantes a vencer</b></td>";

                if (vencidas)
                    detailed += "<td><b>Días vencidos</b></td>";
                else
                    detailed += "<td><b>Días disponibles a vencer</b></td>";

                detailed += "<td><b>Días solicitados</b></td>";
                
                foreach (var employee in employees)
                {
                    detailed += "<tr>";
                    detailed += "<td>" + employee.EmployeeId + "</td>";
                    detailed += "<td>" + employee.EmployeeName + "</td>";
                    detailed += "<td>" + employee.EmployeeDepto + "</td>";
                    detailed += "<td>" + employee.EmployeeLocation + "</td>";
                    detailed += "<td>" + employee.AdmissionDate.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.RenovationDate.ToString("dd/MM/yyyy") + "</td>";
                    detailed += "<td>" + employee.DueVacationDate.ToString("dd/MM/yyyy") + "</td>";

                    if (!vencidas)
                        detailed += "<td>" + employee.DaysToDueVacation + " </td>";

                    detailed += "<td>" + employee.DaysAvailable + " </td>";
                    detailed += "<td>" + employee.DaysRequested + "</td>";
                    detailed += "</tr>";
                }

                detailed += "</table>";

                return new JsonResult { Data = new { result = "200", message = detailed }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult UpdateAllEmployees()
        {
            try
            {
                Helper.UpdateWithAllEmployeesFromAS400();

                return new JsonResult { Data = new { result = "200", message = "success" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
                return new JsonResult { Data = new { result = "500", message = ex.Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        private List<EmployeeOnVacationModel> GetEnVacaciones()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    
                    List<EmployeeOnVacationModel> employees = new List<EmployeeOnVacationModel>();

                    var _employees = db.GetEmployeeOnVacation(deptoOwner).ToList();

                    foreach (var employee in _employees)
                    {
                        employees.Add(new EmployeeOnVacationModel
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.EmployeeName,
                            EmployeeDepto = employee.EmployeeDepto,
                            EmployeeLocation = employee.Location,
                            Year = employee.Year,
                            DaysAvailable = employee.DaysAvailable,
                            DaysTaken = employee.DaysTaken,
                            StartDate = employee.StartDate,
                            EndDate = employee.EndDate,
                            ReturnDate = employee.ReturnDate
                        });
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        private List<EmployeeOnVacationModel> GetVacacionesSolicitadas()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    
                    List<EmployeeOnVacationModel> employees = new List<EmployeeOnVacationModel>();

                    var _employees = db.GetEmployeeVacationRequested(deptoOwner).ToList();

                    foreach (var employee in _employees)
                    {
                        employees.Add(new EmployeeOnVacationModel
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.EmployeeName,
                            EmployeeDepto = employee.EmployeeDepto,
                            EmployeeLocation = employee.Location,
                            Year = employee.Year,
                            DaysAvailable = employee.DaysAvailable,
                            DaysTaken = employee.DaysTaken,
                            StartDate = employee.StartDate,
                            EndDate = employee.EndDate,
                            ReturnDate = employee.ReturnDate,
                            Status = employee.Status
                        });
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        private List<EmployeePendingVacationModel> GetVacacionesPendientesOLD()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    //var _employees = db.GetEmployeePendingVacation().ToList();
                    var _employees = db.GetEmployeePendingVacation(deptoOwner).Where(f => f.daysToDueVacation.HasValue && f.daysToDueVacation.Value > 0).ToList();

                    foreach (var employee in _employees)
                    {
                        employees.Add(new EmployeePendingVacationModel
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.EmployeeName,
                            EmployeeDepto = employee.EmployeeDepto,
                            EmployeePosition = employee.EmployeePosition,
                            EmployeeLocation = employee.Location,
                            AdmissionDate = employee.AdmissionDate.Value,
                            RenovationDate = employee.RenovationDate.Value,
                            DueVacationDate = employee.dueVacationDate.Value,
                            DaysToDueVacation = employee.daysToDueVacation.Value < 0 ? 0 : employee.daysToDueVacation.Value,
                            TimeInCompany = employee.TimeInCompany ?? 0,
                            DaysRequested = employee.daysRequested,
                            DaysAvailable = employee.daysAvailable ?? 0
                        });
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        private List<EmployeePendingVacationModel> GetVacacionesPendientes()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var _employees = db.Employees.Where(e => e.TerminateDate == null && e.Type == "I").ToList();

                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    int deptoCode = int.Parse(Session["depto"].ToString());

                    if (Session["role"].ToString() != "Admin")
                        _employees = _employees.Where(e => e.EmployeeDeptoId == deptoCode).ToList();

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    VacationController vacationController = new VacationController();

                    foreach (var employee in _employees)
                    {
                        try
                        {
                            //Skip it for employee with less than a year in the company
                            if ((DateTime.Today - employee.AdmissionDate).Value.TotalDays <= 366) continue;

                            JsonResult result = vacationController.GetCorrespondingDays(employee.EmployeeId);
                            dynamic data = result.Data;
                            EmployeeDay _employeesDay = JsonConvert.DeserializeObject<EmployeeDay>(data.message);

                            _employeesDay.DueDate = _employeesDay?.DueDate.Value.AddYears(-1);
                            _employeesDay.RenovationDate = _employeesDay?.RenovationDate.Value.AddYears(-1);

                            //If this person has a exception, then add 6 more months to allow to take the vacations
                            var exception = db.ExceptionsVacations.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId && e.Year == _employeesDay.CurrentYear);
                            _employeesDay.DueDate = exception != null ? _employeesDay.DueDate.Value.AddMonths(6) : _employeesDay.DueDate;

                            int daysToDueVacation = (_employeesDay.DueDate.Value - DateTime.Today).Days;
                            int daysAvailable = _employeesDay.TotalDays - _employeesDay.TakenDays.Value;

                            int daysOld = 90;

                            if ((daysToDueVacation > 0 && daysToDueVacation < daysOld) && daysAvailable > 0)
                            {
                                employees.Add(new EmployeePendingVacationModel
                                {
                                    EmployeeId = employee.EmployeeId,
                                    EmployeeName = employee.EmployeeName,
                                    EmployeeDepto = employee.EmployeeDepto,
                                    EmployeePosition = employee.EmployeePosition,
                                    EmployeeLocation = employee.Location,
                                    AdmissionDate = employee.AdmissionDate.Value,
                                    RenovationDate = _employeesDay.RenovationDate.Value,
                                    DueVacationDate = _employeesDay.DueDate.Value,
                                    DaysToDueVacation = daysToDueVacation,
                                    TimeInCompany = (DateTime.Today - employee.AdmissionDate).Value.Days,
                                    DaysRequested = _employeesDay.TakenDays.Value,
                                    DaysAvailable = daysAvailable
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.SendException(ex);
                        }
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        private List<EmployeePendingVacationModel> GetVacacionesVencidas()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var _employees = db.Employees.Where(e => e.TerminateDate == null && e.Type == "I").ToList();

                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    int deptoCode = int.Parse(Session["depto"].ToString());

                    if (Session["role"].ToString() != "Admin")
                        _employees = _employees.Where(e => e.EmployeeDeptoId == deptoCode).ToList();

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    VacationController vacationController = new VacationController();

                    foreach (var employee in _employees)
                    {
                        try
                        {
                            //Skip it for employee with less than a year in the company
                            if ((DateTime.Today - employee.AdmissionDate).Value.TotalDays <= 366) continue;
                            
                            JsonResult result = vacationController.GetCorrespondingDays(employee.EmployeeId);
                            dynamic data = result.Data;
                            EmployeeDay _employeesDay = JsonConvert.DeserializeObject<EmployeeDay>(data.message);

                            _employeesDay.DueDate = _employeesDay?.DueDate.Value.AddYears(-1);
                            _employeesDay.RenovationDate = _employeesDay?.RenovationDate.Value.AddYears(-1);

                            //If this person has a exception, then add 6 more months to allow to take the vacations
                            var exception = db.ExceptionsVacations.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId && e.Year == _employeesDay.CurrentYear);
                            _employeesDay.DueDate = exception != null ? _employeesDay.DueDate.Value.AddMonths(6) : _employeesDay.DueDate;

                            int daysToDueVacation = (_employeesDay.DueDate.Value - DateTime.Today).Days;
                            int daysAvailable = _employeesDay.TotalDays - _employeesDay.TakenDays.Value;

                            if (daysToDueVacation <= 0 && daysAvailable > 0)
                            {
                                employees.Add(new EmployeePendingVacationModel
                                {
                                    EmployeeId = employee.EmployeeId,
                                    EmployeeName = employee.EmployeeName,
                                    EmployeeDepto = employee.EmployeeDepto,
                                    EmployeePosition = employee.EmployeePosition,
                                    EmployeeLocation = employee.Location,
                                    AdmissionDate = employee.AdmissionDate.Value,
                                    RenovationDate = _employeesDay.RenovationDate.Value,
                                    DueVacationDate = _employeesDay.DueDate.Value,
                                    DaysToDueVacation = daysToDueVacation,
                                    TimeInCompany = (DateTime.Today - employee.AdmissionDate).Value.Days,
                                    DaysRequested = _employeesDay.TakenDays.Value,
                                    DaysAvailable = daysAvailable
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.SendException(ex);
                        }
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }

        private List<EmployeePendingVacationModel> GetVacacionesVencidasOLD()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    var _employees = db.GetEmployeePendingVacation(deptoOwner).Where(f => !f.daysToDueVacation.HasValue || f.daysToDueVacation.Value < 0).ToList();

                    foreach (var employee in _employees)
                    {
                        employees.Add(new EmployeePendingVacationModel
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.EmployeeName,
                            EmployeeDepto = employee.EmployeeDepto,
                            EmployeePosition = employee.EmployeePosition,
                            EmployeeLocation = employee.Location,
                            AdmissionDate = employee.AdmissionDate.Value,
                            RenovationDate = employee.RenovationDate.Value,
                            DueVacationDate = employee.dueVacationDate.Value,
                            DaysToDueVacation = employee.daysToDueVacation.Value < 0 ? 0 : employee.daysToDueVacation.Value,
                            TimeInCompany = employee.TimeInCompany ?? 0,
                            DaysRequested = employee.daysRequested,
                            DaysAvailable = employee.daysAvailable ?? 0
                        });
                    }

                    return employees;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return null;
            }
        }
    }
}