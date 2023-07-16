namespace smsCore.Data.Models
{
    public class authenticationClass
    {
        public int ID { get; set; }
        public string RoleId { get; set; }
        public int FormID { get; set; }
        public string FormName { get; set; }
        public bool ViewRecords { get; set; }
        public bool AddRecords { get; set; }
        public bool EditRecords { get; set; }
        public bool DeleteRecords { get; set; }
    }
}