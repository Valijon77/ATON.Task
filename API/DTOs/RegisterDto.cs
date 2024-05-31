using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    [RegularExpression(
        "^[a-zA-Z0-9]*$",
        ErrorMessage = "Разрешены только латинские буквы и цифры."
    )]
    public string Login { get; set; }

    [Required]
    [RegularExpression(
        "^[a-zA-Z0-9]*$",
        ErrorMessage = "Разрешены только латинские буквы и цифры."
    )]
    public string Password { get; set; }

    [Required]
    [RegularExpression(@"^[a-zA-Zа-яА-Я ]*$", ErrorMessage = "Разрешены только латинские и русские буквы.")]
    public string Name { get; set; }

    [Required]
    [Range(0, 2)]
    public int Gender { get; set; }

    public DateTime Birthday { get; set; }

    [Required]
    public bool Admin { get; set; }
}
