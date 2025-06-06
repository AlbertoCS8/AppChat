using System.Collections.Concurrent;

public class UsersConnected //--> instantanea de lo que habra en la parte del servidor, para que el cliente pueda ver los usuarios conectados
{
    public ConcurrentDictionary<string, string> ConnectedUsers { get; } = new();

    public void registConnection(string userName, string status)
    {
        ConnectedUsers[userName] = status;
    }

    public void registDisconnect(string userName)
    {
        ConnectedUsers.TryRemove(userName, out _);
    }
    public bool isConnected(string userName)
    {
        return ConnectedUsers[userName] == "Connected" ? true : false;
    }
}