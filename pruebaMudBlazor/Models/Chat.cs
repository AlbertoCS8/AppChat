namespace pruebaMudBlazor.Models;
public class Chat
{
    public string Id { get; set; } // el roomId
    public List<string> Participantes { get; set; } // IDs de usuarios
    public List<Mensaje> Mensajes { get; set; }
}

