using Microsoft.AspNetCore.SignalR;
using pruebaMudBlazor.Models;
public class ChatHub : Hub
{
    public async Task EnviarMensaje(string roomName,string user, string message) //funcion para enviar mensajes a un grupo
    {
        var mensaje = new Mensaje
        {
            EmisorId = user,
            Texto = message,
            Fecha = DateTime.Now
        };
        await Clients.Group(roomName).SendAsync("ReceiveMessage", mensaje);
    }
    public async Task UnirASala(string roomName) // nos conectamos desde el cliente a una sala de chat, 
    //el nombre de la sala son los usernames de ambos usuarios ordenados alfabéticamente para que no haya problemas con el orden

    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the room {roomName}.");
    }

    // flujo --> 
    // cliente clica en amigo y obtiene un roomname con los usernames de ambos usuarios ordenados alfabéticamente
    // cliente llama a la funcion UnirASala con el roomname (service en cliente)
    // servidor llama a la funcion UnirASala y se une a la sala de chat
    // servidor llama a la funcion EnviarMensaje y envia el mensaje al grupo (sala de chat) con el nombre de la sala y el mensaje
}