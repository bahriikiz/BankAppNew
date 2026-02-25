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

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

//   SERVÝSLER

// Angular (Frontend) isteklerine izin vermek için CORS politikasýný ekliyoruz
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowAnyOrigin();
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
    // JWT doðrulamasý baþarýlý ancak kullanýcýnýn gerekli yetkilere sahip olmadýðý durumlarda 403 Forbidden döndür

    options.Events = new JwtBearerEvents
    {
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

// CORS middleware'ini kullanýma alýyoruz (Routing ve Auth arasýnda olmasý en saðlýklýsýdýr)
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();