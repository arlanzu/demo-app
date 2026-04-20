namespace DemoApp.Domain.Entities;

/// <summary>
/// Represents a persisted contact message submitted by a user.
/// </summary>
public class ContactSubmission
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Message { get; set; } = string.Empty;

    public string SubmissionToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
