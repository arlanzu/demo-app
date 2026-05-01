using DemoApp.Domain.Entities;
using DemoApp.Infrastructure.Data;
using DemoApp.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Tests.Infrastructure.Repositories;

[TestClass]
public sealed class ContactSubmissionRepositoryTests
{
    [TestMethod]
    public async Task AddAsync_AddsSubmissionAndReturnsId()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var submission = CreateSubmission("token000000000000000000000000001");

        var id = await repository.AddAsync(submission, CancellationToken.None);

        Assert.AreNotEqual(0, id);
        Assert.AreEqual(id, submission.Id);
        Assert.AreEqual(1, await database.Context.ContactSubmissions.CountAsync());
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenSubmissionExists_ReturnsSubmission()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var id = await repository.AddAsync(CreateSubmission("token000000000000000000000000001"), CancellationToken.None);

        var result = await repository.GetByIdAsync(id, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual("Ana", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenSubmissionDoesNotExist_ReturnsNull()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);

        var result = await repository.GetByIdAsync(999, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsSubmissionsNewestFirst()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var oldest = CreateSubmission("token000000000000000000000000001", createdAt: new DateTime(2026, 4, 18, 8, 0, 0, DateTimeKind.Utc));
        var newest = CreateSubmission("token000000000000000000000000002", createdAt: new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc), name: "Ben");

        await repository.AddAsync(oldest, CancellationToken.None);
        await repository.AddAsync(newest, CancellationToken.None);

        var result = await repository.GetAllAsync(CancellationToken.None);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(newest.Id, result[0].Id);
        Assert.AreEqual(oldest.Id, result[1].Id);
    }

    [TestMethod]
    public async Task UpdateAsync_UpdatesSubmission()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var id = await repository.AddAsync(CreateSubmission("token000000000000000000000000001"), CancellationToken.None);
        var submission = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(submission);
        submission.Name = "Updated";
        submission.Message = "Updated message";

        await repository.UpdateAsync(submission, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual("Updated", result.Name);
        Assert.AreEqual("Updated message", result.Message);
    }

    [TestMethod]
    public async Task UpdateAsync_SavesReply()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var id = await repository.AddAsync(CreateSubmission("token000000000000000000000000001"), CancellationToken.None);
        var submission = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(submission);
        submission.Reply = "Thanks for the message.";

        await repository.UpdateAsync(submission, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreEqual("Thanks for the message.", result.Reply);
    }

    [TestMethod]
    public async Task DeleteAsync_RemovesSubmission()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var id = await repository.AddAsync(CreateSubmission("token000000000000000000000000001"), CancellationToken.None);
        var submission = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(submission);

        await repository.DeleteAsync(submission, CancellationToken.None);

        Assert.AreEqual(0, await database.Context.ContactSubmissions.CountAsync());
    }

    [TestMethod]
    public async Task TryAddAsync_WhenSubmissionTokenIsUnique_AddsSubmission()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);

        var result = await repository.TryAddAsync(CreateSubmission("token000000000000000000000000001"), CancellationToken.None);

        Assert.IsTrue(result);
        Assert.AreEqual(1, await database.Context.ContactSubmissions.CountAsync());
    }

    [TestMethod]
    public async Task TryAddAsync_WhenSubmissionTokenIsDuplicate_ReturnsFalseAndDoesNotAddDuplicate()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var token = "token000000000000000000000000001";
        await repository.TryAddAsync(CreateSubmission(token), CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var result = await repository.TryAddAsync(CreateSubmission(token, name: "Ben"), CancellationToken.None);

        Assert.IsFalse(result);
        Assert.AreEqual(1, await database.Context.ContactSubmissions.CountAsync());
    }

    [TestMethod]
    public async Task AddAsync_WhenRequiredNameIsNull_ThrowsDbUpdateException()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var submission = CreateSubmission("token000000000000000000000000001");
        submission.Name = null!;

        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            () => repository.AddAsync(submission, CancellationToken.None));
    }

    [TestMethod]
    public async Task AddAsync_WhenRequiredEmailIsNull_ThrowsDbUpdateException()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var submission = CreateSubmission("token000000000000000000000000001");
        submission.Email = null!;

        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            () => repository.AddAsync(submission, CancellationToken.None));
    }

    [TestMethod]
    public async Task AddAsync_WhenRequiredMessageIsNull_ThrowsDbUpdateException()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var submission = CreateSubmission("token000000000000000000000000001");
        submission.Message = null!;

        await Assert.ThrowsExceptionAsync<DbUpdateException>(
            () => repository.AddAsync(submission, CancellationToken.None));
    }

    [TestMethod]
    public void AppDbContext_ConfiguresMaxLengthConstraints()
    {
        using var database = CreateDatabase();
        var entityType = database.Context.Model.FindEntityType(typeof(ContactSubmission));
        Assert.IsNotNull(entityType);

        Assert.AreEqual(100, entityType!.FindProperty(nameof(ContactSubmission.Name))!.GetMaxLength());
        Assert.AreEqual(256, entityType.FindProperty(nameof(ContactSubmission.Email))!.GetMaxLength());
        Assert.AreEqual(30, entityType.FindProperty(nameof(ContactSubmission.Phone))!.GetMaxLength());
        Assert.AreEqual(2000, entityType.FindProperty(nameof(ContactSubmission.Message))!.GetMaxLength());
        Assert.AreEqual(2000, entityType.FindProperty(nameof(ContactSubmission.Reply))!.GetMaxLength());
        Assert.AreEqual(64, entityType.FindProperty(nameof(ContactSubmission.SubmissionToken))!.GetMaxLength());
    }

    [TestMethod]
    public void AppDbContext_ConfiguresUniqueSubmissionTokenIndex()
    {
        using var database = CreateDatabase();
        var entityType = database.Context.Model.FindEntityType(typeof(ContactSubmission));
        Assert.IsNotNull(entityType);

        var index = entityType!.GetIndexes()
            .Single(i => i.Properties.Single().Name == nameof(ContactSubmission.SubmissionToken));

        Assert.IsTrue(index.IsUnique);
    }

    [TestMethod]
    public async Task AddAsync_WhenCreatedAtNotSet_UsesDatabaseDefaultTimestamp()
    {
        using var database = CreateDatabase();
        var repository = new ContactSubmissionRepository(database.Context);
        var submission = CreateSubmission("token000000000000000000000000001", createdAt: default);

        var id = await repository.AddAsync(submission, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(id, CancellationToken.None);
        Assert.IsNotNull(result);
        Assert.AreNotEqual(default, result.CreatedAt);
    }

    private static TestDatabase CreateDatabase()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return new TestDatabase(connection, context);
    }

    private static ContactSubmission CreateSubmission(string token, DateTime? createdAt = null, string name = "Ana")
    {
        return new ContactSubmission
        {
            Name = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            Phone = "+12345",
            Message = $"Message from {name}",
            SubmissionToken = token,
            CreatedAt = createdAt ?? new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDatabase(SqliteConnection connection, AppDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public AppDbContext Context { get; }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
