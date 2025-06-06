namespace pruebaMudBlazor.Client.Services;
using pruebaMudBlazor.Client.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

public class ClienteService : IClienteService
{
    private readonly HttpClient _httpClient;
    private AuthService _authService;
    public ClienteService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<string> RegistrarClienteAsync(UserModel registro)
    {
        var response = await _httpClient.PostAsJsonAsync("/registro", registro);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
         else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new UnauthorizedAccessException("Email y/o contraseña existentes.");
        }else
        {
            throw new Exception("Error al registrar cliente");
        }
    }
    public async Task<UserModel> LoginClienteAsync(string email, string password)
    {
        var loginData = new
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/api/login", loginData);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Inicio de sesión recibido, procesando...");
            return await response.Content.ReadFromJsonAsync<UserModel>();
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error en login: {errorContent}");
            throw new Exception(errorContent);
        }
    }

    public async Task<string> CambiarFotoPerfilAsync(string username, string foto)
    {
        //Console.WriteLine("en servicio cambiar foto los datos son: " + username);
        var model = new
        {
            Username = username,
            Foto = foto
        };

        var response = await _httpClient.PostAsJsonAsync("/api/cambiarFotoPerfil", model);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al cambiar la foto de perfil: {errorContent}");
        }
    }

    public Task<List<UserModel>> BuscarUsuariosAsync(string nombreUsuario,string currentUser)
    {
        return _httpClient.GetFromJsonAsync<List<UserModel>>($"/api/buscarUsuarios?nombreUsuario={nombreUsuario}&currentUser={currentUser}");

    }

    public async Task<bool> AgregarAmigoAsync(string usuarioActual, string usuarioAmigo)
    {

        var model = new
        {
            UsuarioActual = usuarioActual,
            UsuarioAmigo = usuarioAmigo
        };

        var response = await _httpClient.PostAsJsonAsync("/api/agregarAmigo", model);

        if (response.IsSuccessStatusCode)
        {
            var resultado = await response.Content.ReadFromJsonAsync<ResponseServer>();
            if (resultado.CodigoError == 0)
            {
                //Console.WriteLine("Amigo agregado exitosamente.");
                return true;
            }
            else
            {
                //Console.WriteLine($"Error al agregar amigo: {resultado.Mensaje}");
                return false;
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error al agregar amigo: {errorContent}");
            throw new Exception($"Error al agregar amigo: {errorContent}");
        }
    }

    public async Task<List<Amigos>> ObtenerAmigosAsync(List<string> amigosUsernames)
    {
        try
        {

            var response = await _httpClient.PostAsJsonAsync("/api/obtenerAmigos", amigosUsernames);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Amigos>>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al obtener amigos: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ObtenerAmigosAsync: {ex.Message}");
            return new List<Amigos>();
        }
    }

    public Task<bool> IniciarChatAsync(string usuarioActual, string amigoUsername)
    {
        // Console.WriteLine("en servicio iniciar chat los datos son: " + usuarioActual + " " + amigoUsername);
        var req = new
        {
            user1 = usuarioActual,
            user2 = amigoUsername
        };

        return _httpClient.PostAsJsonAsync("/api/iniciarChat", req)
            .ContinueWith(task =>
            {
                if (task.Result.IsSuccessStatusCode)
                {
                    return true;//devolver el tema del response server
                }
                else
                {
                    return false;
                }
            });
    }

    public async Task<ResponseServer> GuardarMensajeAsync(ChatMessage mensaje, string roomId)
    {
        // Console.WriteLine("en servicio guardar mensaje los datos son: " + mensaje.UserName + " " + mensaje.Message);
        var response = await _httpClient.PostAsJsonAsync("/api/guardarMensaje", new { mensaje, roomId });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ResponseServer>();
        }
        else
        {
            throw new Exception("Error al guardar el mensaje");
        }
    }

    public Task<bool> EliminarAmigoAsync(string usuarioActual, string amigoUsername)
    {
        var model = new
        {
            UsuarioActual = usuarioActual,
            UsuarioAmigo = amigoUsername 
        };

        var response = _httpClient.PostAsJsonAsync("/api/eliminarAmigo", model);
        return response.ContinueWith(task =>
        {
            if (task.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
    }

    public Task<List<FriendRequest>> ObtenerNotisAsync(string username)
    {
        // Console.WriteLine("en servicio obtener notis los datos son: " + username);
        return _httpClient.GetFromJsonAsync<List<FriendRequest>>($"/api/obtenerNotificaciones?username={username}");
    }

    public Task<bool> MakeFriendAsync(string usuarioActual, string amigoUsername,bool accepted)
    {
        // Console.WriteLine($"Haciendo amigos: {usuarioActual} -> {amigoUsername}");

        var model = new
        {
            UsuarioActual = usuarioActual,
            UsuarioAmigo = amigoUsername,
            Accepted = accepted // Indica si la solicitud fue aceptada o no
        };

        return _httpClient.PostAsJsonAsync("/api/makeFriend", model)
            .ContinueWith(task =>
            {
                if (task.Result.IsSuccessStatusCode)
                {
                    return true; // Devolver el tema del response server
                }
                else
                {
                    return false;
                }
            });
    }

    public Task<Dictionary<string, int>> GetMensajesNoLeidosAsync(string username)
    {
        // Console.WriteLine("en servicio obtener mensajes no leidos los datos son: " + username);
        return _httpClient.GetFromJsonAsync<Dictionary<string, int>>($"/api/getMensajesNoLeidos?username={username}");
    }

   public Task<bool> ActualizarMensajesNoLeidosAsync(string username, string friendUsername, int mensajesNoLeidos)
{
    return _httpClient.PostAsync($"/api/actualizarMensajesNoLeidos?Username={username}&FriendUsername={friendUsername}&MensajesNoLeidos={mensajesNoLeidos}", null)
        .ContinueWith(task =>
        {
            if (task.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        });
}
}