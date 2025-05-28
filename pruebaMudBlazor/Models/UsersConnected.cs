using System.Collections.Concurrent;

public class UsersConnected
{
    public ConcurrentDictionary<string, string> ConnectedUsers { get; } = new();

    public void registConnection(string userName, string status)
    {
        ConnectedUsers[userName] = status;
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
}