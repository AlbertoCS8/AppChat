namespace pruebaMudBlazor.Client.Models;

public class Amigos
{
    public Amigos()
    {
    }
    public Amigos(string username, string fotoPerfil, string status, string ultimaConexion)
    {
        Username = username;
        FotoPerfil = fotoPerfil;
        Status = status;
        UltimaConexion = ultimaConexion;
    }


    public string Username { get; set; }
    public string FotoPerfil { get; set; }
    public string Status { get; set; } = "Desconectado"; // "En línea" o "Desconectado"
    public string UltimaConexion { get; set; } = string.Empty; // Fecha y hora de la última conexión
    //problema: lo genera en puto UTC, y no en la hora local del usuario, uso api y si no una funcion local

     public override string ToString()
    {
        return $"Usuario: {Username}, Estado: {Status}, Última Conexión: {UltimaConexion:yyyy-MM-dd HH:mm:ss}";
    }
    
}