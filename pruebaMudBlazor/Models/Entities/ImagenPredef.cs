//borrarrrrrr
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ImagenPredefinida // primero que use, no tenia sentido ninguno y no se me ocurrio y ya cuando me di cuenta
//meti un avatar tipico como los de bs icons 
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string path { get; set; }  // coleccion en la que unicamente tenemos el base64 de la imagen predefinida
    public string nombre { get; set; } // nombre de la imagen predefinida

}