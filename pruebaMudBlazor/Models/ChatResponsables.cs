namespace pruebaMudBlazor.Models;
public class ChatResponsables
{
    private readonly Dictionary<string, string> responsables = new();

    public bool TryAsignarResponsable(string roomId, string userName)
    {
        if (!responsables.ContainsKey(roomId))
        {
            responsables[roomId] = userName;
            return true; // Se asignó exitosamente
        }
        return false; // Ya había uno
    }

    public string GetResponsable(string roomId)
    {
        return responsables.TryGetValue(roomId, out var responsable) ? responsable : null;
    }
}