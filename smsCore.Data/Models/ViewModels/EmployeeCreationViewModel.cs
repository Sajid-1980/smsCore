
using Models;
using System.Collections.Generic;

namespace smsCore.Data.Models.ViewModels
{
    public class EmployeeCreationViewModel
    {
        public tbl_Employee Employee { get; set; }
        public List<Qualification> Qualifications { get; set; }
        public List<EmployeeExprience> EmployeeExpriences { get; set; }
        public List<EmployeeSubject> EmloyeeSubjects { get; set; }
    }
    public class EmployeeSubject
    {
public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public bool Select { get; set; }
    }
}