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

        private static int DaysForThisEmployee(Employee employee)
        {
            double totalYears = (DateTime.Now.Date - employee.AdmissionDate.Value).TotalDays / 365;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    int daysForThis = db.DaysBySeniorities.FirstOrDefault(d => d.initialYears >= totalYears && d.endYears <= totalYears).days;

                    return daysForThis;
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return 14; // default days
            }
        }

        public static int GetDaysForEmployee(int employeeId)
        {
            int days = 0;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = Helper.GetEmployee(employeeId);

                    //For new employee with less than 6 months working in the Company.
                    double daysWorking = (DateTime.Now.Date - employee.AdmissionDate.Value).TotalDays;
                    if (daysWorking < 180) return 0;

                    //For employee with more than 6 months working in the Company.
                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    if (employeeDays == null)
                    {
                        var anniversaryDate = new DateTime(DateTime.Now.Date.Year, employee.AdmissionDate.Value.Month, employee.AdmissionDate.Value.Day);
                        double elapsedDays = (DateTime.Now.Date - anniversaryDate).TotalDays;

                        days = DaysForThisEmployee(employee);

                        if (elapsedDays >= 180)
                        {
                            EmployeeDay employeeDay = new EmployeeDay
                            {
                                EmployeeId = employeeId,
                                TakenDays = 0,
                                TotalDays = days,
                                CurrentYear = DateTime.Now.Date.Year,
                                CreatedDate = DateTime.Now
                            };

                            db.EmployeeDays.Add(employeeDay);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return days;
        }
    }
}