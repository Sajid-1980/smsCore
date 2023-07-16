//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("tbl_MonthlySalaryDetails")]
    public partial class tbl_MonthlySalaryDetails
    {
        public tbl_MonthlySalaryDetails()
        {

        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MonthlySalaryId { get; set; }
        public int PayHeadId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public int type { get; set; }
        [ForeignKey("PayHeadId")]
        public virtual tbl_PayHead PayHead { get; set; }
        [ForeignKey("MonthlySalaryId")]
        public virtual tbl_MonthlySalary MonthlySalary { get; set; }
    }
}