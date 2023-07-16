using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using System.Net;

namespace smsCore.Data.Tenant
{
    public interface ITenantProvider
    {
        string GetTenantName();
        Tenant GetTenant(string name);
    }
    public class TenantProvider : ITenantProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantName()
        {
            var host = _httpContextAccessor.HttpContext.Request.Host.Host;
            var tenantName = host.Split('.')[0];
            return tenantName;
        }

        public Tenant GetTenant(string name)
        {
            var connectionString = _configuration.GetSection($"Tenants:{name}:ConnectionString").Value;
            return new Tenant { Name = name, ConnectionString = connectionString };
        }
    }
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantProvider _tenantProvider;

        public TenantMiddleware(RequestDelegate next, ITenantProvider tenantProvider)
        {
            _next = next;
            _tenantProvider = tenantProvider;
        }

        public async Task Invoke(HttpContext context, SchoolEntities dbContext)
        {
            var tenant = _tenantProvider.GetTenant(context.Request.Host.Value);

            // Set the connection string for the current tenant
            dbContext.Database.SetConnectionString(tenant.ConnectionString);

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    


}
