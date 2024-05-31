namespace API.DTOs;

public class UserDto
{
    public Guid Guid { get; set; }
    public string Login { get; set; }
    public string Name { get; set; }
    public string Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Admin { get; set; }
    public string Token { get; set; }
}
