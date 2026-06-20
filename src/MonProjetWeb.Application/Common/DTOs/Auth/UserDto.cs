using MonProjetWeb.Domain.Enums;

namespace MonProjetWeb.Application.Common.DTOs.Auth;

public class UserDto
{
    public int      Id        { get; set; }
    public string   FirstName { get; set; } = string.Empty;
    public string   LastName  { get; set; } = string.Empty;
    public string   Email     { get; set; } = string.Empty;
    public UserRole Role      { get; set; }
}