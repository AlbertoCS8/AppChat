using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


public class Usuario
{
    [BsonId] 
    [BsonRepresentation(BsonType.ObjectId)] 
    public string Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("apellido")]
    public string Apellido { get; set; }

    [BsonElement("nombreUsuario")]
    public string NombreUsuario { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("passwordHash")] // --> siempre hash
    public string PasswordHash { get; set; }

    [BsonElement("amigos")]
    public List<string> Amigos { get; set; } = new List<string>(); // (idea) Lista de IDs de amigos

    [BsonElement("fotoPerfil")]
    public string FotoPerfil { get; set; } // contenido imagen en base64
    
    
}
