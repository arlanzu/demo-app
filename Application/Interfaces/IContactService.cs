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

    /// <summary>
    /// Gets all submitted contact requests for administrative viewing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Read-only list of submission summaries.</returns>
    Task<IReadOnlyList<ContactSubmissionSummary>> GetAllSubmissionsAsync(CancellationToken cancellationToken);
}
