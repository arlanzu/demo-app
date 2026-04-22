namespace DemoApp.Application.Contracts;

/// <summary>
/// Represents a contact request summary returned for administrative screens.
/// </summary>
/// <param name="Id">Database identifier of the submission.</param>
/// <param name="Name">Submitted sender name.</param>
/// <param name="Email">Submitted sender email.</param>
/// <param name="Phone">Submitted sender phone.</param>
/// <param name="Message">Submitted message text.</param>
/// <param name="Reply">Admin reply text.</param>
/// <param name="CreatedAtUtc">Submission timestamp in UTC.</param>
public sealed record ContactSubmissionSummary(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string Message,
    string? Reply,
    DateTime CreatedAtUtc);
