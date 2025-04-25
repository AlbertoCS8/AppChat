//borrarrrrrr
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ImagenPredefinida
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string path { get; set; }  // coleccion en la que unicamente tenemos el base64 de la imagen predefinida
    public string nombre { get; set; } // nombre de la imagen predefinida
    
}