using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsCore.Data.Models.ViewModels
{
    public class ExamDateViewModel
    {
        public int ExamHeldID { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int ClassSectionID { get; set; }
        public int CampusID { get; set; }
        public int SubjectID { get; set; }
        public DateTime ExamDate1 { get; set; }
        public string TimeFrom { get; set; }
        public string ToTime { get; set; }
        public double TotalMarks { get; set; }
    }
}