namespace API.DTOs;

public class RequestByLoginDto
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool Active { get; set; }
}
