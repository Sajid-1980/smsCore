using System;

namespace smsCore.Data.Models.ViewModels
{
    public class StudentComplaintMessageViewModel
    {
        
        public string Message { get; set; }
        public int RegNo { get; set; }
        public string Mobile { get; set; }
        public string Campus { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public int AdmissionID { get; set; }
        public int StudentID { get; set; }
        public string Status { get; set; }
    }

    public class IssueMessageViewModel
    {
        public int RegistrationNo { get; set; }
        public string Month { get; set; }
        public int CampusId { get; set; }
        public bool SendRemarks { get; set; }

    }

    public class StaffMessageViewModel
    {
        public int Id { get; set; }
        public string Mobile { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        
    }

    public class AttendanceMessageViewModel
    {
        public int AttendanceId { get; set; }
        public int AdmissionId { get; set; }
        public int StudentId { get; set; }
        public string Mobile { get; set; }
        public string StudentName { get; set; }
        public string FName { get; set; }
        public string ClassName { get; set; }
        public int RegistrationNo { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceType { get; set; }
    }

    public class BulkMessageViewModel
    {
        public int RegistrationNo { get; set; }
        public string Mobile { get; set; }
        public string Message { get; set; }
        public bool IsRtl { get; set; }
    }

    public class AttendanceByClassViewModel
    {
        public int ID { get; set; }
        public int CampusID { get; set; }
        public int AttendanceType { get; set; }
        public string Date { get; set; }
    }

    public class StaffAttendanceViewModel
    {
        public int ID { get; set; }
        public int CampusID { get; set; }
        public int AttendanceType { get; set; }
        public string Date { get; set; }
    }

}