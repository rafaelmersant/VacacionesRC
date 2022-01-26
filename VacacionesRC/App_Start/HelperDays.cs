using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VacacionesRC.Models;

namespace VacacionesRC.App_Start
{
    public class HelperDays
    {
        public HelperDays()
        {

        }

        public static DateTime? GetReturnDate(DateTime endDate)
        {
            bool foundAvailableDay = false;
            var nextDay = endDate.AddDays(1);

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var holidays = db.Holidays.Where(h => h.Date >= endDate);

                    while (foundAvailableDay == false)
                    {
                        var holiday = holidays.FirstOrDefault(h => h.Date == nextDay.Date);

                        if (nextDay.DayOfWeek != DayOfWeek.Saturday &&
                            nextDay.DayOfWeek != DayOfWeek.Sunday &&
                            holiday == null)
                        {
                            foundAvailableDay = true;
                        }

                        if (!foundAvailableDay)
                            nextDay = nextDay.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return nextDay;
        }

        //Get Holidays between these dates
        public static int GetHolidays(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    return db.Holidays.Where(h => h.Date >= startDate.Date && h.Date <= endDate.Date).Count();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return 0;
        }

        //Get how many days belong to this employee according their years in the company
        private static int DaysForThisEmployeeBySeniority(Employee employee)
        {
            int totalYears = (int)(DateTime.Now.Date - employee.AdmissionDate.Value).TotalDays / 365;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var daysForThis = db.DaysBySeniorities.FirstOrDefault(d => d.initialYears <= totalYears && d.endYears >= totalYears);

                    if (daysForThis != null)
                        return daysForThis.days;
                    else
                        return 14; //default days
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return 14; // default days
            }
        }

        private static EmployeeDay AllDaysTakenCurrentYear(int employeeId)
        {
            try
            {
                using (VacacionesRCEntities db = new VacacionesRCEntities())
                {
                    var employeeDays = db.EmployeeDays
                                         .Where(e => e.EmployeeId == employeeId)
                                         .OrderByDescending(o => o.CreatedDate)
                                         .FirstOrDefault();

                    return employeeDays;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return null;
        }

        private static int GetCurrentYearForEmployee(int employeeId)
        {
            try
            {
                using (VacacionesRCEntities db = new VacacionesRCEntities())
                {
                    var employeeDays = db.EmployeeDays
                                         .Where(e => e.EmployeeId == employeeId && e.TakenDays < 14)
                                         .OrderBy(o => o.CurrentYear)
                                         .FirstOrDefault();

                    if (employeeDays != null)
                        return employeeDays.CurrentYear;
                    else
                    {
                        var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId);
                        if (employee != null && employee.AdmissionDate.Value.Year < DateTime.Today.Year)
                        {
                            var _employeeDays = db.EmployeeDays
                                         .Where(e => e.EmployeeId == employeeId)
                                         .OrderByDescending(o => o.CurrentYear)
                                         .FirstOrDefault();

                            if (_employeeDays != null)
                            {
                                var _date = new DateTime(_employeeDays.CurrentYear, employee.AdmissionDate.Value.Month, employee.AdmissionDate.Value.Day);
                                return _date.AddYears(1).Year;
                            }

                            return DateTime.Today.AddYears(-1).Year;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return DateTime.Today.Year;
        }

        //Get how many days belong to this employee for the current year
        public static EmployeeDay GetDaysForEmployee(int employeeId)
        {
            EmployeeDay employeeDay = null;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = Helper.GetEmployee(employeeId);

                    EmployeeDay _employeeDays = AllDaysTakenCurrentYear(employeeId);
                    bool currentYearFull = _employeeDays != null ? (_employeeDays.TotalDays == (_employeeDays.TakenDays ?? 0)) : false;
                    var __current__ = GetCurrentYearForEmployee(employeeId);
                    var _currentDate = currentYearFull ? new DateTime((_employeeDays.CurrentYear + 1), 1, 1) : new DateTime(__current__, 1, 1);

                    var anniversaryDate = new DateTime(_currentDate.Year, employee.AdmissionDate.Value.Month, employee.AdmissionDate.Value.Day);
                    var renovationDate = anniversaryDate.AddYears(1);//anniversaryDate;
                    var dueDate = renovationDate.AddMonths(6); //anniversaryDate.AddMonths(6);

                    //Check if it's a new employee, so the anniversaryDate should be after one year
                    if (employee.AdmissionDate.Value.AddDays(365) > DateTime.Today)
                    {
                        anniversaryDate = employee.AdmissionDate.Value.AddYears(1);
                        renovationDate = anniversaryDate;
                        dueDate = anniversaryDate.AddMonths(6);
                    }

                    //Check if this vacacions already reaches dueDate
                    if (dueDate <= DateTime.Today.Date)
                    {
                        anniversaryDate = anniversaryDate.AddYears(1);
                        renovationDate = anniversaryDate;//.AddMonths(-6);
                        dueDate = anniversaryDate.AddMonths(6);
                    }

                    employeeDay = new EmployeeDay { TotalDays = 0, RenovationDate = renovationDate, DueDate = dueDate, CurrentYear = anniversaryDate.Year };

                    //For employee with more than 6 months working in the Company.
                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId && e.CurrentYear == anniversaryDate.Year)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    //Take vacation after one year (exceptions)
                    ExceptionsVacation exceptionsVacation = db.ExceptionsVacations.FirstOrDefault(e => e.EmployeeId == employeeId && e.Year == employeeDay.CurrentYear);
                    if (exceptionsVacation != null)
                        dueDate = dueDate.AddMonths(6);

                    if (employeeDays == null)
                    {
                        int days = DaysForThisEmployeeBySeniority(employee);

                        employeeDay = new EmployeeDay
                            {
                                EmployeeId = employeeId,
                                TakenDays = 0,
                                TotalDays = days,
                                CurrentYear = anniversaryDate.Year,
                                RenovationDate = renovationDate,
                                DueDate = dueDate,
                                CreatedDate = DateTime.Now
                            };

                        db.EmployeeDays.Add(employeeDay);
                        db.SaveChanges();
                    }
                    else
                    {
                        employeeDays.DueDate = dueDate;
                        db.SaveChanges();

                        return employeeDays;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return employeeDay;
        }

        //Update takenDays
        public static EmployeeDay UpdateTakenDays(int employeeId, int takenDays, int oldDays = 0, DateTime? endDate = null)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var fullCurrentYear = AllDaysTakenCurrentYear(employeeId);
                    var allDaysTaken = fullCurrentYear != null ? (fullCurrentYear.TotalDays == (fullCurrentYear.TakenDays ?? 0)) : false;

                    int year = allDaysTaken ? DateTime.Today.AddYears(1).Year : fullCurrentYear.CurrentYear;

                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId && e.CurrentYear == year)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    if (employeeDays != null)
                    {
                        int _takenDays = (employeeDays.TakenDays?? 0) + takenDays - oldDays;

                        if (endDate != null && employeeDays.RenovationDate > endDate && _takenDays > 7 && (employeeDays.RenovationDate.Value.Year == year))
                        {
                            ExceptionsVacation exceptionsVacation = db.ExceptionsVacations.FirstOrDefault(e => e.EmployeeId == employeeId && e.Year == employeeDays.CurrentYear);

                            if (exceptionsVacation == null)
                                throw new Exception("(1001) Esta tratando de solicitar más días de lo permitido durante el periodo previo a la renovación.");
                        }
                            
                        employeeDays.TakenDays = _takenDays;
                        db.SaveChanges();
                    }

                    return employeeDays;
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("(1001)"))
                    Helper.SendException(ex);
            }

            return null;
        }
    }
}