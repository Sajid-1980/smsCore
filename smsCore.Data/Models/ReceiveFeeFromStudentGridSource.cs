namespace smsCore.Data.Models
{
    public class ReceiveFeeFromStudentGridSource
    {
        private decimal _received;
        public int? pk { get; set; }
        public int RegNo { get; set; }
        public bool Select { get; set; }
        public string Month { get; set; }
        public decimal ReceiveableAmount { get; set; }


        public decimal LateFee { get; set; }
        public decimal Balance { get; set; }

        public decimal Received
        {
            get => _received;
            set
            {
                _received = value;
                Balance = ReceiveableAmount - _received;
                Select = _received > 0;
            }
        }
    }

}