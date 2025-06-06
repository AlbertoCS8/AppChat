public class Evento
{
     public Evento()
    {
        Accepted = false;  
    }
    
    public Evento(string userNameDestino, string mensaje, string tipo, string userNameOrigen)
    {
        UserNameDestino = userNameDestino;
        Mensaje = mensaje;
        Tipo = tipo;
        UserNameOrigen = userNameOrigen;
        Accepted = false;  // Default value --> para la friend request
    }

    public Evento(string userNameDestino, string mensaje, string tipo, string userNameOrigen, bool accepted)
    {
        UserNameDestino = userNameDestino;
        Mensaje = mensaje;
        Tipo = tipo;
        UserNameOrigen = userNameOrigen;
        Accepted = accepted;
    }

    //nuevo objeto que tendra username de destino, mensaje y tipo (AgregarAmigo,mensajePendiente)
    public string UserNameDestino { get; set; }
    public string Mensaje { get; set; }
    public string Tipo { get; set; } // AgregarAmigo, MensajePendiente, etc.
    public string UserNameOrigen { get; set; }
    public bool Accepted { get; set; } // Indica si la solicitud fue aceptada o no

}