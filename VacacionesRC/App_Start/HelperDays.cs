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

                        days = DaysForThisEmployeeBySeniority(employee);

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
                        else
                        {
                            days = 0;
                        }
                    }
                    else
                    {
                        days = employeeDays.TotalDays;
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