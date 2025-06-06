public class FriendRequest
{
    public string Username { get; set; } // ID de la solicitud de amistad
    public string SenderUsername { get; set; } // ID del usuario que envió la solicitud
    public string Message { get; set; } // Mensaje opcional en la solicitud --> podria haberlo reusao de alguna manera
    //para meterle previsualizacion al mensaje o algo, puedo implementar en un futuro
    
    
}