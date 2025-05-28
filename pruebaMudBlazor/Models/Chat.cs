using MongoDB.Bson.Serialization.Attributes;

namespace pruebaMudBlazor.Models;
public class Chat
{
    [BsonElement("Id")]
    public string Id { get; set; } // el roomId
    public List<ChatMessage> Mensajes { get; set; }
}

