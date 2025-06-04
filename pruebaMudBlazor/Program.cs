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
builder.Services.AddSingleton<Rest>(); // --> este es un diccionario que contiene los responsables de cada chat, para poder enviar mensajes a los usuarios conectados
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
    if (registro == null || string.IsNullOrEmpty(registro.Email) || string.IsNullOrEmpty(registro.Password))
    {
        return Results.BadRequest("Datos de registro inválidos");
    }
    var usuarioExistente = await usuarios.Find(u => u.Email == registro.Email || u.NombreUsuario == registro.NombreUsuario).FirstOrDefaultAsync();
    if (usuarioExistente != null)
    {
        return Results.Conflict("El usuario ya existe");
    }
    Console.WriteLine("Registro de usuario recibido, procesando...");
    var usuario = UserMapper.MapToUsuario(registro);
    // Guardamos el usuario en la base de datos
    await usuarios.InsertOneAsync(usuario);
    return Results.Ok("Registro exitoso desde el endpoint");
});

// Endpoint para iniciar sesión
app.MapPost("/api/login", async (LoginModel loginModel, 
    IMongoCollection<Usuario> usuarios,
    Rest _rest) => 
{
    // Console.WriteLine($"iniciio de sesión recibido en server {loginModel.Email} {loginModel.Password}");

    var usuario = await usuarios.Find(u => u.Email == loginModel.Email).FirstOrDefaultAsync();
    
    if (usuario != null && BCrypt.Net.BCrypt.Verify(loginModel.Password, usuario.PasswordHash))
    {
        // using var scope = scopeFactory.CreateScope();
        // var authState = scope.ServiceProvider.GetRequiredService<AuthService>();

        // Guardamos el estado del usuario en AuthState
        // authState.Username = usuario.NombreUsuario;
        // authState.ImagenBase64 = usuario.FotoPerfil;
        //authState.IsAuthenticated = true;
        // Actualizamos la última conexión del usuario
        Console.WriteLine($"Hora de madridddddd {await _rest.GetMadridTimeFormatted()}");
        await usuarios.UpdateOneAsync(
            u => u.Email == usuario.Email,
            Builders<Usuario>.Update.Set("UltimaConexion", await _rest.GetMadridTimeFormatted())
        );
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
app.MapGet("/api/buscarUsuarios", async (string nombreUsuario, string currentUser, 
    IMongoCollection<Usuario> usuarios) =>
{
    // Get the current user to access their sent friend requests
    var usuarioActual = await usuarios.Find(u => u.NombreUsuario == currentUser).FirstOrDefaultAsync();
    if (usuarioActual == null)
    {
        return Results.NotFound("Usuario no encontrado");
    }

    // Get usernames of users who already have pending requests from current user
    var pendingRequestUsernames = usuarioActual.FriendRequestEnviada?
        .Select(fr => fr.Username)
        .ToList() ?? new List<string>();

    
    // Add the current user to the exclusion list
    pendingRequestUsernames.Add(currentUser);
    //añadimos tambien las notificaciones de solicitudes de amistad recibidas
    if (usuarioActual.Notificaciones != null)
    {
        pendingRequestUsernames.AddRange(usuarioActual.Notificaciones.Select(n => n.SenderUsername));
    }

    // Also exclude existing friends
    if (usuarioActual.Amigos != null)
    {
        pendingRequestUsernames.AddRange(usuarioActual.Amigos);
    }

    // Create filter: name matches pattern AND not in exclusion list
    var filtro = Builders<Usuario>.Filter.And(
        Builders<Usuario>.Filter.Regex(u => u.NombreUsuario, new BsonRegularExpression(nombreUsuario, "i")),
        Builders<Usuario>.Filter.Nin(u => u.NombreUsuario, pendingRequestUsernames)
    );

    var resultados = await usuarios.Find(filtro).ToListAsync();
    return Results.Ok(resultados);
});
// Endpoint para mandar una solicitud de amistad
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
        // añadir la FriendRequestEnviada al usuario actual para poder filtrar al buscar usuarios
        if (usuarioActual.FriendRequestEnviada == null)
        {
            usuarioActual.FriendRequestEnviada = new List<FriendRequest>();
        }
        usuarioActual.FriendRequestEnviada.Add(new FriendRequest
        {
            Username = usuarioAmigo.NombreUsuario, // ID del usuario al que se envía la solicitud
            SenderUsername = usuarioActual.NombreUsuario, // ID del usuario que envía la solicitud
            Message = $"{usuarioActual.Nombre} {usuarioActual.Apellido} quiere ser tu amigo" // Mensaje de la solicitud
        });
        // Verificar si ya existe una solicitud pendiente
        if (usuarioAmigo.Notificaciones.Any(n => n.SenderUsername == usuarioActual.NombreUsuario))
        {
            return Results.BadRequest(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "Ya has enviado una solicitud de amistad a este usuario"
            });
        }
        // usuarioActual.Amigos.Add(model.UsuarioAmigo);
        // // Actualizar usuario en la base de datos
        // await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);

        usuarioAmigo.Notificaciones.Add(new FriendRequest
        {
            Username = usuarioAmigo.NombreUsuario, 
            SenderUsername = usuarioActual.NombreUsuario, // ID del usuario que envió la solicitud
            Message = $"{usuarioActual.Nombre} {usuarioActual.Apellido} quiere ser tu amigo" 
        });
        // Actualizar el usuario amigo en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioAmigo.Id, usuarioAmigo);
        // Actualizar el usuario actual en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);
        //devolvemos modelo ResponseServer
        return Results.Ok(new ResponseServer
        {
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
            Status = usersConnected.GetUserStatus(u.NombreUsuario),
            UltimaConexion = u.UltimaConexion
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
app.MapPost("/api/eliminarAmigo", async (AmigoModel model,
    IMongoCollection<Usuario> usuarios, IMongoCollection<Chat> chatsCollection) =>
{
    try
    {
        // Verificar que ambos usuarios existen
        var usuarioActual = await usuarios.Find(u => u.NombreUsuario == model.UsuarioActual).FirstOrDefaultAsync();
        var usuarioAmigo = await usuarios.Find(u => u.NombreUsuario == model.UsuarioAmigo).FirstOrDefaultAsync();
        if (usuarioActual == null || usuarioAmigo == null)
        {
            return Results.NotFound(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "Uno de los usuarios no existe"
            });
        }
        // Verificar que no son el mismo usuario
        if (model.UsuarioActual == model.UsuarioAmigo)
        {
            return Results.BadRequest(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "No puedes eliminarte a ti mismo como amigo"
            });
        }
        // Verificar si son amigos
        if (!usuarioActual.Amigos.Contains(model.UsuarioAmigo))
        {
            return Results.BadRequest(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "No son amigos"
            });
        }
        usuarioActual.Amigos.Remove(model.UsuarioAmigo);
        usuarioAmigo.Amigos.Remove(model.UsuarioActual);

        // Actualizar usuario en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioAmigo.Id, usuarioAmigo);
        //borramos los chats que existan entre ambos usuarios coleccion chats (chqat tiene _id que es los dos usernames separados por _ y ordenado alfabeticamente)
        var primeroUsername = model.UsuarioActual.CompareTo(model.UsuarioAmigo) < 0 ? model.UsuarioActual : model.UsuarioAmigo;
        var segundoUsername = model.UsuarioActual.CompareTo(model.UsuarioAmigo) < 0 ? model.UsuarioAmigo : model.UsuarioActual;
        var roomId = $"{primeroUsername}_{segundoUsername}";
        // Eliminamos el chat de la colección
        chatsCollection.DeleteOne(c =>
           c.Id == roomId
           );
        return Results.Ok(new ResponseServer
        {
            CodigoError = 0,
            Mensaje = "Amigo eliminado exitosamente"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al eliminar amigo: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});
// Endpoint para obtener notificaciones de un usuario
//return _httpClient.GetFromJsonAsync<List<string>>($"/api/obtenerNotificaciones?username={username}");
app.MapGet("/api/obtenerNotificaciones", async (string username,
    IMongoCollection<Usuario> usuarios) =>
{
    try
    {
        var usuario = await usuarios.Find(u => u.NombreUsuario == username).FirstOrDefaultAsync();
        if (usuario == null)
        {
            return Results.NotFound("Usuario no encontrado");
        }
        return Results.Ok(usuario.Notificaciones);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener notificaciones: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});
// Endpoint para hacer amighos a dos usuarios, paso final del flujo de solicitud de amistad
app.MapPost("/api/makeFriend", async (MakingFriendsModel model,
    IMongoCollection<Usuario> usuarios) =>
{
    try
    {
        // Verificar que ambos usuarios existen
        var usuarioActual = await usuarios.Find(u => u.NombreUsuario == model.UsuarioActual).FirstOrDefaultAsync();
        var usuarioAmigo = await usuarios.Find(u => u.NombreUsuario == model.UsuarioAmigo).FirstOrDefaultAsync();
        var IsAccepted = model.Accepted;
        if (IsAccepted == false)
        {
            if (usuarioActual.Notificaciones != null)
            {
                usuarioActual.Notificaciones.RemoveAll(n => n.SenderUsername == usuarioAmigo.NombreUsuario);
                usuarioAmigo.FriendRequestEnviada?.RemoveAll(fr => fr.Username == usuarioActual.NombreUsuario);
                await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);
                await usuarios.ReplaceOneAsync(u => u.Id == usuarioAmigo.Id, usuarioAmigo);
            }
            return Results.Ok(new ResponseServer
            {
                CodigoError = 0,
                Mensaje = "Solicitud de amistad rechazada"
            });
        }
        if (usuarioActual == null || usuarioAmigo == null)
        {
            return Results.NotFound(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "Uno de los usuarios no existe"
            });
        }
        // Verificar que no son el mismo usuario
        if (model.UsuarioActual == model.UsuarioAmigo)
        {
            return Results.BadRequest(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "No puedes hacerte amigo de ti mismo"
            });
        }
        if (usuarioActual.Amigos == null)
        {
            usuarioActual.Amigos = new List<string>();
        }
        if (usuarioActual.Amigos.Contains(model.UsuarioAmigo))
        {
            return Results.BadRequest(new ResponseServer
            {
                CodigoError = 1,
                Mensaje = "Ya son amigos"
            });
        }
        usuarioActual.Amigos.Add(model.UsuarioAmigo);
        usuarioActual.Notificaciones.RemoveAll(n => n.SenderUsername == usuarioAmigo.NombreUsuario);
        usuarioAmigo.FriendRequestEnviada?.RemoveAll(fr => fr.Username == usuarioActual.NombreUsuario);
        // Actualizar usuario en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);

        usuarioAmigo.Amigos.Add(model.UsuarioActual);

        // Actualizar el usuario amigo en la base de datos
        await usuarios.ReplaceOneAsync(u => u.Id == usuarioAmigo.Id, usuarioAmigo);

        return Results.Ok(new ResponseServer
        {
            CodigoError = 0,
            Mensaje = "Amigos creados exitosamente"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al hacer amigos: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});
// Endpoint para obtener mensajes no leídos de un usuario /api/getMensajesNoLeidos?username={username}"
app.MapGet("/api/getMensajesNoLeidos", async (string username,
    IMongoCollection<Chat> chats, IMongoCollection<Usuario> users) =>
{
    try
    {
        //obtenemos los amigos del usuario
        var usuario = await users.Find(u => u.NombreUsuario == username).FirstOrDefaultAsync();
        if (usuario == null)
        {
            return Results.NotFound("Usuario no encontrado");
        }
        //cremos el id de la sala de chat que es el nombre del amigo con el del usuario actual ordenado alfabeticamente
        var idsSalas = new List<string>();
        foreach (var amigo in usuario.Amigos)
        {
            var primeroUsername = username.CompareTo(amigo) < 0 ? username : amigo;
            var segundoUsername = username.CompareTo(amigo) < 0 ? amigo : username;
            idsSalas.Add($"{primeroUsername}_{segundoUsername}");
        }
        //obtenemos los chats de esas salas
        var chatsEncontrados = await chats.Find(c => idsSalas.Contains(c.Id)).ToListAsync();
        // Creamos un diccionario para almacenar los mensajes no leídos por sala con el nombre del que manda != a nuestro user
        var mensajesNoLeidos = new Dictionary<string, int>();
        foreach (var chat in chatsEncontrados)
        {
            // encontramos mensajes no leidos
            var mensajesSinLeer = chat.Mensajes.Where(m => m.UserName != username && !m.IsRead).ToList();
            //de esta lista de mensajes filtramos por nombre de usuario y contamos los mensajes no leídos
            if (mensajesSinLeer.Count > 0)
            {
                // Obtenemos el nombre del amigo
                var amigoUsername = chat.Id.Split('_').FirstOrDefault(u => u != username);
                if (amigoUsername != null)
                {
                    // Agregamos al diccionario el nombre del amigo y la cantidad de mensajes no leídos
                    mensajesNoLeidos[amigoUsername] = mensajesSinLeer.Count;
                }
            }
        }
        // Devolvemos el diccionario con los mensajes no leídos
        return Results.Ok(mensajesNoLeidos);


    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener mensajes no leídos: {ex.Message}");
        return Results.Problem("Error interno del servidor", statusCode: 500);
    }
});

// Endpoint para actualizar mensajes no leídos de un usuario
app.MapPost("/api/actualizarMensajesNoLeidos", async (string Username, string FriendUsername, int MensajesNoLeidos,
    IMongoCollection<Chat> chats, IMongoCollection<Usuario> users) =>
{
    try
    {
        //obtenemos el usuario actual
        var usuario = await users.Find(u => u.NombreUsuario == Username).FirstOrDefaultAsync();
        if (usuario == null)
        {
            return Results.NotFound("Usuario no encontrado");
        }

        //creamos el id de la sala de chat
        var primeroUsername = Username.CompareTo(FriendUsername) < 0 ? Username : FriendUsername;
        var segundoUsername = Username.CompareTo(FriendUsername) < 0 ? FriendUsername : Username;
        var roomId = $"{primeroUsername}_{segundoUsername}";

        //obtenemos el chat de esa sala
        var chat = await chats.Find(c => c.Id == roomId).FirstOrDefaultAsync();
        if (chat == null)
        {
            return Results.NotFound("Sala de chat no encontrada");
        }
        // obtenemos los mensajes, los vamos a recorrer
        var mensajesSinLeer = chat.Mensajes.Where(m => m.UserName != Username && !m.IsRead).ToList();
        // Comprobamos si hay mensajes no leídos
        if (mensajesSinLeer.Count > 0)
        {
            // Actualizamos el estado de los mensajes a leídos
            foreach (var mensaje in mensajesSinLeer)
            {
                mensaje.IsRead = true;
            }
            // Actualizamos el chat en la base de datos
            await chats.ReplaceOneAsync(c => c.Id == roomId, chat);
        }
        // Return success result if everything went fine
        return Results.Ok("Mensajes actualizados correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al actualizar mensajes no leídos: {ex.Message}");
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
public class MakingFriendsModel
{
    public string UsuarioActual { get; set; }
    public string UsuarioAmigo { get; set; }
    public bool Accepted { get; set; } // Indica si la solicitud fue aceptada o no
}
