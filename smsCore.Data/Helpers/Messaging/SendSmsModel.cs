namespace sms.Models
{
    public class SendSmsModel
    {
        public string[] MobileNo { get; set; }
        public string Mask { get; set; }
        public string Message { get; set; }
        public bool Unicode { get; set; }

        public bool testMode { get; set; }

        public int GetMessageCount()
        {
            var totalMessage = 0;
            if (Message.Length <= 160)
            {
                totalMessage = 1;
            }

            else if (Message.Length > 160)
            {
                totalMessage = Message.Length / 160;
                if (Message.Length % 160 > 0) totalMessage += 1;
            }

            totalMessage = totalMessage * MobileNo.Length;

            return totalMessage;
        }
    }
}