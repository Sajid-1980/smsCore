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

    public partial class Result
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ClassSubjectID { get; set; }
        public int ExamHeldID { get; set; }
        public double ObtainedMarks { get; set; }
        public int AdmissionID { get; set; }
        public string CheckedBy { get; set; }
        public string UserID { get; set; }
    
        public virtual Admission Admission { get; set; }
        public virtual ClassSubject ClassSubject { get; set; }
        public virtual ExamHeld ExamHeld { get; set; }
        [ForeignKey("UserID")]
        public virtual  ApplicationUser AspNetUser { get; set; }
    }
}
