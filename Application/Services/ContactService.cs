using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Domain.Entities;

namespace DemoApp.Application.Services;

/// <summary>
/// Executes contact submission business rules.
/// </summary>
public class ContactService : IContactService
{
    private readonly IContactSubmissionRepository _contactSubmissionRepository;

    /// <summary>
    /// Creates a service instance.
    /// </summary>
    /// <param name="contactSubmissionRepository">Repository used for persistence operations.</param>
    public ContactService(IContactSubmissionRepository contactSubmissionRepository)
    {
        _contactSubmissionRepository = contactSubmissionRepository;
    }

    /// <inheritdoc />
    public async Task<ContactSubmissionResult> SubmitContactAsync(ContactSubmissionRequest request, CancellationToken cancellationToken)
    {
        var entity = new ContactSubmission
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Message = request.Message.Trim(),
            SubmissionToken = request.SubmissionToken,
            CreatedAt = DateTime.UtcNow
        };

        var isInserted = await _contactSubmissionRepository.TryAddAsync(entity, cancellationToken);
        return isInserted
            ? ContactSubmissionResult.Success()
            : ContactSubmissionResult.Duplicate();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContactSubmissionSummary>> GetAllSubmissionsAsync(CancellationToken cancellationToken)
    {
        var submissions = await _contactSubmissionRepository.GetAllAsync(cancellationToken);

        return submissions
            .Select(s => new ContactSubmissionSummary(
                s.Id,
                s.Name,
                s.Email,
                s.Phone,
                s.Message,
                s.CreatedAt))
            .ToList();
    }
}
