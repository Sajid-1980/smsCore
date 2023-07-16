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

    public partial class HostelAdmission
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public HostelAdmission()
        {
            this.HostelAttendances = new HashSet<HostelAttendance>();
            this.Visitors = new HashSet<Visitor>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int HostelId { get; set; }
        public int AdmisionId { get; set; }
        public DateTime AdmissionDate { get; set; }
        public int Fee { get; set; }
        public bool Isexpel { get; set; }

        [ForeignKey("HostelId")]
        public virtual Hostel Hostel { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
        [ForeignKey("AdmisionId")]
        public virtual Admission Admission { get; set; }
        public virtual ICollection<HostelAttendance> HostelAttendances { get; set; }
        public virtual ICollection<Visitor> Visitors { get; set; }
    }
}