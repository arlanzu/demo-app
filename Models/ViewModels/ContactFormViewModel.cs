using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models.ViewModels;

/// <summary>
/// Represents contact form fields used by the presentation layer.
/// </summary>
public class ContactFormViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters.")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Submission token is required.")]
    [StringLength(64, MinimumLength = 32, ErrorMessage = "Submission token length is invalid.")]
    [RegularExpression("^[a-f0-9]{32}$", ErrorMessage = "Submission token format is invalid.")]
    public string SubmissionToken { get; set; } = string.Empty;
}
