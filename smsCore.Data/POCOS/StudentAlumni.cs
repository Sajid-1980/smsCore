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

    public partial class StudentAlumni
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Studied { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public string OfficeAddress { get; set; }
        public string HomeAddress { get; set; }
        public string DefaultCellNo { get; set; }
        public string CellNo { get; set; }
        public string OfficePhone { get; set; }
        public string ResidancePhone { get; set; }
        public string Email { get; set; }
        public string Facebook { get; set; }
        public byte[] Photo { get; set; }
        public Nullable<int> StudentID { get; set; }
        public Nullable<System.DateTime> EntryDate { get; set; }
        public string remarks { get; set; }
        public string Campus { get; set; }

        [ForeignKey("StudentID")]
        public virtual Student Student{get;set;}
    }
}
