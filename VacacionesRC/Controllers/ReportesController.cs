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
            if (Session["role"].ToString() != "Admin" && Session["role"].ToString() != "Depto") return RedirectToAction("Index", "Home");

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
            if (Session["role"].ToString() != "Admin" && Session["role"].ToString() != "Depto") return RedirectToAction("Index", "Home");

            List<EmployeeOnVacationModel> employees = GetEnVacaciones();

            return View(employees);
        }

        public ActionResult VacacionesPendientes()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin" && Session["role"].ToString() != "Depto") return RedirectToAction("Index", "Home");

            List<EmployeePendingVacationModel> employees = GetVacacionesPendientes();

            return View(employees);
        }

        public ActionResult VacacionesSolicitadas()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin" && Session["role"].ToString() != "Depto") return RedirectToAction("Index", "Home");

            List<EmployeeOnVacationModel> employees = GetVacacionesSolicitadas();

            return View(employees);
        }

        public ActionResult VacacionesVencidas()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin" && Session["role"].ToString() != "Depto") return RedirectToAction("Index", "Home");

            List<EmployeePendingVacationModel> employees = GetVacacionesVencidas();
            
            return View(employees);
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
                detailed += "<td><b>Año</b></td>";

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
                detailed += "<td><b>Año</b></td>";
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
                    int deptoId = 0;

                    List<EmployeeOnVacationModel> employees = new List<EmployeeOnVacationModel>();

                    Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == deptoOwner);
                    if (department != null && Session["role"].ToString() != "Admin")
                        deptoId = department.DeptoCode;

                    var _employees = db.GetEmployeeOnVacation(deptoId).ToList();

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
                    int deptoId = 0;

                    List<EmployeeOnVacationModel> employees = new List<EmployeeOnVacationModel>();

                    Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == deptoOwner);
                    if (department != null && Session["role"].ToString() != "Admin")
                        deptoId = department.DeptoCode;

                    var _employees = db.GetEmployeeVacationRequested(deptoId).ToList();

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

        private List<EmployeePendingVacationModel> GetVacacionesPendientes()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    int deptoId = 0;

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == deptoOwner);
                    if (department != null && Session["role"].ToString() != "Admin")
                        deptoId = department.DeptoCode;
                    
                    //var _employees = db.GetEmployeePendingVacation().ToList();
                    var _employees = db.GetEmployeePendingVacation(deptoId).Where(f => f.daysToDueVacation.HasValue && f.daysToDueVacation.Value > 0).ToList();

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

        private List<EmployeePendingVacationModel> GetVacacionesVencidas()
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int deptoOwner = Session["employeeID"] != null ? int.Parse(Session["employeeID"].ToString()) : 0;
                    int deptoId = 0;

                    List<EmployeePendingVacationModel> employees = new List<EmployeePendingVacationModel>();

                    Department department = db.Departments.FirstOrDefault(d => d.DeptoOwner == deptoOwner);
                    if (department != null && Session["role"].ToString() != "Admin")
                        deptoId = department.DeptoCode;
                    
                    var _employees = db.GetEmployeePendingVacation(deptoId).Where(f => !f.daysToDueVacation.HasValue || f.daysToDueVacation.Value < 0).ToList();

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