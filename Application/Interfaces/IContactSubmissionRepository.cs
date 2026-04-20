using DemoApp.Domain.Entities;

namespace DemoApp.Application.Interfaces;

/// <summary>
/// Defines persistence operations for contact submissions.
/// </summary>
public interface IContactSubmissionRepository
{
    /// <summary>
    /// Checks whether a submission token already exists.
    /// </summary>
    /// <param name="submissionToken">Token to check.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns><c>true</c> when the token exists; otherwise, <c>false</c>.</returns>
    Task<bool> SubmissionTokenExistsAsync(string submissionToken, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new contact submission.
    /// </summary>
    /// <param name="submission">Entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task AddAsync(ContactSubmission submission, CancellationToken cancellationToken);
}
