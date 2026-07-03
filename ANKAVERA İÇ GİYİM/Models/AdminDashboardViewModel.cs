namespace ANKAVERA_İÇ_GİYİM.Models;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int AdminCount { get; set; }
    public int UserCount { get; set; }
    public List<ApplicationUser> RecentUsers { get; set; } = new();
}