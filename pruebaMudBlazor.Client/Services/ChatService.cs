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
    public Action<ChatMessage> ObsMensajeRecibido { get; set; } // como los observables de angular!!
    public async Task ConnectToHub(string roomId)
    {

        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            Console.WriteLine("Conectando al hub de chat...");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5235/chathub")
                .Build();

            // Configura la conexión para unirse a la sala de chat
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("UnirASala", roomId);


            // // Configura los eventos del hub aquí, si es necesario
            _hubConnection.On<ChatMessage>("ReceiveMessage", ( ChatMessage mensaje) =>
            {
                Console.WriteLine($"Mensaje recibido: {mensaje.Message} de parte de {mensaje.UserName} en la sala {roomId}");
                ObsMensajeRecibido?.Invoke(mensaje); // notificamos a los suscritos al observable

            });

            Console.WriteLine("Conectado al hub de chat.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al conectar al hub: {ex.Message}");
            Console.WriteLine($"Error al conectar al hub: {ex}");
            throw;
        }
    }
    public async Task SendMessage(string roomId,ChatMessage message)
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
    public void notificarFrontend(string mensaje)
    {
        // Aquí puedes implementar la lógica para notificar al frontend sobre el nuevo mensaje
        // Por ejemplo, puedes usar un evento o un callback para actualizar la interfaz de usuario

    }


}