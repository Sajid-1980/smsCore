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

    public partial class ComSub
    {
        [Key]
        public int ID { get; set; }
        public int Attendance { get; set; }
        public int Result { get; set; }
        public int IssuedFee { get; set; }
        public int ReceiveFee { get; set; }
        public int StudentID { get; set; }
    
        public virtual Student Student { get; set; }
    }
}
