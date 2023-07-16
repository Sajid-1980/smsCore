using smsCore.Data.Tenant;
using Hangfire.SqlServer;

namespace smsCore.Helpers
{
    public class MultiTenantSqlServerStorageProvider : SqlServerStorage
    {
        private readonly ITenantProvider _tenantProvider;

        public MultiTenantSqlServerStorageProvider(ITenantProvider? tenantProvider) : base(tenantProvider.GetTenant(tenantProvider.GetTenantName()).ConnectionString)
        {
            _tenantProvider = tenantProvider;
        }
        //public override string SchemaName => _tenantProvider.GetCurrentTenant().DatabaseSchema;
    }
}
