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

        [Table("tbl_AccountGroup")]
    public partial class tbl_AccountGroup
    {
       public tbl_AccountGroup()
        {
            this.tbl_AccountLedger = new HashSet<tbl_AccountLedger>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AccountGroupName { get; set; }
        public Nullable<int> GroupUnder { get; set; }
        public string CrDr { get; set; }
        public string Narration { get; set; }
        public bool IsDefault { get; set; }
        public string Nature { get; set; }
        public string AffectGrossProfit { get; set; }
        public DateTime EntryDate { get; set; }
        public string UserId { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public virtual ICollection<tbl_AccountLedger> tbl_AccountLedger { get; set; }
    }
}
