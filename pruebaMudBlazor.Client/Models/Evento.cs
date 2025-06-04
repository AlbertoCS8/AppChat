public class Evento
{
     public Evento()
    {
        // Initialize with default values
        Accepted = false;  // Default value
    }
    
    // Constructor for common properties
    public Evento(string userNameDestino, string mensaje, string tipo, string userNameOrigen)
    {
        UserNameDestino = userNameDestino;
        Mensaje = mensaje;
        Tipo = tipo;
        UserNameOrigen = userNameOrigen;
        Accepted = false;  // Default value
    }

    // Constructor that includes the Accepted property
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