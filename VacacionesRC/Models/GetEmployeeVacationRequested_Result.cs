//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VacacionesRC.Models
{
    using System;
    
    public partial class GetEmployeeVacationRequested_Result
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int DaysAvailable { get; set; }
        public int DaysRequested { get; set; }
        public int DaysTaken { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public string Note { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeDepto { get; set; }
        public string Location { get; set; }
    }
}