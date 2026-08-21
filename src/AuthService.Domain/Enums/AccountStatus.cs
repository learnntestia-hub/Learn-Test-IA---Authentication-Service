using System.Text.Json.Serialization;

namespace AuthService.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountStatus
{
    Pending = 0,
    Active = 1,
    Blocked = 2
}