using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UpdateUserDto
{
    [Required]
    public string Login { get; set; }

    [Required]
    [RegularExpression(
        @"^[a-zA-Zа-яА-Я ]*$",
        ErrorMessage = "Разрешены только латинские и русские буквы."
    )]
    public string Name { get; set; }

    [Range(0, 3), Required]
    public int Gender { get; set; }

    [Required]
    public DateTime Birthday { get; set; }
}
