namespace DemoApp.Application.Contracts;

/// <summary>
/// Carries admin-provided submission data for create and update operations.
/// </summary>
/// <param name="Name">Name of the sender.</param>
/// <param name="Email">Email of the sender.</param>
/// <param name="Phone">Optional phone number of the sender.</param>
/// <param name="Message">Message body.</param>
public sealed record AdminSubmissionRequest(
    string Name,
    string Email,
    string? Phone,
    string Message);
