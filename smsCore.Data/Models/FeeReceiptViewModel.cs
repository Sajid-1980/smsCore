namespace smsCore.Data.Models
{
    public class FeeReceiptViewModel
    {
        public string Name { get; set; }
        public int RegistrationNo { get; set; }
        public decimal Total { get; set; }
        public decimal Received { get; set; }
        public decimal LateFee { get; set; }
    }
}