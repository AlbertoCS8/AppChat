using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace pruebaMudBlazor.Client.Services;
public class ChatService
{
    private readonly NavigationManager _navigationManager;
  public HubConnection _hubConnection;
  public string _userId { get; set; } = "jhasdjfhjkfhjkhdfjkhjkhdf";
     public async Task ConnectToHub(string roomId)
        {
            try
            {
                // Crear la conexión al hub
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri("/chatHub"))
                    .WithAutomaticReconnect()
                    .Build();
                
                // Configurar el manejador para recibir mensajes
                _hubConnection.On<string, string, string>("ReceiveMessage", (user, message, room) => {
                    Console.WriteLine($"Mensaje recibido de {user} en sala {room}: {message}");
                    // Aquí puedes disparar un evento o realizar otra acción
                });
                
                // Iniciar la conexión
                await _hubConnection.StartAsync();
                Console.WriteLine("Conectado al hub");
                
                // Unirse a la sala específica
                
                Console.WriteLine($"Unido a la sala: {roomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar al hub: {ex.Message}");
                Console.WriteLine($"Error al conectar al hub: {ex}");   
                throw;
            }
        }
    public async Task recibir()
    {
        Console.WriteLine("recibiendo mensajesfdsuifhasuiofhui...");
    }

}