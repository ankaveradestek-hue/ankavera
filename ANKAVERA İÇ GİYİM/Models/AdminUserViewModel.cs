namespace ANKAVERA_İÇ_GİYİM.Models;

public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime RegisteredAt { get; set; }
    public bool IsLocked { get; set; }
}