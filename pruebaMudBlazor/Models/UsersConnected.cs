using System.Collections.Concurrent;

public class UsersConnected
{
    // Mapeo de usuarios al estado (en línea, desconectado)
    public ConcurrentDictionary<string, string> ConnectedUsers { get; } = new();
    
    // Nuevo: Mapeo de usuarios a su ConnectionId
    private ConcurrentDictionary<string, string> _userConnectionIds = new();

    // Registrar conexión con estado
    public void registConnection(string userName, string status)
    {
        ConnectedUsers[userName] = status;
    }

    // Nuevo: Registrar conexión con ConnectionId
    public void RegisterConnectionId(string userName, string connectionId)
    {
        _userConnectionIds[userName] = connectionId;
        // También actualiza el estado si lo necesitas
        ConnectedUsers[userName] = "En línea";
    }

    // Nuevo: Eliminar conexión cuando un usuario se desconecta
    public void RemoveConnection(string userName)
    {
        _userConnectionIds.TryRemove(userName, out _);
        ConnectedUsers[userName] = "Desconectado";
    }

    // Método para obtener el ConnectionId por nombre de usuario
    public string GetConnectionIdByUsername(string username)
    {
        if (_userConnectionIds.TryGetValue(username, out string connectionId))
        {
            return connectionId;
        }
        return null; // Usuario no conectado o no encontrado
    }

    public string GetUserStatus(string username)
    {
        if (ConnectedUsers.TryGetValue(username, out string status))
        {
            return status;
        }
        return "Desconectado"; // Estado por defecto
    }

    public Dictionary<string, string> GetAllUsersStatus()
    {
        return new Dictionary<string, string>(ConnectedUsers);
    }
    
    // Nuevo: Obtener todos los usuarios conectados con sus ConnectionIds
    public Dictionary<string, string> GetAllConnectedUsers()
    {
        return new Dictionary<string, string>(_userConnectionIds);
    }
}