using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using pruebaMudBlazor.Client.Models;

namespace pruebaMudBlazor.Client.Services;

public class ChatService : IDisposable
{

    public HubConnection _hubConnection; // hace referencia al hub de chat del servidor 
                                         // de ahi que tenga esos metodos a los que puedo llamnar con el invokasync
                                         //Lo usamos para enviar mensajes y recibirlos
    public HubConnection _globalHubConnection; // Este lo usaremos para eventos globales como conexion de usuarios o desconexion
    public Action<ChatMessage> ObsMensajeRecibido { get; set; }// como los observables de angular!!
    public Action<string, string> ObsUserConnection { get; set; } //para el evento de conexion de usuarios
    public Action<Evento> ObsNotificacionRecibida { get; set; } //para el evento de notificaciones globales
                                                                // private NavigationManager _navigationManager;
                                                                // public ChatService(NavigationManager navigationManager)
                                                                // {
                                                                //     _navigationManager = navigationManager;
                                                                // }
                                                                // private string baseUrl => _navigationManager.BaseUri;
    private NavigationManager  _navigationManager;
    public ChatService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
    public async Task ConnectToGlobalHub(string username)
    {
        if (_globalHubConnection != null)
        {
            await _globalHubConnection.DisposeAsync();
        }
        Console.WriteLine("Conectando al hub global...");
        _globalHubConnection = new HubConnectionBuilder()
             .WithUrl($"{_navigationManager.BaseUri}chathub") 
            .WithAutomaticReconnect()
            .Build();

        //ponemos el _globalHub a la escucha de evenyos
        _globalHubConnection.On<string, string>("UserConnection", (username, status) =>
        {
            ObsUserConnection?.Invoke(username, status);
            // Notificar cambio de Status de usuarios --> En linea o desconectado...
        });

        // Añadir la suscripción una sola vez
        _globalHubConnection.On<Evento>("ReceiveNotification", (Evento evento) =>
        {
            ObsNotificacionRecibida?.Invoke(evento);
        });

        await _globalHubConnection.StartAsync();
        await _globalHubConnection.InvokeAsync("UserConnection", $"{username}", "En línea");
    }
    public async Task DisconnectFromGlobalHub(string username)
    {
        try
        {
            if (_globalHubConnection != null && _globalHubConnection.State == HubConnectionState.Connected)
            {
                //Console.WriteLine($"Notificando desconexión del usuario {username}");
                await _globalHubConnection.InvokeAsync("UserConnection", username, "Desconectado");

                await _globalHubConnection.StopAsync();
                await _globalHubConnection.DisposeAsync();
                _globalHubConnection = null;

                //Console.WriteLine("Desconexión del hub global completada");
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
                await _hubConnection.InvokeAsync("UnirASala", UserName, roomId);
                return _hubConnection;
            }

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }

            //Console.WriteLine("Conectando al hub de chat...");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_navigationManager.BaseUri}chathub")
                .WithAutomaticReconnect()
                .Build();

            // Eventos que estan a la escucha, cuando desde Chathub se disparan con --> await Clients.OthersInGroup(roomName).SendAsync("ReceiveMessage", mensaje);
            //se ejecuta el codigo
            _hubConnection.On<ChatMessage>("ReceiveMessage", (ChatMessage mensaje) =>
            {
                //Console.WriteLine($"Mensaje recibido: {mensaje.Message} de parte de {mensaje.UserName}");
                ObsMensajeRecibido?.Invoke(mensaje);
            });
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("UnirASala", UserName, roomId);
            //Console.WriteLine("Conectado al hub de chat.");
            return _hubConnection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al conectar al hub: {ex.Message}");
            throw;
        }
    }
    public async Task SendMessage(string roomId, ChatMessage message, string SelectedUser)
    {
        try
        {
            //Console.WriteLine($"memnsje a enviar: {message.Message} de {message.UserName} estado {message.IsRead}");
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("EnviarMensaje", roomId, message, SelectedUser);

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
            var hubConnection = connection ?? _hubConnection;

            if (hubConnection != null && hubConnection.State == HubConnectionState.Connected)
            {
                //Console.WriteLine("Obteniendo mensajes de la sala...");
                var mensajes = await hubConnection.InvokeAsync<List<ChatMessage>>("ObtenerMensajes", roomId);
                //Console.WriteLine($"Mensajes de la sala {roomId} obtenidos: {mensajes.Count} mensajes.");
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
    public async Task EnviarNotificacionAlGlobalHub(Evento mensaje)//vestigio de lo que hacia antes, -->
    //los eventos como envios de mensajes a usuarios los iba a enviar por el global a todos y que ya cuando ellos recibieran
    //el mensaje, filtraran si eran para ellos o no, muy ineficiente, pero despues de darle vueltas y sobretodo
    //aprender bien a usar signalR pues mandaremos el mensaje a un usuario concreto por su connectionId almacenado en un
    //diccionario de usuarios conectados en ese momento en la app
    {
        try
        {
            if (_globalHubConnection != null && _globalHubConnection.State == HubConnectionState.Connected)
            {
                await _globalHubConnection.InvokeAsync("EnviarNotificacion", mensaje);
            }
            else
            {
                Console.WriteLine("No estás conectado al hub global.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar la notificación: {ex.Message}");
            throw;
        }
    }
    public async Task LeaveRoomAsync(string roomName)
{
    try
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveRoom", roomName);
            Console.WriteLine($"Left room: {roomName}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error leaving room: {ex.Message}");
    }
}

// Esto es porque el ultimo dia me dio un monton de problemas con un observable duplicado..
private readonly HashSet<Action<Evento>> _notificacionSuscriptores = new();
private readonly HashSet<Action<ChatMessage>> _mensajeSuscriptores = new();
private readonly HashSet<Action<string, string>> _conexionSuscriptores = new();

// Agregar suscriptor de notificaciones con manejo de duplicados
public void SuscribirNotificacion(Action<Evento> handler)
{
    if (!_notificacionSuscriptores.Contains(handler))
    {
        ObsNotificacionRecibida += handler;
        _notificacionSuscriptores.Add(handler);
        Console.WriteLine($"Suscripción añadida. Total: {_notificacionSuscriptores.Count}");
    }
}

// Eliminar suscriptor de notificaciones, hace un commit no se manejaban aqui, se manejaban en el MainLaout las suscripciones
public void DesuscribirNotificacion(Action<Evento> handler)
{
    if (_notificacionSuscriptores.Contains(handler))
    {
        ObsNotificacionRecibida -= handler;
        _notificacionSuscriptores.Remove(handler);
        Console.WriteLine($"Suscripción eliminada. Total: {_notificacionSuscriptores.Count}");
    }
}

// Método para depuración
public int ContarSuscriptoresNotificacion()
{
    return _notificacionSuscriptores.Count;
}

// Implementación de IDisposable
public void Dispose()
{
    Console.WriteLine("Liberando recursos del ChatService...");

    // Limpiar conexiones
    if (_hubConnection != null)
    {
        _hubConnection.DisposeAsync().AsTask().Wait();
        _hubConnection = null;
    }

    if (_globalHubConnection != null)
    {
        _globalHubConnection.DisposeAsync().AsTask().Wait();
        _globalHubConnection = null;
    }

    // Limpiar eventos
    ObsMensajeRecibido = null;
    ObsUserConnection = null;
    ObsNotificacionRecibida = null;

    // Limpiar listas de suscriptores
    _notificacionSuscriptores.Clear();
    _mensajeSuscriptores.Clear();
    _conexionSuscriptores.Clear();
}
}