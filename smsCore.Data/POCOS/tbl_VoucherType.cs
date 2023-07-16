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

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("tbl_VoucherType")]
    public partial class tbl_VoucherType
    {
        public tbl_VoucherType()
        {

        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string VoucherTypeName { get; set; }
        public string TypeOfVoucher { get; set; }
        public string MethodOfVoucherNumbering { get; set; }
        public bool IsTaxApplicable { get; set; }
        public string Narration { get; set; }
        public DateTime EntryDate { get; set; }
        public string UserId { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int MasterId { get; set; }
        public string Declaration { get; set; }
        public string Heading1 { get; set; }
        public string Heading2 { get; set; }
        public string Heading3 { get; set; }
        public string Heading4 { get; set; }
    }
}
