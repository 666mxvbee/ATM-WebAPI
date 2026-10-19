using System.ComponentModel.DataAnnotations;

namespace Atm.Application.Options;

public sealed class SessionOptions
{
    [Required]
    [MinLength(5, ErrorMessage = "System password must be at least 5 characters long.")]
    public required string SystemPassword { get; init; }
}