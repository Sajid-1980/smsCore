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

    [Table("EmployeeAttendanceType")]
    public partial class EmployeeAttendanceType
    {
        public EmployeeAttendanceType()
        {
            this.EmployeeAttendances = new HashSet<EmployeeAttendance>();
        }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string AttendanceName { get; set; }
        public double FineInDays { get; set; }
        public int YearlyAllowed { get; set; }
        public string Code { get; set; }
    
        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; }
    }
}
