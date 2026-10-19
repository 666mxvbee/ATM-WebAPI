using System.ComponentModel.DataAnnotations;

namespace Atm.Presentation.Models;

public sealed class CreateUserSessionRequest
{
    [Required]
    [RegularExpression(@"^\d{20}$", ErrorMessage = "Account number must be exactly 20 digits long.")]
    public required string AccountNumber { get; init; }

    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Pincode must be exactly 4 digits long.")]
    public required string PinCode { get; init; }
}