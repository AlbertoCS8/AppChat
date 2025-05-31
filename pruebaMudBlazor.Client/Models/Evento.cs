public class Evento
{
    public Evento()
    {
    }
    // Constructor para inicializar el evento con los valores necesarios
    public Evento(string userNameDestino, string mensaje, string tipo, string userNameOrigen)
    {
        UserNameDestino = userNameDestino;
        Mensaje = mensaje;
        Tipo = tipo;
        UserNameOrigen = userNameOrigen;
    }

    //nuevo objeto que tendra username de destino, mensaje y tipo (AgregarAmigo,mensajePendiente)
    public string UserNameDestino { get; set; }
    public string Mensaje { get; set; }
    public string Tipo { get; set; } // AgregarAmigo, MensajePendiente, etc.
    public string UserNameOrigen { get; set; } // Opcional: si necesitas saber quién envió el evento

}