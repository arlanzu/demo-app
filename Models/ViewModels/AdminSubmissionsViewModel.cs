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
