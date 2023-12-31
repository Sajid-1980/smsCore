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

    public partial class StudentAttendence
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public System.DateTime AttendanceDate { get; set; }
        public int AdmissionID { get; set; }
        public double FineAmount { get; set; }
        public int AttendanceTypeID { get; set; }
        public string UserID { get; set; }
        public Nullable<bool> sendmessage { get; set; }
        public Nullable<bool> manaual { get; set; }
    
        public virtual Admission Admission { get; set; }
        [ForeignKey("AttendanceTypeID")]
        public virtual StudentAttendanceType StudentAttendanceType { get; set; }
        [ForeignKey("UserID")]
        public virtual  ApplicationUser AspNetUser { get; set; }
    }
}
