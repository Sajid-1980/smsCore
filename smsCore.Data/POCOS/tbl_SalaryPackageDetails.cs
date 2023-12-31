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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("tbl_SalaryPackageDetails")]
    public partial class tbl_SalaryPackageDetails
    {
        public tbl_SalaryPackageDetails()
        {
            
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int salaryPackageDetailsId { get; set; }
        public int salaryPackageId { get; set; }
        public int payHeadId { get; set; }
        public decimal amount { get; set; }
        public string narration { get; set; }
        [ForeignKey("payHeadId")]
        public virtual tbl_PayHead PayHead { get; set; }
        [ForeignKey("salaryPackageId")]
        public virtual tbl_SalaryPackage SalaryPackage { get; set; }
    }
}
