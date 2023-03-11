using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacacionesRC.ViewModels
{
    public class VacationModel
    {
        public int Id { get; set; }
        public Guid IdHash { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeePosition { get; set; }
        public int DeptoId { get; set; }
        public string DeptoName { get; set; }
        public string Status { get; set; }
        public int DaysTaken { get; set; }
        public int DaysAvailable { get; set; }
        public int DaysRequested { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Note { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string AcceptedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string RejectedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int Year { get; set; }
        public string Location { get; set; }
    }

    public class DeptoModel
    {
        public int DeptoCode { get; set; }
        public string DeptoName { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string UserRole { get; set; }
    }

    public class ExceptionModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int Year { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
    }

    public class EmployeeOnVacationModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDepto { get; set; }
        public string EmployeeLocation { get; set; }
        public int Year { get; set; }
        public int DaysAvailable { get; set; }
        public int DaysTaken { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
    }

    public class EmployeePendingVacationModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDepto { get; set; }
        public string EmployeePosition { get; set; }
        public string EmployeeLocation { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime RenovationDate { get; set; }
        public DateTime DueVacationDate { get; set; }
        public int DaysToDueVacation { get; set; }
        public int DaysAvailable { get; set; }
        public int TimeInCompany { get; set; }
        public int DaysRequested { get; set; }
    }
}