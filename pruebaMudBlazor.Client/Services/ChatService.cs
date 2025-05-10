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
    public HubConnection _globalHubConnection; // Este lo usaremos para eventos globales como conexion de usuarios
    public Action<ChatMessage> ObsMensajeRecibido { get; set; }// como los observables de angular!!
    public Action <string, string> ObsUserConnection { get; set; } //para el evento de conexion de usuarios
    
    public async Task ConnectToGlobalHub(string username){
    // Descartar conexión previa si existe
    if (_globalHubConnection != null)
    {
        await _globalHubConnection.DisposeAsync();
    }
    Console.WriteLine("Conectando al hub global...");
    _globalHubConnection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5235/chathub")
        .WithAutomaticReconnect()
        .Build();  
    
    //ponemos el _globalHub a la escucha de evenyos
    _globalHubConnection.On<string, string>("UserConnection", (username, status) => {
        ObsUserConnection?.Invoke(username, status);
        // Notificar cambio de estado de usuarios
    });
    await _globalHubConnection.StartAsync();
    await _globalHubConnection.InvokeAsync("UserConnection", $"{username}", "En línea");
    // No unirse a ninguna sala específica
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
                .WithUrl("http://localhost:5235/chathub")
                .WithAutomaticReconnect() // Añadir reconexión automática
                .Build();

            // Configura los eventos del hub
            _hubConnection.On<ChatMessage>("ReceiveMessage", (ChatMessage mensaje) =>
            {
                Console.WriteLine($"Mensaje recibido: {mensaje.Message} de parte de {mensaje.UserName}");
                ObsMensajeRecibido?.Invoke(mensaje);
            });

            // Iniciar conexión
            await _hubConnection.StartAsync();
            // refactor de planteamniento, esto deberia estar en el login no aqui porque me cargaria el tema de el poder ver a todos los usuarios on line


            // Unirse a la sala
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
    public async Task SendMessage(string roomId, ChatMessage message) //sobra porque guardo en ChatHUb
    {
        try                                     //este user es quien lo manda
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("EnviarMensaje", roomId, message);//funcion de el chatHUb en la parte del server

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
    //metodo que recibe como parametro el username para guardarlo en el diccionario de usuarios conectados
    //lo llamamos una vez hacemos login
    public async void RegistrarConexion(string userName, string status)
    {
        try{
            await _hubConnection.InvokeAsync("RegistrarConexion", userName, status);

        }catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar la conexión: {ex.Message}");
        }
    }
    // public async Task<string> ObtenerResponsableAsync(string roomId)
    // {
    //     if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
    //     {
    //         try
    //         {
    //             var responsable = await _hubConnection.InvokeAsync<string>("ObtenerResponsable", roomId);
    //             Console.WriteLine($"Responsable de la sala {roomId} es: {responsable}");
    //             return responsable;
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"Error al obtener responsable: {ex.Message}");
    //             return null;
    //         }
    //     }
    //     else
    //     {
    //         Console.WriteLine("No estás conectado al hub de chat.");
    //         return null;
    //     }
    // }
}