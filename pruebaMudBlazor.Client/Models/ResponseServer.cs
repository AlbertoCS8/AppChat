namespace pruebaMudBlazor.Client.Models;

public class ResponseServer<T>
{
    public string Mensaje { get; set; }
    public int CodigoError { get; set; } // 0 = sin error, 1 = error de servidor, 2 = error de cliente pendiente
    //implementar en todos los endpoints
    public T Datos { get; set; } // Datos tipados, pueden ser cualquier tipo
}

// respuestas sin datos, para los endpoints que no devuelven nada
public class ResponseServer
{
    public string Mensaje { get; set; }
    public int CodigoError { get; set; } // 0 = sin error, 1 = error de servidor, 2 = error de cliente
}