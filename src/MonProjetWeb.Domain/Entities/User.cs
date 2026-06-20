using MonProjetWeb.Domain.Common;
using MonProjetWeb.Domain.Enums;

namespace MonProjetWeb.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role    { get; set; } = UserRole.Viewer;
    public bool IsActive    { get; set; } = true;

    // Navigation
    public ICollection<GcpAccount> GcpAccounts { get; set; } = new List<GcpAccount>();
}