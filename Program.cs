using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using System.Text;
using arroyoSeco.Infrastructure.Data;
using arroyoSeco.Infrastructure.Auth;
using arroyoSeco.Infrastructure.Services;
using arroyoSeco.Domain.Entities.Usuarios;
using arroyoSeco.Application.Common.Interfaces;
using arroyoSeco.Infrastructure.Services;
using arroyoSeco.Application.Features.Alojamiento.Commands.Crear;
using arroyoSeco.Application.Features.Reservas.Commands.Crear;
using arroyoSeco.Application.Features.Gastronomia.Commands.Crear;
using arroyoSeco.Application.Features.Reservas.Commands.CambiarEstado;
using arroyoSeco.Infrastructure.Storage;
using System.Text.Json.Serialization;
using System.Runtime.ExceptionServices;
using arroyoSeco.Services;

var builder = WebApplication.CreateBuilder(args);

// Tama�o m�ximo del body a nivel Kestrel (50 MB)
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 50_000_000;
});

// En producción Railway usa la variable PORT, en desarrollo usa HTTPS
if (builder.Environment.IsProduction())
{
    // Railway asigna el puerto automáticamente
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    builder.WebHost.UseKestrel(o =>
    {
        o.ListenLocalhost(7190, lo => lo.UseHttps());
    });
}

builder.Services.AddHttpContextAccessor();

// Capturar excepciones globales (si algo revienta mostrar log)
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    Console.WriteLine("UNHANDLED: " + e.ExceptionObject);
};
TaskScheduler.UnobservedTaskException += (s, e) =>
{
    Console.WriteLine("UNOBSERVED: " + e.Exception);
    e.SetObserved();
};
AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
{
    Console.WriteLine("FIRST CHANCE: " + e.Exception.GetType().Name + " - " + e.Exception.Message);
};
builder.Services.AddHostedService<ShutdownLogger>();
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50_000_000;
    o.ValueLengthLimit = int.MaxValue;
    o.MemoryBufferThreshold = 1024 * 1024;
});

const string CorsPolicy = "FrontPolicy";
if (builder.Environment.IsProduction())
{
    builder.Services.AddCors(p =>
    {
        p.AddPolicy(CorsPolicy, policy =>
        {
            policy.WithOrigins(
                "https://arroyosecoservices.vercel.app",
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}
else
{
    builder.Services.AddCors(p =>
    {
        p.AddPolicy(CorsPolicy, policy =>
        {
            policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });
    });
}

// Obtener connection string desde DATABASE_URL (Render) o ConnectionStrings__DefaultConnection (local)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convertir URI de PostgreSQL (formato Render) a connection string de Npgsql
    var uri = new Uri(databaseUrl);
    var port = uri.Port > 0 ? uri.Port : 5432; // Puerto por defecto de PostgreSQL
    connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine($"=== Using DATABASE_URL from environment (converted from URI)");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("No connection string configured");
    Console.WriteLine($"=== Using DefaultConnection from appsettings.json");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
    .EnableSensitiveDataLogging()
);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString, 
        npgsql => npgsql.MigrationsAssembly("arroyoSeco.Infrastructure")));

builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequireDigit = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IFolioGenerator, FolioGenerator>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStorageService, DiskStorageService>();
builder.Services.AddScoped<CrearAlojamientoCommandHandler>();
builder.Services.AddScoped<CrearEstablecimientoCommandHandler>();
builder.Services.AddScoped<CrearMenuCommandHandler>();
builder.Services.AddScoped<AgregarMenuItemCommandHandler>();
builder.Services.AddScoped<CrearMesaCommandHandler>();
builder.Services.AddScoped<CrearReservaGastronomiaCommandHandler>();
builder.Services.AddScoped<CrearReservaCommandHandler>();
builder.Services.AddScoped<CambiarEstadoReservaCommandHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // ← Agregar camelCase
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ArroyoSeco API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
var storage = builder.Configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();
if (string.IsNullOrWhiteSpace(storage.ComprobantesPath))
{
    storage.ComprobantesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "arroyoSeco", "comprobantes");
}
builder.Services.PostConfigure<StorageOptions>(o =>
{
    if (string.IsNullOrWhiteSpace(o.ComprobantesPath))
        o.ComprobantesPath = storage.ComprobantesPath;
});

// Configurar opciones de Email
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
var emailOptions = builder.Configuration.GetSection("Email").Get<EmailOptions>();
// Solo usa la configuración de Gmail desde appsettings.json
if (emailOptions != null && string.IsNullOrWhiteSpace(emailOptions.SmtpHost))
{
    // No se asigna Mailtrap, solo se usa la configuración de Gmail
}
builder.Services.PostConfigure<EmailOptions>(o =>
{
    if (emailOptions != null && !string.IsNullOrWhiteSpace(emailOptions.SmtpHost))
    {
        o.SmtpHost = emailOptions.SmtpHost;
        o.SmtpPort = emailOptions.SmtpPort;
        o.SmtpUsername = emailOptions.SmtpUsername;
        o.SmtpPassword = emailOptions.SmtpPassword;
        o.FromEmail = emailOptions.FromEmail;
        o.FromName = emailOptions.FromName;
    }
});

var app = builder.Build();

// Middleware manual para responder OPTIONS con headers de CORS
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        if (context.Request.Headers.ContainsKey("Origin"))
            context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

// Middleware global de errores (evita cierre silencioso)
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine("GLOBAL EXCEPTION: " + ex);
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsync("Error interno");
    }
});

// Crear carpeta y servir archivos
var storageOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>().Value;
var comprobantesPath = storageOptions.ComprobantesPath;

// En producción, usar una ruta temporal si no está configurada o no es absoluta
if (string.IsNullOrEmpty(comprobantesPath) || !Path.IsPathRooted(comprobantesPath))
{
    comprobantesPath = Path.Combine(Path.GetTempPath(), "arroyoseco-comprobantes");

}

Directory.CreateDirectory(comprobantesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(comprobantesPath),
    RequestPath = "/comprobantes"
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors(CorsPolicy); // CORS debe ir antes de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Endpoint de salud para verificar que no se cayó
app.MapGet("/health", () => Results.Ok("OK"));

// Endpoint global OPTIONS para CORS preflight
app.MapMethods("/{**any}", new[] { "OPTIONS" }, () => Results.Ok())
    .RequireCors(CorsPolicy);

app.MapControllers();

// Aplicar migraciones automáticamente en producción
using (var scope = app.Services.CreateScope())
{
    try
    {
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
// Tamaño máximo del body a nivel Kestrel (50 MB)
        appDbContext.Database.Migrate();
        authDbContext.Database.Migrate();
        Console.WriteLine("=== Migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== Error applying migrations: {ex.Message}");
        throw;
    }
}

// Crear roles si no existen
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Cliente", "Oferente", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Crear usuario admin si no existe
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@arroyo.com";
    var adminPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");

            var app = builder.Build();

            // ✅ Handler OPTIONS primero
            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "OPTIONS")
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin))
                    {
                        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                        context.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,PATCH,OPTIONS";
                        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
                        context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                        context.Response.Headers["Access-Control-Max-Age"] = "600";
                    }
                    context.Response.StatusCode = 204; // No Content
                    await context.Response.CompleteAsync();
                    return;
                }
                await next();
            });

            app.UseSwagger();
            app.UseSwaggerUI();

            // ✅ HTTPS solo en desarrollo
            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            // Middleware global de errores (evita cierre silencioso)
            app.Use(async (ctx, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GLOBAL EXCEPTION: " + ex);
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.WriteAsync("Error interno");
                }
            });

            // Crear carpeta y servir archivos
            var storageOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>().Value;
            var comprobantesPath = storageOptions.ComprobantesPath;

            // En producción, usar una ruta temporal si no está configurada o no es absoluta
            if (string.IsNullOrEmpty(comprobantesPath) || !Path.IsPathRooted(comprobantesPath))
            {
                comprobantesPath = Path.Combine(Path.GetTempPath(), "arroyoseco-comprobantes");
            }

            Directory.CreateDirectory(comprobantesPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(comprobantesPath),
                RequestPath = "/comprobantes"
            });

            app.UseCors(CorsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();

            // Endpoint de salud para verificar que no se cayó
            app.MapGet("/health", () => Results.Ok("OK"));

            // Endpoint global OPTIONS para CORS preflight
            app.MapMethods("/{**any}", new[] { "OPTIONS" }, () => Results.Ok())
                .RequireCors(CorsPolicy);

            app.MapControllers();

            // Aplicar migraciones automáticamente en producción
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                    appDbContext.Database.Migrate();
                    authDbContext.Database.Migrate();
                    Console.WriteLine("=== Migrations applied successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== Error applying migrations: {ex.Message}");
                    throw;
                }
            }

            // Crear roles si no existen
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Cliente", "Oferente", "Admin" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                // Crear usuario admin si no existe
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@arroyo.com";
                var adminPassword = builder.Configuration["SeedAdmin:Password"] ?? "Admin123!";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"=== Admin user created: {adminEmail}");
                    }
                    else
                    {
                        Console.WriteLine($"=== Error creating admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    // Verify and assign Admin role if user exists but doesn't have it
                    var adminRoles = await userManager.GetRolesAsync(adminUser);
                    if (!adminRoles.Contains("Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"=== Admin role assigned to existing user: {adminEmail}");
                    }
                    else
                    {
                        Console.WriteLine($"=== Admin user already exists with correct role: {adminEmail}");
                    }
                }
            }

            app.Run();

