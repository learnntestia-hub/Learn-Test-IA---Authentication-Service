// space to define the UserRole enumeration, which represents different roles that users can have in the authentication service. This can be used to manage permissions and access control based on the user's role.
namespace AuthService.Domain.Enums;

// Enumeration to define user roles in the authentication service
public enum UserRole
{
    User = 0,
    Admin = 1,
}