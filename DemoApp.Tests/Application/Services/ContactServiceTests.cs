using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Application.Services;
using DemoApp.Domain.Entities;

namespace DemoApp.Tests.Application.Services;

[TestClass]
public sealed class ContactServiceTests
{
    [TestMethod]
    public async Task SubmitContactAsync_WhenRepositoryInserts_ReturnsSuccess()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = true };
        var service = new ContactService(repository);
        var request = new ContactSubmissionRequest("  Ana  ", "  ana@example.com  ", "  +12345  ", "  Hello there from test  ", "abcdefabcdefabcdefabcdefabcdefab");

        var result = await service.SubmitContactAsync(request, CancellationToken.None);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsDuplicate);
        Assert.IsNotNull(repository.LastSubmission);
        Assert.AreEqual("Ana", repository.LastSubmission.Name);
        Assert.AreEqual("ana@example.com", repository.LastSubmission.Email);
        Assert.AreEqual("+12345", repository.LastSubmission.Phone);
        Assert.AreEqual("Hello there from test", repository.LastSubmission.Message);
        Assert.AreEqual(request.SubmissionToken, repository.LastSubmission.SubmissionToken);
    }

    [TestMethod]
    public async Task SubmitContactAsync_WhenRepositoryReportsDuplicate_ReturnsDuplicate()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = false };
        var service = new ContactService(repository);
        var request = new ContactSubmissionRequest("Ana", "ana@example.com", null, "Hello there from test", "abcdefabcdefabcdefabcdefabcdefab");

        var result = await service.SubmitContactAsync(request, CancellationToken.None);

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsDuplicate);
    }

    [TestMethod]
    public async Task SubmitContactAsync_WhenPhoneIsNull_SavesNullPhone()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = true };
        var service = new ContactService(repository);
        var request = new ContactSubmissionRequest("Ana", "ana@example.com", null, "Hello there from test", "abcdefabcdefabcdefabcdefabcdefab");

        await service.SubmitContactAsync(request, CancellationToken.None);

        Assert.IsNotNull(repository.LastSubmission);
        Assert.IsNull(repository.LastSubmission.Phone);
    }

    [TestMethod]
    public async Task SubmitContactAsync_WhenPhoneIsWhitespace_SavesNullPhone()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = true };
        var service = new ContactService(repository);
        var request = new ContactSubmissionRequest("Ana", "ana@example.com", "   ", "Hello there from test", "abcdefabcdefabcdefabcdefabcdefab");

        await service.SubmitContactAsync(request, CancellationToken.None);

        Assert.IsNotNull(repository.LastSubmission);
        Assert.IsNull(repository.LastSubmission.Phone);
    }

    [TestMethod]
    public async Task SubmitContactAsync_PreservesSubmissionTokenFromRequest()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = true };
        var service = new ContactService(repository);
        var token = "abcdefabcdefabcdefabcdefabcdefab";

        await service.SubmitContactAsync(new ContactSubmissionRequest("Ana", "ana@example.com", null, "Hello there from test", token), CancellationToken.None);

        Assert.IsNotNull(repository.LastSubmission);
        Assert.AreEqual(token, repository.LastSubmission.SubmissionToken);
    }

    [TestMethod]
    public async Task SubmitContactAsync_WhenDuplicate_DoesNotCallAddRepositoryMethod()
    {
        var repository = new FakeContactSubmissionRepository { NextTryAddResult = false };
        var service = new ContactService(repository);

        await service.SubmitContactAsync(new ContactSubmissionRequest("Ana", "ana@example.com", null, "Hello there from test", "abcdefabcdefabcdefabcdefabcdefab"), CancellationToken.None);

        Assert.AreEqual(1, repository.TryAddCallCount);
        Assert.AreEqual(0, repository.AddCallCount);
    }

    [TestMethod]
    public async Task GetAllSubmissionsAsync_ReturnsRepositorySubmissions()
    {
        var first = CreateEntity(1, "Ana", DateTime.UtcNow);
        var second = CreateEntity(2, "Ben", DateTime.UtcNow.AddMinutes(-1), reply: "Reply");
        var repository = new FakeContactSubmissionRepository
        {
            StoredSubmissions = [first, second]
        };
        var service = new ContactService(repository);

        var result = await service.GetAllSubmissionsAsync(CancellationToken.None);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(first.Id, result[0].Id);
        Assert.AreEqual(second.Reply, result[1].Reply);
    }

    [TestMethod]
    public async Task GetSubmissionByIdAsync_WhenFound_ReturnsMappedSummary()
    {
        var entity = CreateEntity(7, "Ana", DateTime.UtcNow, reply: "Reply");
        var repository = new FakeContactSubmissionRepository
        {
            StoredSubmissions = [entity]
        };
        var service = new ContactService(repository);

        var result = await service.GetSubmissionByIdAsync(7, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result.Id);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.Email, result.Email);
        Assert.AreEqual(entity.Phone, result.Phone);
        Assert.AreEqual(entity.Message, result.Message);
        Assert.AreEqual(entity.Reply, result.Reply);
        Assert.AreEqual(entity.CreatedAt, result.CreatedAtUtc);
    }

    [TestMethod]
    public async Task GetSubmissionByIdAsync_WhenMissing_ReturnsNull()
    {
        var service = new ContactService(new FakeContactSubmissionRepository());

        var result = await service.GetSubmissionByIdAsync(999, CancellationToken.None);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_TrimsInputAndGeneratesSubmissionToken()
    {
        var repository = new FakeContactSubmissionRepository();
        var service = new ContactService(repository);
        var request = new AdminSubmissionRequest("  Ana  ", "  ana@example.com  ", "  +12345  ", "  Admin message  ");

        var id = await service.CreateSubmissionAsync(request, CancellationToken.None);

        Assert.AreEqual(1, id);
        Assert.AreEqual(1, repository.AddCallCount);
        Assert.IsNotNull(repository.LastSubmission);
        Assert.AreEqual("Ana", repository.LastSubmission.Name);
        Assert.AreEqual("ana@example.com", repository.LastSubmission.Email);
        Assert.AreEqual("+12345", repository.LastSubmission.Phone);
        Assert.AreEqual("Admin message", repository.LastSubmission.Message);
        Assert.IsFalse(string.IsNullOrWhiteSpace(repository.LastSubmission.SubmissionToken));
        Assert.AreEqual(32, repository.LastSubmission.SubmissionToken.Length);
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_WhenPhoneWhitespace_SavesNullPhone()
    {
        var repository = new FakeContactSubmissionRepository();
        var service = new ContactService(repository);
        var request = new AdminSubmissionRequest("Ana", "ana@example.com", "   ", "Admin message");

        await service.CreateSubmissionAsync(request, CancellationToken.None);

        Assert.IsNotNull(repository.LastSubmission);
        Assert.IsNull(repository.LastSubmission.Phone);
    }

    [TestMethod]
    public async Task UpdateSubmissionAsync_WhenFound_UpdatesEditableFieldsAndPreservesCreatedAtAndToken()
    {
        var createdAt = new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc);
        var entity = CreateEntity(4, "Ana", createdAt);
        var token = entity.SubmissionToken;
        var repository = new FakeContactSubmissionRepository
        {
            StoredSubmissions = [entity]
        };
        var service = new ContactService(repository);
        var request = new AdminSubmissionRequest("  Ben  ", "  ben@example.com  ", "   ", "  Updated message  ");

        var updated = await service.UpdateSubmissionAsync(4, request, CancellationToken.None);

        Assert.IsTrue(updated);
        Assert.AreEqual(1, repository.UpdateCallCount);
        Assert.AreEqual("Ben", entity.Name);
        Assert.AreEqual("ben@example.com", entity.Email);
        Assert.IsNull(entity.Phone);
        Assert.AreEqual("Updated message", entity.Message);
        Assert.AreEqual(token, entity.SubmissionToken);
        Assert.AreEqual(createdAt, entity.CreatedAt);
    }

    [TestMethod]
    public async Task UpdateSubmissionAsync_WhenMissing_ReturnsFalse()
    {
        var service = new ContactService(new FakeContactSubmissionRepository());

        var updated = await service.UpdateSubmissionAsync(4, new AdminSubmissionRequest("Ana", "ana@example.com", null, "Message"), CancellationToken.None);

        Assert.IsFalse(updated);
    }

    [TestMethod]
    public async Task DeleteSubmissionAsync_WhenFound_DeletesSubmission()
    {
        var entity = CreateEntity(4, "Ana", DateTime.UtcNow);
        var repository = new FakeContactSubmissionRepository
        {
            StoredSubmissions = [entity]
        };
        var service = new ContactService(repository);

        var deleted = await service.DeleteSubmissionAsync(4, CancellationToken.None);

        Assert.IsTrue(deleted);
        Assert.AreEqual(1, repository.DeleteCallCount);
        Assert.AreEqual(0, repository.StoredSubmissions.Count);
    }

    [TestMethod]
    public async Task DeleteSubmissionAsync_WhenMissing_ReturnsFalse()
    {
        var repository = new FakeContactSubmissionRepository();
        var service = new ContactService(repository);

        var deleted = await service.DeleteSubmissionAsync(4, CancellationToken.None);

        Assert.IsFalse(deleted);
        Assert.AreEqual(0, repository.DeleteCallCount);
    }

    [TestMethod]
    public async Task SetReplyAsync_WhenFound_TrimsReplyAndDoesNotOverwriteOtherFields()
    {
        var createdAt = new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc);
        var entity = CreateEntity(4, "Ana", createdAt);
        var originalToken = entity.SubmissionToken;
        var repository = new FakeContactSubmissionRepository
        {
            StoredSubmissions = [entity]
        };
        var service = new ContactService(repository);

        var updated = await service.SetReplyAsync(4, "  Thank you.  ", CancellationToken.None);

        Assert.IsTrue(updated);
        Assert.AreEqual(1, repository.UpdateCallCount);
        Assert.AreEqual("Thank you.", entity.Reply);
        Assert.AreEqual("Ana", entity.Name);
        Assert.AreEqual("ana@example.com", entity.Email);
        Assert.AreEqual("Message from Ana", entity.Message);
        Assert.AreEqual(originalToken, entity.SubmissionToken);
        Assert.AreEqual(createdAt, entity.CreatedAt);
    }

    [TestMethod]
    public async Task SetReplyAsync_WhenMissing_ReturnsFalse()
    {
        var service = new ContactService(new FakeContactSubmissionRepository());

        var updated = await service.SetReplyAsync(4, "Reply", CancellationToken.None);

        Assert.IsFalse(updated);
    }

    private static ContactSubmission CreateEntity(int id, string name, DateTime createdAt, string? reply = null)
    {
        return new ContactSubmission
        {
            Id = id,
            Name = name,
            Email = $"{name.ToLowerInvariant()}@example.com",
            Phone = "+12345",
            Message = $"Message from {name}",
            Reply = reply,
            SubmissionToken = $"token{id:000000000000000000000000000}",
            CreatedAt = createdAt
        };
    }

    private sealed class FakeContactSubmissionRepository : IContactSubmissionRepository
    {
        public bool NextTryAddResult { get; set; } = true;

        public List<ContactSubmission> StoredSubmissions { get; set; } = new();

        public ContactSubmission? LastSubmission { get; private set; }

        public int TryAddCallCount { get; private set; }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public Task<bool> TryAddAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            TryAddCallCount++;
            LastSubmission = submission;

            if (NextTryAddResult)
            {
                submission.Id = StoredSubmissions.Count + 1;
                StoredSubmissions.Add(submission);
            }

            return Task.FromResult(NextTryAddResult);
        }

        public Task<IReadOnlyList<ContactSubmission>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ContactSubmission> result = StoredSubmissions;
            return Task.FromResult(result);
        }

        public Task<ContactSubmission?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredSubmissions.FirstOrDefault(s => s.Id == id));
        }

        public Task<int> AddAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            AddCallCount++;
            LastSubmission = submission;
            submission.Id = StoredSubmissions.Count + 1;
            StoredSubmissions.Add(submission);
            return Task.FromResult(submission.Id);
        }

        public Task UpdateAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            UpdateCallCount++;
            LastSubmission = submission;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            StoredSubmissions.Remove(submission);
            return Task.CompletedTask;
        }
    }
}
