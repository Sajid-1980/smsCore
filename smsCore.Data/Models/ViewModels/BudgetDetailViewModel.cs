using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace smsCore.Data.Models.ViewModels
{
    public class BudgetMasterViewModel
    {
        [Required] public decimal BudgetMasterId { get; set; }

        [Required] public string BudgetName { get; set; }

        [Required] public DateTime FromDate { get; set; }

        [Required] public DateTime ToDate { get; set; }

        public decimal totalDr { get; set; }
        public decimal totalCr { get; set; }
        public string extra1 { get; set; }
        public string extra2 { get; set; }
        public DateTime extra1Date { get; set; }

        public string Narration { get; set; }

        public List<BudgetDetailsViewModle> BudgetDetails { get; set; }
    }

    public class BudgetDetailsViewModle
    {
        public decimal Id { get; set; }
        public decimal BudgetMasterId { get; set; }
        public decimal Amount { get; set; }
        public string extra1 { get; set; }
        public string extra2 { get; set; }
        public DateTime extra1Date { get; set; }
        public decimal credit { get; set; }
        public decimal debit { get; set; }
    }
}