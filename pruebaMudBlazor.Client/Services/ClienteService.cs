namespace pruebaMudBlazor.Client.Services;
using pruebaMudBlazor.Client.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

public class ClienteService : IClienteService
{
    private readonly HttpClient _httpClient;

    public ClienteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> RegistrarClienteAsync(UserModel registro)
    {
        var response = await _httpClient.PostAsJsonAsync("/registro", registro);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new Exception("Error al registrar cliente");
        }
    }
    public async Task<UserModel> LoginClienteAsync(string email, string password)
    {
        Console.WriteLine("en servicio login los datos son: "+email+" "+password);
        
        // Crea un objeto de login explícito
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
    Console.WriteLine("en servicio cambiar foto los datos son: " + username);
    
    // Enviar los datos como un objeto JSON
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

    public Task<List<UserModel>> BuscarUsuariosAsync(string nombreUsuario)
    {
        // Console.WriteLine("en servicio buscar usuarios los datos son: " + nombreUsuario);
        return _httpClient.GetFromJsonAsync<List<UserModel>>($"/api/buscarUsuarios?nombreUsuario={nombreUsuario}");

    }

   // En ClienteService.cs
public async Task<bool> AgregarAmigoAsync(string usuarioActual, string usuarioAmigo)
{
    Console.WriteLine($"Agregando amigo: {usuarioActual} -> {usuarioAmigo}");
    
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
            Console.WriteLine("Amigo agregado exitosamente.");
            return true;
        }
        else
        {
            Console.WriteLine($"Error al agregar amigo: {resultado.Mensaje}");
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
}