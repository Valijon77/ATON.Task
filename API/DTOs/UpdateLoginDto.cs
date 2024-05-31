using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UpdateLoginDto
{
    [Required]
    public string Login { get; set; }

    [Required]
    [RegularExpression(
        "^[a-zA-Z0-9]*$",
        ErrorMessage = "Разрешены только латинские буквы и цифры."
    )]
    public string NewLogin { get; set; }
}
