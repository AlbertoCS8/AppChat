using MongoDB.Driver;
using Microsoft.Extensions.Options;


public class MongoDbService
{
    private readonly IMongoCollection<Usuario> _usuarios;

    //inicializamos la base de datos y la coleccion de usuarios
    public MongoDbService(IOptions<MongoDbSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _usuarios = database.GetCollection<Usuario>("Usuarios");
    }

    public async Task<List<Usuario>> GetUsuarios() =>
        await _usuarios.Find(usuario => true).ToListAsync();
    
    public async Task<Usuario> GetUsuarioPorId(string id) =>
        await _usuarios.Find(usuario => usuario.Id == id).FirstOrDefaultAsync();
    //insertar un usuario en la base de datos
    public async Task CrearUsuario(Usuario usuario) =>
        await _usuarios.InsertOneAsync(usuario);
}
