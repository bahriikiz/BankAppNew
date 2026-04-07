using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnlineBankAppServer.Presentation;
using OnlineBankAppServer.Persistance;
using OnlineBankAppServer.Application;
using OnlineBankAppServer.Application.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using OnlineBankAppServer.Infrasturcture;
using System.Threading.RateLimiting;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

//   SERVÝSLER

// CORS POLÝTÝKASI (Çerezlerin geçiþine izin verildi - GÜVENLÝK!)
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .SetIsOriginAllowed(origin => true)
         .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddApplication();
builder.Services.AddInfrasturcture();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(AssemblyReference).Assembly)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

// --- GÜVENLÝK: RATE LIMITING KALKANI ---
builder.Services.AddRateLimiter(options =>
{
    // Yapay Zeka için: IP baþýna dakikada maksimum 5 soru!
    options.AddPolicy("AiLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Online Bank App API",
        Version = "v1",
        Description = "Banka Uygulamasý API Dokümantasyonu"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Sadece Token'ý yapýþtýrýn (Bearer yazmanýza gerek yok)",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "OnlineBankApp",
        ValidAudience = "OnlineBankAppUsers",
        IssuerSigningKey = JwtProvider.GetPublicKey()
    };

    // GÜVENLÝK: TOKEN'I ÇEREZDEN (COOKIE) OKU!
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Ýstemci token'ý header'da göndermese bile, eðer çerezde varsa al!
            var token = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var userId = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                         ?? context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var tokenStamp = context.Principal?.FindFirst("SecurityStamp")?.Value;

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedUserId))
            {
          
                var userStampFromDb = await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == parsedUserId)
                    .Select(u => u.SecurityStamp)
                    .FirstOrDefaultAsync();

                if (userStampFromDb == Guid.Empty || !userStampFromDb.ToString().Equals(tokenStamp, StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail("Oturum iptal edilmiþ veya geçersiz!");
                }
            }
            else
            {
                context.Fail("Kimliksiz token.");
            }
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                Message = "Bu iþlemi gerçekleþtirmek için Yönetici (Admin) yetkisine sahip olmanýz gerekmektedir!"
            });

            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHostedService<OnlineBankAppServer.WebApi.BackgroundServices.OpenBankingSyncWorker>();

WebApplication app = builder.Build();

app.UseMiddleware<OnlineBankAppServer.WebApi.Middlewares.ExceptionMiddleware>();

app.UseCors();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();