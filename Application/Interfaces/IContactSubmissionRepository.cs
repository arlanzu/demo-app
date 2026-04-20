using DemoApp.Domain.Entities;

namespace DemoApp.Application.Interfaces;

/// <summary>
/// Defines persistence operations for contact submissions.
/// </summary>
public interface IContactSubmissionRepository
{
    /// <summary>
    /// Persists a new contact submission if the token is unique.
    /// </summary>
    /// <param name="submission">Entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns><c>true</c> when inserted; <c>false</c> when a duplicate token is detected.</returns>
    Task<bool> TryAddAsync(ContactSubmission submission, CancellationToken cancellationToken);
}
