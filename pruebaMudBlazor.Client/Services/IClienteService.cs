namespace pruebaMudBlazor.Client.Services;
using pruebaMudBlazor.Client.Models;

public interface IClienteService
{
    Task<string> RegistrarClienteAsync(UserModel registro);
    //login
    Task<UserModel> LoginClienteAsync(string email, string password);

    Task<string> CambiarFotoPerfilAsync(string username, string foto);

    Task<List<UserModel>> BuscarUsuariosAsync(string nombreUsuario);
    //tarea para agregar amigos, devuelve un obj con un codigo de error o un mensaje

    Task<bool> AgregarAmigoAsync(string usuarioActual, string usuarioAmigo);

    //tarea para recuperar datos de amigos a traves de un List de usernames
    Task<List<Amigos>> ObtenerAmigosAsync(List<string> amigosUsernames);
    Task<List<FriendRequest>> ObtenerNotisAsync(string username);

    //tarea para giardar en bdd un mensaje
    Task<ResponseServer> GuardarMensajeAsync(ChatMessage mensaje, string roomId);

    //tarea para iniciar chat con un amigo, le pasamos usernamen del amigo y el usuario actual
    Task<bool> IniciarChatAsync(string usuarioActual, string amigoUsername);
    //tarea para eliminar un amigo, le pasamos el username del usuario actual y el amigo a eliminar
    Task<bool> EliminarAmigoAsync(string usuarioActual, string amigoUsername);
    //tarea para hacer aigos a dos usuarios 
    Task<bool> MakeFriendAsync(string usuarioActual, string amigoUsername);
}