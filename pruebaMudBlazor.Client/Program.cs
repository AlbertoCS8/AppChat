using Blazored.Modal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using pruebaMudBlazor.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
builder.Services.AddBlazoredModal();
// En Program.cs
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
