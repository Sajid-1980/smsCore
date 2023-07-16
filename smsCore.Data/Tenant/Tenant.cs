using Microsoft.EntityFrameworkCore;

namespace smsCore.Data.Tenant
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
    
}
