using MongoDB.Bson;
using MongoDB.Driver;
using pruebaMudBlazor.Models;
using pruebaMudBlazor.Client.Models;
using Microsoft.AspNetCore.Mvc;

namespace pruebaMudBlazor.Controladores
{
    public static class UsuarioController
    {
        public static void MapUsuarioEndpoints(WebApplication app)
        {
            // Endpoint para registrar un usuario
            app.MapPost("/registro", RegistrarUsuario);
            
            // Endpoint para iniciar sesión
            app.MapPost("/api/login", IniciarSesion);
            
            // Endpoint para cambiar la foto de perfil
            app.MapPost("/api/cambiarFotoPerfil", CambiarFotoPerfil);
            
            // Endpoint para buscar usuarios por nombre de usuario
            app.MapGet("/api/buscarUsuarios", BuscarUsuarios);
            
            // Endpoint para obtener notificaciones de un usuario
            app.MapGet("/api/obtenerNotificaciones", ObtenerNotificaciones);
            
            // Endpoint para obtener mensajes no leídos de un usuario
            app.MapGet("/api/getMensajesNoLeidos", ObtenerMensajesNoLeidos);
        }

        private static async Task<IResult> RegistrarUsuario(pruebaMudBlazor.Models.UserModel registro,
            IMongoCollection<Usuario> usuarios)
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
        }

        private static async Task<IResult> IniciarSesion(LoginModel loginModel,
            IMongoCollection<Usuario> usuarios,
            Rest _rest)
        {
            var usuario = await usuarios.Find(u => u.Email == loginModel.Email).FirstOrDefaultAsync();
            
            if (usuario != null && BCrypt.Net.BCrypt.Verify(loginModel.Password, usuario.PasswordHash))
            {
                // Actualizamos la última conexión del usuario
                Console.WriteLine($"Hora de madrid: {await _rest.GetMadridTimeFormatted()}");
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
        }

        private static async Task<IResult> CambiarFotoPerfil(FotoPerfilModel model,
            IMongoCollection<Usuario> usuarios)
        {
            var usuario = await usuarios.Find(u => u.NombreUsuario == model.Username).FirstOrDefaultAsync();
            
            if (usuario != null)
            {
                usuario.FotoPerfil = model.Foto;
                await usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
                return Results.Ok("Foto de perfil actualizada");
            }
            else
            {
                return Results.NotFound("Usuario no encontrado");
            }
        }

        private static async Task<IResult> BuscarUsuarios(string nombreUsuario, string currentUser,
            IMongoCollection<Usuario> usuarios)
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
        }

        private static async Task<IResult> ObtenerNotificaciones(string username,
            IMongoCollection<Usuario> usuarios)
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
        }

        private static async Task<IResult> ObtenerMensajesNoLeidos(string username,
            IMongoCollection<Chat> chats, IMongoCollection<Usuario> users)
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
        }
    }

    public static class AmigosController
    {
        public static void MapAmigosEndpoints(WebApplication app)
        {
            // Endpoint para mandar una solicitud de amistad
            app.MapPost("/api/agregarAmigo", AgregarAmigo);
            
            // Endpoint para obtener amigos
            app.MapPost("/api/obtenerAmigos", ObtenerAmigos);
            
            // Endpoint para eliminar amigo
            app.MapPost("/api/eliminarAmigo", EliminarAmigo);
            
            // Endpoint para hacer amigos a dos usuarios
            app.MapPost("/api/makeFriend", MakeFriend);
        }

        private static async Task<IResult> AgregarAmigo(AmigoModel model, IMongoCollection<Usuario> usuarios)
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
                
                // Verificar si ya son amigos
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
                    Username = usuarioAmigo.NombreUsuario,
                    SenderUsername = usuarioActual.NombreUsuario,
                    Message = $"{usuarioActual.Nombre} {usuarioActual.Apellido} quiere ser tu amigo"
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

                usuarioAmigo.Notificaciones.Add(new FriendRequest
                {
                    Username = usuarioAmigo.NombreUsuario, 
                    SenderUsername = usuarioActual.NombreUsuario,
                    Message = $"{usuarioActual.Nombre} {usuarioActual.Apellido} quiere ser tu amigo" 
                });
                
                // Actualizar el usuario amigo en la base de datos
                await usuarios.ReplaceOneAsync(u => u.Id == usuarioAmigo.Id, usuarioAmigo);
                
                // Actualizar el usuario actual en la base de datos
                await usuarios.ReplaceOneAsync(u => u.Id == usuarioActual.Id, usuarioActual);
                
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
        }

        private static async Task<IResult> ObtenerAmigos(List<string> amigosUsernames, 
            IMongoCollection<Usuario> usuarios, UsersConnected usersConnected)
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
        }

        private static async Task<IResult> EliminarAmigo(AmigoModel model,
            IMongoCollection<Usuario> usuarios, IMongoCollection<Chat> chatsCollection)
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
                
                //borramos los chats que existan entre ambos usuarios coleccion chats
                var primeroUsername = model.UsuarioActual.CompareTo(model.UsuarioAmigo) < 0 ? model.UsuarioActual : model.UsuarioAmigo;
                var segundoUsername = model.UsuarioActual.CompareTo(model.UsuarioAmigo) < 0 ? model.UsuarioAmigo : model.UsuarioActual;
                var roomId = $"{primeroUsername}_{segundoUsername}";
                
                // Eliminamos el chat de la colección
                chatsCollection.DeleteOne(c => c.Id == roomId);
                
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
        }

        private static async Task<IResult> MakeFriend(MakingFriendsModel model,
            IMongoCollection<Usuario> usuarios)
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
        }
    }

    public static class ChatController
    {
        public static void MapChatEndpoints(WebApplication app)
        {
            // Endpoint para guardar un mensaje en la base de datos
            app.MapPost("/api/guardarMensaje", GuardarMensaje);
            
            // Endpoint para actualizar mensajes no leídos de un usuario
            app.MapPost("/api/actualizarMensajesNoLeidos", ActualizarMensajesNoLeidos);
        }

        private static async Task<IResult> GuardarMensaje(pruebaMudBlazor.Models.ChatMessage mensaje, string roomId)
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
        }

        private static async Task<IResult> ActualizarMensajesNoLeidos(string Username, string FriendUsername, int MensajesNoLeidos,
            IMongoCollection<Chat> chats, IMongoCollection<Usuario> users)
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
        }
    }
}