using API.Jobs;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Service;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Repository;
using Service;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string settingFile = string.Empty;

            string? envEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (envEnvironment == null)
                settingFile = "appsettings.Development.json";
            else if (envEnvironment == "Development")
                settingFile = "appsettings.Development.json";
            else if (envEnvironment == "Production")
                settingFile = "appsettings.json";

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile(settingFile, optional: false, reloadOnChange: true);
            });

            // Add services to the container.

            builder.Services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = builder.Configuration["Redis:Server"];
            });

            #region MongoDb

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(builder.Configuration["MongoDB:ConnectionString"]);
                var client = new MongoClient(settings);
                return client;
            });

            #endregion

            #region Repository

            var dbMongoName = builder.Configuration["MongoDB:DbName"];

            builder.Services.AddSingleton<IUserRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserBalanceRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserBalanceRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserBalanceHistoryRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserBalanceHistoryRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IVoucherRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new VoucherRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserVoucherRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserVoucherRepository(mongoClient, dbMongoName); });
            builder.Services.AddSingleton<IUserBlackListRepository>(sp => { var mongoClient = sp.GetRequiredService<IMongoClient>(); return new UserBlackListRepository(mongoClient, dbMongoName); });

            #endregion

            #region Services

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITelegramService, TelegramService>();
            builder.Services.AddScoped<IBotSmsService, BotSmsService>();
            builder.Services.AddScoped<ISmsActiveService, SmsActiveService>();
            builder.Services.AddScoped<IForexService, ForexService>();
            builder.Services.AddScoped<ICacheService, CacheService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<I5SimService, FiveSimService>();

            builder.Services.AddTransient<IStringLocalizer<BotSmsService>, StringLocalizer<BotSmsService>>();

            #endregion

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "pt-BR" };
                options.SetDefaultCulture(supportedCultures[0])
                         .AddSupportedCultures(supportedCultures)
                         .AddSupportedUICultures(supportedCultures);
            });

            builder.Services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented; // Opcional: formata��o do JSON
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // HANGFIRE
            builder.Services.AddHangfire(config =>
            {
                var mongoUrlBuilder = new MongoUrlBuilder(builder.Configuration["MongoDB:ConnectionString"]);
                var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());
                config.UseMongoStorage(mongoClient, dbMongoName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy(),
                    },
                    CheckConnection = false,
                    Prefix = builder.Configuration["HangFirePrefix"]
                });
            });

            builder.Services.Configure<BackgroundJobServerOptions>(options =>
            {
                options.WorkerCount = 20;
                options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
                options.ServerCheckInterval = TimeSpan.FromSeconds(5);
            });

            builder.Services.AddHangfireServer();

            #region BACKGROUND TASKS

            builder.Services.AddHostedService<AutoCreateVoucher>();

            #endregion

            var app = builder.Build();

            // Mensagem indicando que o bot está funcionando
            Console.WriteLine("ta indo");

            app.UseRequestLocalization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // ...
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            //app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthorization();

            // Configuração de autorização para o Hangfire Dashboard
            var options = new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            };
            app.UseHangfireDashboard("/trigger", options);

            app.MapControllers();

            app.Run();
        }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}