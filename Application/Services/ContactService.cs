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
        var isDuplicate = await _contactSubmissionRepository.SubmissionTokenExistsAsync(request.SubmissionToken, cancellationToken);
        if (isDuplicate)
        {
            return ContactSubmissionResult.Duplicate();
        }

        var entity = new ContactSubmission
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Message = request.Message.Trim(),
            SubmissionToken = request.SubmissionToken,
            CreatedAt = DateTime.UtcNow
        };

        await _contactSubmissionRepository.AddAsync(entity, cancellationToken);
        return ContactSubmissionResult.Success();
    }
}
