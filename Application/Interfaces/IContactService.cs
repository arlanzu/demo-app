using DemoApp.Application.Contracts;

namespace DemoApp.Application.Interfaces;

/// <summary>
/// Defines application operations for contact submission use cases.
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Submits a contact message if it has not already been submitted using the same token.
    /// </summary>
    /// <param name="request">Validated request payload.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A submission result describing success or duplicate detection.</returns>
    Task<ContactSubmissionResult> SubmitContactAsync(ContactSubmissionRequest request, CancellationToken cancellationToken);
}
