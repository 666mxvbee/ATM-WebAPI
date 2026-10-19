using System.ComponentModel.DataAnnotations;

namespace Atm.Presentation.Models;

public sealed class CreateAdminSessionRequest
{
    [Required]
    [MinLength(5, ErrorMessage = "System password must be at least 5 characters long.")]
    public required string SystemPassword { get; init; }
}