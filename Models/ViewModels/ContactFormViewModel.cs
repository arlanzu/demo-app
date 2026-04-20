using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models.ViewModels;

/// <summary>
/// Represents contact form fields used by the presentation layer.
/// </summary>
public class ContactFormViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string SubmissionToken { get; set; } = string.Empty;
}
