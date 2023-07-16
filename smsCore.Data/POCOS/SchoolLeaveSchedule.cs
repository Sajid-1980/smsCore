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

    [Table("SchoolLeaveSchedule")]
    public partial class SchoolLeaveSchedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime date { get; set; }
        public string holidayName { get; set; }
        public int CampusID { get; set; }
        public string Color { get; set; }
        public bool IsHoliday { get; set; }
    
        [ForeignKey("CampusID")]
        public virtual Campus Campus { get; set; }
    }
}