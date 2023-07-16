using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsCore.Data.Models.ViewModels
{
    public class ExamRemarksViewModel
    {

        public int ExamHeldId { get; set; }
        public int ID { get; set; }
        public string Attendance { get; set; }
        public string Confid { get; set; }
        public string CoTeamWork { get; set; }
        public string Discpline { get; set; }
        public string Manners { get; set; }
        public string Neatness { get; set; }
        public string OralExp { get; set; }
        public string PhyFit { get; set; }
        public string Punctuality { get; set; }
        public string SocBeh { get; set; }
        public string Remarks { get; set; }
    }
}