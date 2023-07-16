namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class tbl_BudgetDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int BudgetMasterId { get; set; }
        public int LedgerId { get; set; }
        public string CrDr { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }


        [ForeignKey("BudgetMasterId")]
        public virtual tbl_BudgetMaster BudgetMaster { get; set; }
    }
}
