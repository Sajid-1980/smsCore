using System;
using System.Collections.Generic;
using smsCore.Data.Helpers;

namespace smsCore.Data.Models.ViewModels
{
    public class SalaryPackageViewModel
    {
        
        public int Id { get; set; }
        public string PackageName { get; set; }
        public bool IsActive { get; set; }
        public string Narration { get; set; }
        public List<SalaryPayHeadViewModel> PayHeads { get; set; }
    }
    public class SalaryPayHeadViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Type { get; set; }
    }

    public class MonthlySalarySettingViewModel
    {
        public int EmployeeId { get; set; }
        public int SalaryPackageId { get; set; }
        public string Month { get; set; }
        public DateTime _Month { get { return DateTimeHelper.ConvertDate(Month); } }
    }
}