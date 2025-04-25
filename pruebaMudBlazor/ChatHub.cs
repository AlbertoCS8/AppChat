using Microsoft.AspNetCore.SignalR;
using pruebaMudBlazor.Models;
public class ChatHub : Hub
{
    public async Task SendMessage(string roomName,string user, string message) //funcion para enviar mensajes a un grupo
    {
        var mensaje = new Mensaje
        {
            EmisorId = user,
            Texto = message,
            Fecha = DateTime.Now
        };
        await Clients.Group(roomName).SendAsync("ReceiveMessage", mensaje);
    }
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the room {roomName}.");
    }
}