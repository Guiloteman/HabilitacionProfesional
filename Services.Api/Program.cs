using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.Api.Middlewares;
using Services.Application;
using Services.Domain.Entities;
using Services.Infrastructure;
using Services.Infrastructure.Persistence.Scaffolded;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ServicesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ServicesDbContext>()
.AddDefaultTokenProviders();

// 1. CONFIGURAR CORS PARA PERMITIR AL CLIENTE BLAZOR COMUNICARSE CON SIGNALR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7094", "http://localhost:5006") // Puertos de tu proyecto Client
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Obligatorio para las conexiones de SignalR
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// 2. USAR CORS ANTES DE MAPEAR LOS ENDPOINTS Y HUBS
app.UseCors("AllowBlazorClient");

app.MapHub<LocationHub>("/locationHub");

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();