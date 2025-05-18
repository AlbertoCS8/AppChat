using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using pruebaMudBlazor.Models;

public class ChatHub : Hub
{
    private readonly ChatResponsables _chatResponsables;
    private UsersConnected _usersConnected;
    private readonly IMongoCollection<Chat> _chatsCollection;    
    // Inyección por constructor
    public ChatHub(ChatResponsables chatResponsables, IMongoCollection<Chat> chatsCollection, UsersConnected usersConnected)
    {
        _chatResponsables = chatResponsables;
        _chatsCollection = chatsCollection;
        _usersConnected = usersConnected;
    }
     //funcion para enviar mensajes a un grupo, los guarda en BdD
    public async Task EnviarMensaje(string roomName,ChatMessage mensaje)
    {
        //Console.WriteLine($"Mensaje recibido: {mensaje.Message} de parte de {mensaje.UserName} en la sala {roomName}");
        if (_chatsCollection.Find(c => c.Id == roomName).CountDocuments() == 0) 
        {
        //si no existe la sala en la base de datos la creamos
        // esta consulta cada vez que se manda un mensaje nos la podemos ahorrar con una sola al unirse a la sala
        // guardando el resultado en un diccionario o algo asi y consultas el dicc cada vez que te unes a la sala
            var chat = new Chat { Id = roomName, Mensajes = new List<ChatMessage>() };
            _chatsCollection.InsertOne(chat);
        }
        // Guardar el mensaje en la base de datos
        await _chatsCollection.UpdateOneAsync(
            c => c.Id == roomName,
            Builders<Chat>.Update.Push(c => c.Mensajes, mensaje)
        );
        await Clients.OthersInGroup(roomName).SendAsync("ReceiveMessage", mensaje);

    }
    //Funcion principal de chatHub, aqui podemos ver la filosofia que utilizamos, unimos a usuarios
    // a salas de chat y enviamos mensajes a esas salas
    public async Task UnirASala(string UserName,string roomName) 
    {
    // nos conectamos desde el cliente a una sala de chat, 
    //el nombre de la sala son los usernames de ambos usuarios ordenados alfabéticamente para que no haya problemas con el orden
    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);        
    }
        //quitarrrrrrrrr borrraarrrrrr
    public string ObtenerResponsable(string roomName) //obtenemos el responsable de la sala
    {
        return _chatResponsables.GetResponsable(roomName);
    }
    //Obtenemos los mensajes de la sala de chat desde la base de datos
    public async Task<List<ChatMessage>> ObtenerMensajes(string roomName) //obtenemos los mensajes de la sala
    {
        var chat = await _chatsCollection.Find(c => c.Id == roomName).FirstOrDefaultAsync();
        if (chat != null && chat.Mensajes != null)
        {
            Console.WriteLine($"Mensajes de la sala {roomName} obtenidos: {chat.Mensajes.Count} mensajes.");
            return chat.Mensajes;
        }
        Console.WriteLine($"No se encontraron mensajes para la sala {roomName}.");
        return new List<ChatMessage>();
    }

    public async void UserConnection(string userName, string status) //registramos la conexion de un usuario
    {
        try
        {
            await Clients.Others.SendAsync("UserConnection", userName, status);
            _usersConnected.registConnection(userName, status);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al registrar la conexión: {ex.Message}");
        }
    }
    // flujo --> 
    // cliente clica en amigo y obtiene un roomname con los usernames de ambos usuarios ordenados alfabéticamente
    // cliente llama a la funcion UnirASala con el roomname (service en cliente)
    // servidor llama a la funcion UnirASala y se une a la sala de chat
    // servidor llama a la funcion EnviarMensaje y envia el mensaje al grupo (sala de chat) con el nombre de la sala y el mensaje
}