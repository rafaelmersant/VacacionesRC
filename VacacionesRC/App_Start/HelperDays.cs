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

        //Get how many days belong to this employee for the current year
        public static EmployeeDay GetDaysForEmployee(int employeeId)
        {
            EmployeeDay employeeDay = null;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = Helper.GetEmployee(employeeId);
                    var anniversaryDate = new DateTime(DateTime.Now.Date.Year, employee.AdmissionDate.Value.Month, employee.AdmissionDate.Value.Day);
                    var renovationDate = anniversaryDate.AddDays(-180);
                    var dueDate = anniversaryDate.AddDays(180);

                    //Check if this vacacions already reaches dueDate
                    if (dueDate <= DateTime.Today.Date)
                    {
                        anniversaryDate = anniversaryDate.AddYears(1);
                        renovationDate = anniversaryDate.AddDays(-180);
                        dueDate = anniversaryDate.AddDays(180);
                    }

                    employeeDay = new EmployeeDay { TotalDays = 0, RenovationDate = renovationDate, DueDate = dueDate, CurrentYear = anniversaryDate.Year };

                    //For new employee with less than 6 months working in the Company.
                    double daysWorking = (DateTime.Now.Date - employee.AdmissionDate.Value).TotalDays;
                    if (daysWorking < 180) return employeeDay;

                    //For employee with more than 6 months working in the Company.
                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId && e.CurrentYear == anniversaryDate.Year)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    if (employeeDays == null)
                    {
                        int days = DaysForThisEmployeeBySeniority(employee);

                        if (renovationDate <= DateTime.Today.Date)
                        {
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
                        }

                        db.EmployeeDays.Add(employeeDay);
                        db.SaveChanges();
                    }
                    else
                    {
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
        public static void UpdateTakenDays(int employeeId, int takenDays)
        {
            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    if (employeeDays != null)
                    {
                        employeeDays.TakenDays += takenDays;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }
    }
}