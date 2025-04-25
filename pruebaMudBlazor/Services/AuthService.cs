namespace pruebaMudBlazor.Services;
public class AuthService
{
    public string Username { get; set; }
    public string ImagenBase64 { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<string> Amigos { get; set; } = new List<string>();


}