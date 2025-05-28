using System.Collections.Concurrent;

public class UsersConnected
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