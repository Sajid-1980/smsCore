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

    public partial class UnwantedClient
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int StdID { get; set; }
        public string UserID { get; set; }
        public string Reason { get; set; }
        public string CNIC { get; set; }
    
        public virtual Student Student { get; set; }
        [ForeignKey("UserID")]
        public virtual  ApplicationUser AspNetUser { get; set; }
    }
}
