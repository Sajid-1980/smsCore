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

    [Table("tbl_SalaryPackage")]
    public partial class tbl_SalaryPackage
    {
        public tbl_SalaryPackage() {
            this.SalaryPackageDetails = new HashSet<tbl_SalaryPackageDetails>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int salaryPackageId { get; set; }
        public string salaryPackageName { get; set; }
        public bool isActive { get; set; }
        public string narration { get; set; }
        public DateTime EntryDate { get; set; }
        public string UserId { get; set; }
        public virtual ICollection<tbl_SalaryPackageDetails> SalaryPackageDetails { get; set; }
    }
}
