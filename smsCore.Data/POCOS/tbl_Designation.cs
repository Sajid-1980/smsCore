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

    public partial class tbl_Designation
    {
        public tbl_Designation()
        {
            this.tbl_Employee = new HashSet<tbl_Employee>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Column("designationId")]
        public int Id { get; set; }
        public string designationName { get; set; }
        public Nullable<decimal> leaveDays { get; set; }
        public Nullable<decimal> advanceAmount { get; set; }
        public string narration { get; set; }
        public Nullable<System.DateTime> extraDate { get; set; }
        public string extra1 { get; set; }
        public string extra2 { get; set; }
    
        public virtual ICollection<tbl_Employee> tbl_Employee { get; set; }
    }
}
