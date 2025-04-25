namespace MyApplication.Shared.Models;
//probar meter en shared y usarlos desde ambos proyectos, al principio no me los detectaba
using FluentValidation;

public class UserModel
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmarPassword { get; set; } = "";

    public string Email { get; set; } = "";
    public bool AceptaTerminos { get; set; } = false;
}

public class UserModelValidator : AbstractValidator<UserModel>
{
    public UserModelValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Debe ser un correo válido.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");
        RuleFor(x => x.ConfirmarPassword).Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
        RuleFor(x => x.AceptaTerminos).Equal(true).WithMessage("Debe aceptar los términos y condiciones.");
        
    }
}
