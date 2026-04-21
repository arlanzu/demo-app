using System.Security.Cryptography;
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

    /// <inheritdoc />
    public async Task<ContactSubmissionSummary?> GetSubmissionByIdAsync(int id, CancellationToken cancellationToken)
    {
        var submission = await _contactSubmissionRepository.GetByIdAsync(id, cancellationToken);

        if (submission is null)
        {
            return null;
        }

        return new ContactSubmissionSummary(
            submission.Id,
            submission.Name,
            submission.Email,
            submission.Phone,
            submission.Message,
            submission.CreatedAt);
    }

    /// <inheritdoc />
    public async Task<int> CreateSubmissionAsync(AdminSubmissionRequest request, CancellationToken cancellationToken)
    {
        var entity = new ContactSubmission
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Message = request.Message.Trim(),
            SubmissionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        return await _contactSubmissionRepository.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _contactSubmissionRepository.GetByIdAsync(id, cancellationToken);

        if (submission is null)
        {
            return false;
        }

        submission.Name = request.Name.Trim();
        submission.Email = request.Email.Trim();
        submission.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        submission.Message = request.Message.Trim();

        await _contactSubmissionRepository.UpdateAsync(submission, cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken)
    {
        var submission = await _contactSubmissionRepository.GetByIdAsync(id, cancellationToken);

        if (submission is null)
        {
            return false;
        }

        await _contactSubmissionRepository.DeleteAsync(submission, cancellationToken);
        return true;
    }
}
