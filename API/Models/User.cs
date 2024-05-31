using System.ComponentModel.DataAnnotations;
using API.Enums;

namespace API.Models;

public class User
{
    [Key]
    public Guid Guid { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public Gender Gender { get; set; } // Info: Male - 0, Female - 1, Unknown - 2
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}
