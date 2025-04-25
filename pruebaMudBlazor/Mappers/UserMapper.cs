using pruebaMudBlazor.Models;
//bcrypt



public static class UserMapper
{
    public static Usuario MapToUsuario(UserModel userModel)
    {
        return new Usuario
        {
            Nombre = userModel.Nombre,
            Apellido = userModel.Apellido,
            NombreUsuario = userModel.NombreUsuario,
            Email = userModel.Email,
            PasswordHash = HashPassword(userModel.Password), 
            Amigos = new List<string>(), //sera un array de usernames que en nuestro caso seran unicos tambien
            FotoPerfil = ""
        };
    }
    public static UserModel MapToUserModel(Usuario usuario)
    {
        return new UserModel
        {
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            FotoPerfil = usuario.FotoPerfil,
            Amigos = usuario.Amigos
        };
    }

    private static string HashPassword(string password)
    {
        
        return BCrypt.Net.BCrypt.HashPassword(password); 
    }
}
