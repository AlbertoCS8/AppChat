
namespace pruebaMudBlazor.Models
{
    public class ChatMessage
    {
        //public string profileImg { get; set; } //para renderizar la img de perfil
        public string UserName { get; set; } 
        public string Message { get; set; }
        public string Time { get; set; } // Formato de hora: "HH:mm"
    }
}