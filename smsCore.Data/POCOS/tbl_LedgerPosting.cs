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

    public partial class tbl_LedgerPosting
    {
        public tbl_LedgerPosting()
        {
        
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CampusId { get; set; }
        public int voucherTypeId { get; set; }
        public string voucherNo { get; set; }
        public int ledgerId { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public int detailsId { get; set; }
        public int yearId { get; set; }
        public string invoiceNo { get; set; }
        public string chequeNo { get; set; }
        public DateTime chequeDate { get; set; }
        public string UserId { get; set; }
        public DateTime EntryDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}