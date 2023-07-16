using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Models;

namespace smsCore.Data.Models.ViewModels.Accounts
{
    public class ExpenceEntryViewModel
    {
        public tbl_PaymentMaster PaymentMaster { get; set; }
        public List<tbl_PaymentDetails> PaymentDetails { get; set; }
    }
}