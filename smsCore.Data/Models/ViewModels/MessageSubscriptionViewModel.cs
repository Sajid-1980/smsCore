using System;

namespace smsCore.Data.Models.ViewModels
{

    public class MessageSubscriptionViewModel
    {
        public int StudentId { get; set; }
        public bool Attendance { get; set; }
        public bool Result { get; set; }
        public bool ReceiveFee { get; set; }
        public bool IssuedFee { get; set; }
    }
}