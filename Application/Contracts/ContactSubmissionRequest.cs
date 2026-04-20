namespace DemoApp.Application.Contracts;

/// <summary>
/// Carries validated contact submission data from presentation to application layer.
/// </summary>
/// <param name="Name">Name of the person submitting the message.</param>
/// <param name="Email">Email of the person submitting the message.</param>
/// <param name="Phone">Optional phone number of the sender.</param>
/// <param name="Message">Free-text message from the sender.</param>
/// <param name="SubmissionToken">Client-generated token used for duplicate submission checks.</param>
public sealed record ContactSubmissionRequest(
    string Name,
    string Email,
    string? Phone,
    string Message,
    string SubmissionToken);
