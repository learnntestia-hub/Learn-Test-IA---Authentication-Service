using System.Text.Json.Serialization;

namespace AuthService.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UsageType
{
    Personal = 0,
    Student = 1,
    Business = 2
}