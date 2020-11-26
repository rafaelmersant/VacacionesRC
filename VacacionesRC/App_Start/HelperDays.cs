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
        public static EmployeeDay GetDaysForEmployee(int employeeId)
        {
            EmployeeDay employeeDay = null;

            try
            {
                using (var db = new VacacionesRCEntities())
                {
                    Employee employee = Helper.GetEmployee(employeeId);
                    var anniversaryDate = new DateTime(DateTime.Now.Date.Year, employee.AdmissionDate.Value.Month, employee.AdmissionDate.Value.Day);
                    var renovationDate = anniversaryDate.AddDays(180);
                    var dueDate = renovationDate.AddDays(180);

                    employeeDay = new EmployeeDay { TotalDays = 0, RenovationDate = renovationDate, DueDate = dueDate, CurrentYear = DateTime.Now.Year };

                    //For new employee with less than 6 months working in the Company.
                    double daysWorking = (DateTime.Now.Date - employee.AdmissionDate.Value).TotalDays;
                    if (daysWorking < 180) return employeeDay;

                    //For employee with more than 6 months working in the Company.
                    var employeeDays = db.EmployeeDays
                                            .Where(e => e.EmployeeId == employeeId)
                                            .OrderByDescending(o => o.CreatedDate)
                                            .FirstOrDefault();

                    if (employeeDays == null)
                    {
                        double elapsedDays = (DateTime.Now.Date - anniversaryDate).TotalDays;

                        int days = DaysForThisEmployeeBySeniority(employee);

                        if (elapsedDays >= 180)
                        {
                            employeeDay = new EmployeeDay
                            {
                                EmployeeId = employeeId,
                                TakenDays = 0,
                                TotalDays = days,
                                CurrentYear = DateTime.Now.Date.Year,
                                RenovationDate = renovationDate,
                                DueDate = dueDate,
                                CreatedDate = DateTime.Now
                            };

                            db.EmployeeDays.Add(employeeDay);
                            db.SaveChanges();
                        } 
                        else
                        {
                            return employeeDay;
                        }
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
    }
}