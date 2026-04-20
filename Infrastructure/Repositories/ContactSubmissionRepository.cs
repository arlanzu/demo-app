using DemoApp.Application.Interfaces;
using DemoApp.Domain.Entities;
using DemoApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation for contact submission persistence.
/// </summary>
public class ContactSubmissionRepository : IContactSubmissionRepository
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Creates a repository instance.
    /// </summary>
    /// <param name="dbContext">Database context used for persistence operations.</param>
    public ContactSubmissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<bool> SubmissionTokenExistsAsync(string submissionToken, CancellationToken cancellationToken)
    {
        return _dbContext.ContactSubmissions
            .AsNoTracking()
            .AnyAsync(x => x.SubmissionToken == submissionToken, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ContactSubmission submission, CancellationToken cancellationToken)
    {
        await _dbContext.ContactSubmissions.AddAsync(submission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
