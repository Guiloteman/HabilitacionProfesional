using Microsoft.AspNetCore.Components.Authorization;
using Services.Client.Components;
using Services.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Necesario para leer las cookies de la petición actual
builder.Services.AddHttpContextAccessor();

// 2. Registrar el manejador que reenvía la cookie de identidad
builder.Services.AddTransient<CookieHandler>();

// 3. Configurar el HttpClient nombrado apuntando a la API
builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7245/");
}).AddHttpMessageHandler<CookieHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();