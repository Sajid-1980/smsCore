using System;
using System.Collections.Generic;
using Models;

namespace smsCore.Data.Models.ViewModels
{
    public class EmployeeEditViewModel
    {
        public string branchName { get; set; }
        public int CampusID { get; set; }
        public string bankAccountNumber { get; set; }
        public string branchCode { get; set; }
        public string panNumber { get; set; }
        public string pfNumber { get; set; }
        public string esiNumber { get; set; }
        public DateTime? extraDate { get; set; }
        public string extra1 { get; set; }
        public string extra2 { get; set; }
        public decimal? defaultPackageId { get; set; }
        public string Title { get; set; }
        public byte[] Photo { get; set; }
        public string FatherName { get; set; }
        public string CNIC { get; set; }
        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; }
        public virtual ICollection<LeavedStaff> LeavedStaffs { get; set; }
        public virtual ICollection<ProfessionalTraining> ProfessionalTrainings { get; set; }
        public virtual ICollection<EmployeeShortLeave> EmployeeShortLeaves { get; set; }
        public virtual List<EmployeeExprienceViewModel> EmployeeExpriences { get; set; }
        public virtual List<EmployeeQualificationViewModel> EmployeeQualification { get; set; }
        public virtual List<EmployeeExperinceSubjectViewModel> EmployeeExperinceSubjects { get; set; }
        public string bankName { get; set; }
        public decimal? dailyWage { get; set; }
        public string salaryType { get; set; }
        public DateTime? visaExpiryDate { get; set; }
        public decimal Id { get; set; }
        public decimal? designationId { get; set; }
        public string employeeName { get; set; }
        public string employeeCode { get; set; }
        public DateTime? dob { get; set; }
        public string maritalStatus { get; set; }
        public string gender { get; set; }
        public string qualification { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public virtual ICollection<TeachingSubject> TeachingSubjects { get; set; }
        public string mobileNumber { get; set; }
        public DateTime? joiningDate { get; set; }
        public DateTime? terminationDate { get; set; }
        public bool? isActive { get; set; }
        public string narration { get; set; }
        public string bloodGroup { get; set; }
        public string passportNo { get; set; }
        public DateTime? passportExpiryDate { get; set; }
        public string labourCardNumber { get; set; }
        public DateTime? labourCardExpiryDate { get; set; }
        public string visaNumber { get; set; }
        public string email { get; set; }
        public virtual tbl_Designation tbl_Designation { get; set; }
    }

    public class EmployeeExprienceViewModel
    {
        public int ID { get; set; }
        public decimal StfID { get; set; }
        public string Institution { get; set; }
        public string Desgnition { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public virtual tbl_Employee tbl_Employee { get; set; }
    }

    public class EmployeeQualificationViewModel
    {
        public int ID { get; set; }
        public decimal StaffID { get; set; }
        public string UserID { get; set; }
        public string University { get; set; }
        public string QualificationType { get; set; }
        public int Year { get; set; }
        public string Grade { get; set; }
        public virtual ApplicationUser AspNetUser { get; set; }
        public virtual tbl_Employee tbl_Employee { get; set; }
    }

    public class EmployeeExperinceSubjectViewModel
    {
        public decimal EmployeeId { get; set; }
        public int SubjectId { get; set; }
        public decimal ID { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual tbl_Employee tbl_Employee { get; set; }
    }
}