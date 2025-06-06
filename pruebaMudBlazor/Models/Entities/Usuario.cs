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
    public List<string> Amigos { get; set; } = new List<string>(); // Lista usernames de amigos, no habrá usernames
    // duplicados en BBDD asique los podremos usar a modo de id

    [BsonElement("fotoPerfil")]
    public string FotoPerfil { get; set; } // contenido imagen en base64
    [BsonElement("ultimaConexion")]
    public string UltimaConexion { get; set; } = string.Empty; // fecha y hora de la ultima conexion --> nos evitammos 
    // formatos de fechas en la medida de lo posible 
    [BsonElement("notificaciones")]
    public List<FriendRequest> Notificaciones { get; set; } = new List<FriendRequest>(); // Lista de notificaciones(FriendRequest)

    [BsonElement("FriendRequestEnviada")]
    public List<FriendRequest> FriendRequestEnviada { get; set; } = new List<FriendRequest>(); // Lista de solicitudes de 
    // amistad enviadas, las guardamos para asi evitar que se envien solicitudes duplicadas
    
    
}
