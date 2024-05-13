using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Severis.Business;
using Severis.DataAccess;
using Severis.DataAccess.Models.Context;
using Severis.DataAccess.Repository;
using Severis.FileProcessor.Services;
using System.Security.Principal;

namespace Severis.FileProcessor
{
    public class Program
    {
        public static ServiceProvider? ServiceProvider;


        public static void Main(string[] args)
        {
            const string loggerTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}]<{ThreadId}> [{SourceContext:l}] {Message:lj}{NewLine}{Exception}";
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logfile = Path.Combine(baseDir, "App_Data", "logs", "log.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.FromLogContext()
                .WriteTo.Console(LogEventLevel.Information, loggerTemplate, theme: AnsiConsoleTheme.Literate)
                .WriteTo.File(logfile, LogEventLevel.Information, loggerTemplate,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90)
                .CreateLogger();

            try
            {
                Log.Information("====================================================================");
                Log.Information($"Application Starts. Version: {System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version}");
                Log.Information($"Application Directory: {baseDir}");
                if (OperatingSystem.IsWindows())
                {
                    var userName = WindowsIdentity.GetCurrent().Name;
                    Log.Information("The runner account is [{runnerAccountName}]", userName);
                }

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application terminated unexpectedly");
            }
            finally
            {
                Log.Information("====================================================================\r\n");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Configure the app here.
                    
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddHttpClient();
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                    services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
                    services.AddTransient<IUnitOfWork, UnitOfWork>();
                    services.AddDbContext<SeverisDbContext>(
                        options =>
                        { 
                            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")); 
                        });
                    services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
                    services.AddScoped<ICommonServices, CommonServices>();
                    ServiceProvider = services.BuildServiceProvider();
                })
                .UseSerilog();
    }
}
