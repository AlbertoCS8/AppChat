using MudBlazor;
using pruebaMudBlazor.Client.Services;

//esto fue triple del ultimo dia, me tuve que poner a buscar por que me estaba duplicando un mensaje solo
//al eliminar un amigo y volverlo a agregar, y era por que me estaba suscribiendo dos veces al evento de notificacion
// pero en una parte inhóspita del codigo y buah... la que me lio, al final ha quedado guay
public class NotificacionService
{
    private readonly ChatService _chatService;
    private readonly AuthService _authService;
    private readonly ISnackbar _snackBar;
    private bool _yaSuscrito = false;

    public NotificacionService(ChatService chatService, AuthService authService, ISnackbar snackBar)
    {
        _chatService = chatService;
        _authService = authService;
        _snackBar = snackBar;

        Suscribir();
    }

    private void Suscribir()
    {
        if (_yaSuscrito) return;

        _chatService.ObsNotificacionRecibida += HandleNotificacion;
        _yaSuscrito = true;
    }

    private void HandleNotificacion(Evento notificacion)
    {//notific para que el cliente sepa que ha recibido una notificacion, y que hacer con ella
        Console.WriteLine($"[NOTIFICACION] Tipo: {notificacion.Tipo}");

        switch (notificacion.Tipo)
        {
            case "NuevoMensaje":
                if (_authService.MensajesNoLeidos.ContainsKey(notificacion.UserNameOrigen))
                {
                    //_snackBar.Add($"Tienes un nuevo mensaje de {notificacion.UserNameOrigen}, accede a el chat para leerlo", Severity.Info);
                    _authService.MensajesNoLeidos[notificacion.UserNameOrigen]++;
                }
                else
                    _authService.MensajesNoLeidos[notificacion.UserNameOrigen] = 1;

                _authService.NotificarCambio("NuevoMensaje");
                break;

            case "NuevoAmigo":
                _snackBar.Add("Tienes una nueva solicitud de amistad", Severity.Info); // solicitud de amistad
                break;

            case "AmigoAceptado":
                if (notificacion.Accepted)
                {
                    _snackBar.Add("Tu solicitud de amistad fue aceptada", Severity.Success);
                    _authService.Amigos.Add(notificacion.UserNameOrigen);
                    _authService.MensajesNoLeidos[notificacion.UserNameOrigen] = 0;
                    _authService.NotificarCambio("CambiosListaAmigos");
                }
                else
                    _snackBar.Add($"Tu solicitud fue rechazada por {notificacion.UserNameOrigen}", Severity.Error);
                break;

            case "AmigoBorrado":
                _snackBar.Add($"Has eliminado a {notificacion.UserNameOrigen} de tu lista de amigos", Severity.Warning);
                _authService.Amigos.Remove(notificacion.UserNameOrigen);
                _authService.MensajesNoLeidos.Remove(notificacion.UserNameOrigen);
                _authService.NotificarCambio("CambiosListaAmigos");
                break;

        }
    }
}
