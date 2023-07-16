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
    [Table("ApplicationLeave")]
    public partial class ApplicationLeave
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public Nullable<bool> Status { get; set; }
        public System.DateTime DateFrom { get; set; }
        public System.DateTime DateTo { get; set; }
        public string Comments { get; set; }
        [ForeignKey("StaffId")]
        public virtual tbl_Employee tbl_Employee { get; set; }
    }
}
