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

    /// <summary>
    /// Gets one submission by id for admin workflows.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The matching summary or <c>null</c> when not found.</returns>
    Task<ContactSubmissionSummary?> GetSubmissionByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new submission from admin input.
    /// </summary>
    /// <param name="request">Validated admin input.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Created submission id.</returns>
    Task<int> CreateSubmissionAsync(AdminSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing submission.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="request">Validated admin input.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns><c>true</c> when updated; otherwise <c>false</c>.</returns>
    Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a submission by id.
    /// </summary>
    /// <param name="id">Submission identifier.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns><c>true</c> when deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken);
}
