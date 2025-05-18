using Blazored.Modal;
using MongoDB.Bson;
using MongoDB.Driver;
using MudBlazor.Services;
using pruebaMudBlazor.Client.Pages;
using pruebaMudBlazor.Client.Services; 
using pruebaMudBlazor.Components;
using pruebaMudBlazor.Models;
using pruebaMudBlazor.Client.Models;



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
builder.Services.AddSingleton<UsersConnected>();// --> va a ser un diccionario de usuarios conectados al que accederemos si queremos reflejar el status de los usuarios

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

// Endpoint para registrar un usuario
app.MapPost("/registro", async (pruebaMudBlazor.Models.UserModel registro, 
    IMongoCollection<Usuario> usuarios) =>
{
    Console.WriteLine("Registro de usuario recibido, procesando...");
    var usuario = UserMapper.MapToUsuario(registro);
    // Guardamos el usuario en la base de datos
    await usuarios.InsertOneAsync(usuario);
    return Results.Ok("Registro exitoso desde el endpoint");
});

// Endpoint para iniciar sesión
app.MapPost("/api/login", async (LoginModel loginModel, 
    IMongoCollection<Usuario> usuarios) => 
{
    Console.WriteLine($"iniciio de sesión recibido en server {loginModel.Email} {loginModel.Password}");

    var usuario = await usuarios.Find(u => u.Email == loginModel.Email).FirstOrDefaultAsync();
    
    if (usuario != null && BCrypt.Net.BCrypt.Verify(loginModel.Password, usuario.PasswordHash))
    {
        // using var scope = scopeFactory.CreateScope();
        // var authState = scope.ServiceProvider.GetRequiredService<AuthService>();

        // Guardamos el estado del usuario en AuthState
       // authState.Username = usuario.NombreUsuario;
       // authState.ImagenBase64 = usuario.FotoPerfil;
        //authState.IsAuthenticated = true;
    var respuesta = UserMapper.MapToUserModel(usuario);
        
        return Results.Ok(respuesta);
    }
    else 
    {
        return Results.BadRequest("Credenciales inválidas");
    }
});
// Endpoint para cambiar la foto de perfil
app.MapPost("/api/cambiarFotoPerfil", async (FotoPerfilModel model, 
    IMongoCollection<Usuario> usuarios) =>
{
    var usuario = await usuarios.Find(u => u.NombreUsuario == model.Username).FirstOrDefaultAsync();
    
    if (usuario != null)
    {
        usuario.FotoPerfil = model.Foto;
        await usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
        // act foto perfil base64
        return Results.Ok("Foto de perfil actualizada");
    }
    else
    {
        return Results.NotFound("Usuario no encontrado");
    }
});
// Endpoint para buscar usuarios por nombre de usuario --> func buscar amigos
app.MapGet("/api/buscarUsuarios", async (string nombreUsuario, 
    IMongoCollection<Usuario> usuarios) =>
{
    // Buscamos usuarios que contengan el nombre de usuario
    var filtro = Builders<Usuario>.Filter.Regex(u => u.NombreUsuario, new BsonRegularExpression(nombreUsuario, "i"));
    var resultados = await usuarios.Find(filtro).ToListAsync();
    
    return Results.Ok(resultados);
});
// Endpoint para agregar amigos
app.MapPost("/api/agregarAmigo", async (AmigoModel model, IMongoCollection<Usuario> usuarios) =>
{
    try
    {
        // Verificar que ambos usuarios existen
        var usuarioActual = await usuarios.Find(u => u.NombreUsuario == model.UsuarioActual).FirstOrDefaultAsync();
        var usuarioAmigo = await usuarios.Find(u => u.NombreUsuario == model.UsuarioAmigo).FirstOrDefaultAsync();
        if (usuarioActual == null || usuarioAmigo == null)
        {
            return Results.NotFound(new ResponseServer { 
                CodigoError = 1, 
                Mensaje = "Uno de los usuarios no existe"
            });
        }
        // Verificar que no son el mismo usuario
        if (model.UsuarioActual == model.UsuarioAmigo)
        {
            return Results.BadRequest(new ResponseServer{ 
                CodigoError = 1, 
                Mensaje = "No puedes agregar a ti mismo como amigo"
            });
        }
        // Inicializar la lista de amigos si es null
        if (usuarioActual.Amigos == null)
        {
            usuarioActual.Amigos = new List<string>();
        }
        // Verificar si ya son amigos -->Z futuro cambio que salga un boton con un texto distinto indicando que ya son amigos
        if (usuarioActual.Amigos.Contains(model.UsuarioAmigo))
        {
            return Results.BadRequest(new ResponseServer{ 
                CodigoError = 1, 
                Mensaje = "Ya son amigos"
            });
        }
        usuarioActual.Amigos.Add(model.UsuarioAmigo);
        // Actualizar usuario en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);
        //devolvemos modelo ResponseServer
        return Results.Ok(new ResponseServer{ 
            CodigoError = 0, 
            Mensaje = "Amigo agregado exitosamente"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al agregar amigo: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});
// Endpoint para obtener amigos --> en el perfil a la hora de ver la lista de amigos veremos usernames y fotos
//y cuando iniciemos chats igual
app.MapPost("/api/obtenerAmigos", async (List<string> amigosUsernames, 
    IMongoCollection<Usuario> usuarios,UsersConnected usersConnected) =>
{
    try
    {
        //obtenemos los usernames/ img de perfil de cada user de la lista
        var usuarios_encontrados = await usuarios.Find(u => amigosUsernames.Contains(u.NombreUsuario)).ToListAsync();
        
        // Mapear a la lista de objetos Amigos con solo la información necesaria
        var amigos = usuarios_encontrados.Select(u => new Amigos
        {
            Username = u.NombreUsuario,
            FotoPerfil = u.FotoPerfil,
            Status = usersConnected.GetUserStatus(u.NombreUsuario) 
        }).ToList();
        
        return Results.Ok(amigos);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener amigos: {ex.Message}");
        return Results.Problem("Error al obtener la lista de amigos", statusCode: 500, extensions: new Dictionary<string, object>
        {
            { "CodigoError", 1 }
        });
    }
});
// Endpoint para guardar un mensaje en la base de datos
app.MapPost("/api/guardarMensaje", async (pruebaMudBlazor.Models.ChatMessage mensaje, string roomId) =>
{
    try
    {
        // Guardamos el mensaje en Bdd Coleccion Chats crearemos un objeto Chat por ejemplo que contenga
        //ChatMessages y el roomId lo puedo usar para identificar el chat
                
        
        return Results.Ok("Mensaje guardado exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al guardar el mensaje: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});



app.UseAntiforgery();

app.MapHub<ChatHub>("/chathub"); // es un metodo de la clase ChatHub, crea una url y registra la clase
// para poder usar los metodos de la clase ChatHub de manera remota


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
