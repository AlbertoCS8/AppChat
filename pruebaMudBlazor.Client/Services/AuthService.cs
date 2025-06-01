public class AuthService
{
    public string Username { get; set; }
    public string ImagenBase64 { get; set; }
    public bool IsAuthenticated { get; set; }
    public string Email { get; set; } = "";

    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public List<string> Amigos { get; set; } = new List<string>();
    public Dictionary<string, int> MensajesNoLeidos { get; set; } = new Dictionary<string, int>();

    public event Action<string> CambiosEnComponente;

    public void NotificarCambio(string tipoCambio)
    {
        CambiosEnComponente?.Invoke(tipoCambio);
    }

}