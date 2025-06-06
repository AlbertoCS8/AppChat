using MongoDB.Bson.Serialization.Attributes;

namespace pruebaMudBlazor.Models;
public class Chat
{
    [BsonElement("Id")]
    public string Id { get; set; } // el roomId username1_username2 ordenados alfabeticamente
    public List<ChatMessage> Mensajes { get; set; }
}

