using System.ComponentModel.DataAnnotations;

namespace DemoApp.Models.ViewModels;

/// <summary>
/// Represents the admin page model containing all contact submissions.
/// </summary>
public class AdminSubmissionsViewModel
{
    public List<AdminSubmissionItemViewModel> Submissions { get; set; } = new();
}

/// <summary>
/// Represents one row in the admin submissions list.
/// </summary>
public class AdminSubmissionItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Represents create/edit form data for admin submission operations.
/// </summary>
public class AdminSubmissionFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone]
    public string? Phone { get; set; }

    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents details page data for one submission.
/// </summary>
public class AdminSubmissionDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
