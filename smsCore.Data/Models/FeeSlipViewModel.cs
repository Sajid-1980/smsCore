using System.Collections.Generic;
using System;
using smsCore.Data.Helpers;

namespace smsCore.Data.Models
{
    public class FeeSlipViewModel
    {
        public FeeSlipViewModel()
        {
            Fee = new List<OptionalFee>();
        }
       public int CampusId { get; set; }
        public int ClassId { get; set; }
        public string DueDate { get; set; }
        public DateTime _DueDate { get { return DateTimeHelper.ConvertDate(DueDate); } }
        public string ForMonth { get; set; }
        public DateTime _ForMonth { get { return DateTimeHelper.ConvertDate(ForMonth); } }
        public string LastFineDate { get; set; }
        public DateTime _LastFineDate { get { return DateTimeHelper.ConvertDate(LastFineDate); } }
        public bool SendMessage { get; set; }
        public bool AddSmsCharges { get; set; }
        public bool AddTransport { get; set; }
        public bool OverWrite { get; set; }
        public List<OptionalFee> Fee { get; set; }

        
    }

    public class OptionalFee
    {
        public int Id { get; set; }
        public bool Select { get; set; }
        public decimal Amount { get; set; }
    }
}
