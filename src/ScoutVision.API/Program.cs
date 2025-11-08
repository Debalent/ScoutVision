using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using RabbitMQ.Client;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Text;
using ScoutVision.Infrastructure.Data;
using ScoutVision.Infrastructure.Services;
using ScoutVision.Infrastructure.Auth;
using ScoutVision.Infrastructure.RealTime;
using ScoutVision.Infrastructure.Messaging;
using ScoutVision.Infrastructure.Caching;
using ScoutVision.Infrastructure.Monitoring;
using Hangfire;
using Hangfire.SqlServer;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "scoutvision-{0:yyyy.MM.dd}",
        BufferCleanPayload = true,
    })
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Swagger/OpenAPI
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() 
        { 
            Title = "ScoutVision Pro Intelligence API", 
            Version = "v2.0",
            Description = "Comprehensive sports intelligence platform with real-time predictions, injury prevention, and transfer valuation"
        });
        c.EnableAnnotations();
        
        // Add JWT authentication to Swagger
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    // Database Context
    builder.Services.AddDbContext<ScoutVisionDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.CommandTimeout(300)));

    // PostgreSQL TimescaleDB for time-series data
    builder.Services.AddDbContext<TimeSeriesContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("TimeSeriesConnection"),
            pgsqlOptions => pgsqlOptions.CommandTimeout(300)));

    // Redis Caching
    var redisConnection = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis__ConnectionString") ?? "redis:6379");
    builder.Services.AddSingleton(redisConnection);
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionFactory = () => Task.FromResult<IConnectionMultiplexer>(redisConnection);
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // Authentication - JWT
    var jwtSecret = builder.Configuration["Auth__JwtSecret"] ?? "your-super-secret-jwt-key-change-in-production-minimum-32-chars";
    var key = Encoding.ASCII.GetBytes(jwtSecret);
    
    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        
        // Support WebSocket authentication
        x.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api/hubs") &&
                    context.Request.Query.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        options.AddPolicy("Coach", policy => policy.RequireRole("Coach", "Admin"));
        options.AddPolicy("Analyst", policy => policy.RequireRole("Analyst", "Coach", "Admin"));
        options.AddPolicy("Club", policy => policy.RequireRole("Club", "Coach", "Admin"));
    });

    // Real-time communication with SignalR
    builder.Services.AddSignalR(o =>
    {
        o.MaximumReceiveMessageSize = 100_000_000; // 100MB for video data
        o.HandshakeTimeout = TimeSpan.FromSeconds(10);
        o.KeepAliveInterval = TimeSpan.FromSeconds(30);
    });

    // RabbitMQ Message Queue
    builder.Services.AddSingleton<IConnectionFactory>(sp =>
    {
        var connectionString = builder.Configuration.GetConnectionString("RabbitMQ__ConnectionString") ?? "amqp://scout_admin:ScoutVision2024!@rabbitmq:5672/";
        var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
        factory.AutomaticRecoveryEnabled = true;
        factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);
        return factory;
    });
    builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();

    // Core Services
    builder.Services.AddScoped<ISearchService, SearchService>();
    builder.Services.AddScoped<IFootageAnalysisService, FootageAnalysisService>();
    builder.Services.AddScoped<IStatBookService, StatBookService>();
    builder.Services.AddScoped<IInjuryPrevention, InjuryPreventionService>();
    builder.Services.AddScoped<ITransferValuation, TransferValuationService>();
    builder.Services.AddScoped<IDataIntegrationService, DataIntegrationService>();
    builder.Services.AddScoped<IMultiTenantService, MultiTenantService>();
    builder.Services.AddSingleton<ScoutVision.API.Services.EmailService>();

    // TODO: Implement concrete service classes for the following interfaces
    // Notification Services - interfaces defined in ScoutVision.Infrastructure.Notifications
    // Reporting Services - interfaces defined in ScoutVision.Infrastructure.Reporting
    // Collaboration Services - interfaces defined in ScoutVision.Infrastructure.Collaboration
    // Security Services - interfaces defined in ScoutVision.Infrastructure.Security
    // Data Integration Services - interfaces defined in ScoutVision.Infrastructure.Integration
    // Marketplace Services - interfaces defined in ScoutVision.Infrastructure.Marketplace
    // Advanced AI Services - interfaces defined in ScoutVision.AI.Services

    // Background Jobs with Hangfire
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddHangfireServer();

    // Rate Limiting (AspNetCoreRateLimit)
    builder.Services.AddMemoryCache();
    builder.Services.Configure<AspNetCoreRateLimit.IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<AspNetCoreRateLimit.IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

    // HTTP Client factory for external APIs
    builder.Services.AddHttpClient();

    // CORS - allow real-time connections
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("X-Total-Count", "X-Total-Pages");
        });

        options.AddPolicy("AllowBlazorApp", policy =>
        {
            policy.WithOrigins("https://localhost:7001", "http://localhost:5001", "http://localhost", "https://localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Total-Count", "X-Total-Pages");
        });
    });

    // OpenTelemetry Tracing
    builder.Services.AddOpenTelemetry()
        .WithTracing(b =>
        {
            b
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("scoutvision-api"))
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                })
                .AddEntityFrameworkCoreInstrumentation(o =>
                {
                    o.SetDbStatementForText = true;
                    o.SetDbStatementForStoredProcedure = true;
                })
                .AddHttpClientInstrumentation(o =>
                {
                    o.RecordException = true;
                })
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = builder.Configuration["Jaeger__AgentHost"] ?? "jaeger";
                    o.AgentPort = int.Parse(builder.Configuration["Jaeger__AgentPort"] ?? "6831");
                });
        });

    // Metrics and Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("Database")
        .AddCheck<RedisHealthCheck>("Redis")
        .AddCheck<RabbitMQHealthCheck>("RabbitMQ")
        .AddCheck<TimeSeriesHealthCheck>("TimeSeries");

    builder.Services.AddPrometheusMetrics();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ScoutVision Pro API v2.0");
            c.RoutePrefix = string.Empty;
        });
    }

    // Use HTTPS redirection only in production
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Middleware pipeline
    app.UseRouting();
    app.UseCors("AllowBlazorApp");
    
    // Logging middleware
    app.UseSerilogRequestLogging();

    // Authentication and Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // OpenTelemetry metrics endpoint
    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    // Map endpoints
    app.MapControllers();
    
    // Real-time hubs
    app.MapHub<PlayerAnalyticsHub>("/api/hubs/player-analytics");
    app.MapHub<InjuryAlertHub>("/api/hubs/injury-alerts");
    app.MapHub<TransferValueHub>("/api/hubs/transfer-values");

    // Health check endpoint
    app.MapHealthChecks("/api/health");
    app.MapHealthChecks("/api/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteResponse
    });

    // Metrics endpoint
    app.MapMetrics();

    // Database migration
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ScoutVisionDbContext>();
        context.Database.Migrate();
        
        var timeSeriesContext = scope.ServiceProvider.GetRequiredService<TimeSeriesContext>();
        timeSeriesContext.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}