using System.ComponentModel.DataAnnotations;

namespace AgriStream.Web.Models;

public class LoginViewModel
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required, StringLength(50)] public string Username { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required] public string Role { get; set; } = "Farmer";
}