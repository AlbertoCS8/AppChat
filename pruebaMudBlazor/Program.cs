using Blazored.Modal;
using MongoDB.Bson;
using MongoDB.Driver;
using MudBlazor.Services;
using pruebaMudBlazor.Client.Pages;
using pruebaMudBlazor.Client.Services; 
using pruebaMudBlazor.Components;
using pruebaMudBlazor.Models;
using pruebaMudBlazor.Client.Models;
using pruebaMudBlazor.Controladores;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddSignalR();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddBlazoredModal();
// builder.Services.AddScoped< pruebaMudBlazor.Services.ClienteService>();

// Configurar MongoDB desde el archivo de configuración
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IClienteService,ClienteService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ThemeService>();
// Registrar MongoDB en el contenedor de dependencias
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString")));

builder.Services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(builder.Configuration.GetValue<string>("MongoDbSettings:DatabaseName")));
builder.Services.AddSingleton<IMongoCollection<Usuario>>(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<Usuario>("Usuarios"));
// builder.Services.AddSingleton<IMongoCollection<ImagenPredefinida>>(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<ImagenPredefinida>("ImagenPredefinida"));
builder.Services.AddSingleton<IMongoCollection<Chat>>(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<Chat>("Chats"));
// Registrar el servicio MongoDbService (si lo necesitas para la lógica adicional)
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<ChatResponsables>();// quitar
builder.Services.AddSingleton<Rest>(); // --> este es un diccionario que contiene los responsables de cada chat, para poder enviar mensajes a los usuarios conectados
builder.Services.AddSingleton<UsersConnected>();// --> va a ser un diccionario de usuarios conectados al que accederemos si queremos reflejar el status de los usuarios
builder.Services.AddScoped<NotificacionService>(); // --> este servicio se encargará de enviar notificaciones a los usuarios conectados
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Registrar los endpoints desde los controladores
UsuarioController.MapUsuarioEndpoints(app);
AmigosController.MapAmigosEndpoints(app);
ChatController.MapChatEndpoints(app);

app.UseAntiforgery();
app.MapHub<ChatHub>("/chathub");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(pruebaMudBlazor.Client._Imports).Assembly);

app.Run();
public class AmigoModel
{
    public string UsuarioActual { get; set; }
    public string UsuarioAmigo { get; set; }
}
public class LoginModel 
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class FotoPerfilModel
{
    public string Username { get; set; }
    public string Foto { get; set; }
}
public class MakingFriendsModel
{
    public string UsuarioActual { get; set; }
    public string UsuarioAmigo { get; set; }
    public bool Accepted { get; set; } // Indica si la solicitud fue aceptada o no
}
