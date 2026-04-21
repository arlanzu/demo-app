using DemoApp.Application.Interfaces;
using DemoApp.Domain.Entities;
using DemoApp.Infrastructure.Data;
using Microsoft.Data.Sqlite;
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
    public async Task<bool> TryAddAsync(ContactSubmission submission, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.ContactSubmissions.AddAsync(submission, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContactSubmission>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ContactSubmissions
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
