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
    /// Gets all submissions for administrative listing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Read-only list of submission summaries.</returns>
    Task<IReadOnlyList<ContactSubmissionSummary>> GetAllSubmissionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets one submission by identifier.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Submission summary when found; otherwise null.</returns>
    Task<ContactSubmissionSummary?> GetSubmissionByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new submission from admin input.
    /// </summary>
    /// <param name="request">Validated admin submission payload.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Created submission identifier.</returns>
    Task<int> CreateSubmissionAsync(AdminSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing submission by identifier.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="request">Validated admin update payload.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True when updated; false when not found.</returns>
    Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an existing submission by identifier.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True when deleted; false when not found.</returns>
    Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Sets an admin reply for a submission.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="reply">Reply text to store.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>True when updated; false when not found.</returns>
    Task<bool> SetReplyAsync(int id, string reply, CancellationToken cancellationToken);
}
