namespace pruebaMudBlazor.Client.Models
{
    using FluentValidation;

    public class UserModel
    {
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string NombreUsuario { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmarPassword { get; set; } = "";
        public string Email { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public List<string> Amigos { get; set; } = new List<string>();
        //podible texto de estado como el estado del wsp no me dio time
             public override string ToString()
    {
        return $"Nombre: {Nombre}, Apellido: {Apellido}, Usuario: {NombreUsuario}, Email: {Email}";
    }
    }

    public class UserModelValidator : AbstractValidator<UserModel>
    {
        public UserModelValidator()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.");
            RuleFor(x => x.Apellido).NotEmpty().MinimumLength(3).WithMessage("Minimo 3 caracteres");
            RuleFor(x => x.NombreUsuario).NotEmpty().WithMessage("El nombre de usuario es obligatorio.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Debe ser un correo válido.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
            RuleFor(x => x.ConfirmarPassword).NotEmpty().Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<UserModel>.CreateWithOptions((UserModel)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    
    }
}