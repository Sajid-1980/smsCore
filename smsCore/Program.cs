using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Models;
using smsCore;
using smsCore.BackgroundTasks;
using smsCore.Data;
using smsCore.Data.Helpers;
using smsCore.Data.Models;
using smsCore.Data.Services;
using smsCore.Data.Tenant;
using Swashbuckle.AspNetCore.SwaggerUI;
using Syncfusion.EJ2.Charts;
using System.Configuration;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VVhkQlFaclZJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkZiX39adH1VRmZZVUQ=");
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddDbContext<SchoolEntities>();

builder.Services.AddDbContext<SchoolEntities>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SchoolEntities>();
builder.Services.AddSignalRCore();
builder.Services.AddScoped<SelectListHelper>();
builder.Services.AddScoped<MessagingSystem>();

builder.Services.AddTransient<LoggedInUserHelper>();
builder.Services.AddTransient<StaticResources>();
builder.Services.AddTransient<ClaimHelper>();
//builder.Services.AddTransient<ClaimHelper>();
builder.Services.AddTransient<MenuHelper>();
builder.Services.AddTransient<SeedDataHelper>();
builder.Services.AddTransient<DatabaseAccessSql>();
builder.Services.AddTransient<CurrentUser>();
builder.Services.AddTransient<ClsBussinessSetting>();
builder.Services.AddScoped<LedgerPostingSP>();
builder.Services.AddScoped<AddFeeBackgroundTask>();
builder.Services.AddScoped<FeeSystemHelper>();
builder.Services.AddScoped<UtilityFunctions>();
builder.Services.AddScoped<Extensions1>();
builder.Services.AddScoped<ExcelSheetWorker>();
builder.Services.AddScoped<ReportData>();
builder.Services.AddScoped<AttendanceHelper>();
builder.Services.AddScoped<TransactionsGeneralFill>();
builder.Services.AddScoped<timetableLogics>();

//builder.Services.AddHangfire(configuration => configuration
//    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//    .UseSimpleAssemblyNameTypeSerializer()
//    .UseRecommendedSerializerSettings());
//.UseStorage<string>(connectionString)); 

//builder.Services.AddHangfire(configuration => configuration
//    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//    .UseSimpleAssemblyNameTypeSerializer()
//    .UseRecommendedSerializerSettings() 
//    .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));


//builder.Services.AddHangfireServer();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // Add the following line to include a bearer token in the Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add the following line to require a bearer token to access Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });

    // Add the following line to add an example of how to use the bearer token
    //c.OperationFilter<SecurityRequirementsOperationFilter>();
});



builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
    CustomClaimsPrincipalFactory>();
 
var app = builder.Build();
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "My API Documentation";
    c.DefaultModelsExpandDepth(-1);
    c.DefaultModelRendering(ModelRendering.Example);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();

});

//app.UseHangfireDashboard();
//app.UseHangfireServer();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
