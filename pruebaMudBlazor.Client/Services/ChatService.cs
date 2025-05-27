using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using pruebaMudBlazor.Client.Models;

namespace pruebaMudBlazor.Client.Services;
public class ChatService
{

    public HubConnection _hubConnection; // hace referencia al hub de chat del servidor 
                                         // de ahi que tenga esos metodos a los que puedo llamnar con el invokasync
                                         //Lo usamos para enviar mensajes y recibirlos
    public HubConnection _globalHubConnection; // Este lo usaremos para eventos globales como conexion de usuarios o desconexion
    public Action<ChatMessage> ObsMensajeRecibido { get; set; }// como los observables de angular!!
    public Action<string, string> ObsUserConnection { get; set; } //para el evento de conexion de usuarios
    private NavigationManager _navigationManager;
    public ChatService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    private string baseUrl => _navigationManager.BaseUri;
    public async Task ConnectToGlobalHub(string username)
    {
        // Descartar conexión previa si existe
        if (_globalHubConnection != null)
        {
            await _globalHubConnection.DisposeAsync();
        }
        Console.WriteLine("Conectando al hub global...");
        _globalHubConnection = new HubConnectionBuilder()
             .WithUrl($"{baseUrl}chathub")
            .WithAutomaticReconnect()
            .Build();

        //ponemos el _globalHub a la escucha de evenyos
        _globalHubConnection.On<string, string>("UserConnection", (username, status) =>
        {
            ObsUserConnection?.Invoke(username, status);
            // Notificar cambio de Status de usuarios --> En linea o desconectado...
        });
        await _globalHubConnection.StartAsync();
        await _globalHubConnection.InvokeAsync("UserConnection", $"{username}", "En línea");
    }
    //Método para desconectar del hub global
    public async Task DisconnectFromGlobalHub(string username)
    {
        try
        {
            if (_globalHubConnection != null && _globalHubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine($"Notificando desconexión del usuario {username}");

                // Notificar la desconexión antes de cerrar la conexión
                await _globalHubConnection.InvokeAsync("UserConnection", username, "Desconectado");

                // Detener la conexión
                await _globalHubConnection.StopAsync();
                await _globalHubConnection.DisposeAsync();
                _globalHubConnection = null;

                Console.WriteLine("Desconexión del hub global completada");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al desconectar del hub global: {ex.Message}");
        }
    }
    public async Task<HubConnection> ConnectToHub(string UserName, string roomId)
    {
        try
        {
            // Si ya hay una conexión activa, la reutilizamos
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                // Cambiar de sala si es necesario
                await _hubConnection.InvokeAsync("UnirASala", UserName, roomId);
                return _hubConnection;
            }

            // Descartar conexión previa si existe pero no está conectada
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            Console.WriteLine("Conectando al hub de chat...");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}chathub")
                .WithAutomaticReconnect()
                .Build();

            // Eventos que estan a la escucha, cuando desde Chathub se disparan con --> await Clients.OthersInGroup(roomName).SendAsync("ReceiveMessage", mensaje);
            //se ejecuta el codigo
            _hubConnection.On<ChatMessage>("ReceiveMessage", (ChatMessage mensaje) =>
            {
                Console.WriteLine($"Mensaje recibido: {mensaje.Message} de parte de {mensaje.UserName}");
                ObsMensajeRecibido?.Invoke(mensaje);
            });
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("UnirASala", UserName, roomId);
            Console.WriteLine("Conectado al hub de chat.");
            return _hubConnection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al conectar al hub: {ex.Message}");
            throw;
        }
    }
    public async Task SendMessage(string roomId, ChatMessage message)
    {
        try
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("EnviarMensaje", roomId, message);

            }
            else
            {
                Console.WriteLine("No estás conectado al hub de chat.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el mensaje: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ChatMessage>> GetChatMessagesAsync(string roomId, HubConnection connection = null)
    {
        try
        {
            // Usar la conexión proporcionada o la existente
            var hubConnection = connection ?? _hubConnection;

            if (hubConnection != null && hubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("Obteniendo mensajes de la sala...");
                var mensajes = await hubConnection.InvokeAsync<List<ChatMessage>>("ObtenerMensajes", roomId);
                Console.WriteLine($"Mensajes de la sala {roomId} obtenidos: {mensajes.Count} mensajes.");
                return mensajes;
            }
            else
            {
                Console.WriteLine("No hay conexión disponible al hub de chat.");
                return new List<ChatMessage>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener mensajes: {ex.Message}");
            return new List<ChatMessage>();
        }
    }

}