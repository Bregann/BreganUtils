using BreganUtils.ProjectMonitor;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Dashboard.Dark;
using Hangfire.PostgreSql;
using HangfireDashboard;

#if DEBUG
ProjectMonitorConfig.SetupMonitor("debug", AppConfig.ProjectMonitorApiKey);
#else
ProjectMonitorConfig.SetupMonitor("release", AppConfig.ProjectMonitorApiKey);
#endif

ProjectMonitorCommon.ReportProjectUp("Hangfire Dashboard");

var builder = WebApplication.CreateBuilder(args);

#if RELEASE
builder.WebHost.UseUrls("http://localhost:5001");
#endif

builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "projectmonitor" })
        .UseDarkDashboard()
);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var auth = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,
            SslRedirect = false,
            LoginCaseSensitive = true,
            Users = new []
            {
                new BasicAuthAuthorizationUser
                {
                    Login = AppConfig.HFLogin,
                    PasswordClear = AppConfig.HFPassword
                }
            }
        })};

//Setup all the HF dashboards
var pm = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "projectmonitor" });
var fm = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "financemanager" });
var catbot = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "catbot" });
var twitchbot = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "twitchbot" });
var messagingMicroservice = new PostgreSqlStorage(AppConfig.HFConnectionString, new PostgreSqlStorageOptions { SchemaName = "messagingmicroservice" });

app.MapHangfireDashboard("/pm", new DashboardOptions
{
    Authorization = auth
}, pm);

app.MapHangfireDashboard("/catbot", new DashboardOptions
{
    Authorization = auth
}, catbot);

app.MapHangfireDashboard("/fm", new DashboardOptions
{
    Authorization = auth
}, fm);

app.MapHangfireDashboard("/emailms", new DashboardOptions
{
    Authorization = auth
}, messagingMicroservice);

app.MapHangfireDashboard("/twitchbot", new DashboardOptions
{
    Authorization = auth
}, twitchbot);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Run();