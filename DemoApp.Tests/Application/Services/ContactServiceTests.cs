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

    private sealed class FakeContactSubmissionRepository : IContactSubmissionRepository
    {
        public bool NextTryAddResult { get; set; }

        public ContactSubmission? LastSubmission { get; private set; }

        public Task<bool> TryAddAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            LastSubmission = submission;
            return Task.FromResult(NextTryAddResult);
        }

        public Task<IReadOnlyList<ContactSubmission>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ContactSubmission> result = Array.Empty<ContactSubmission>();
            return Task.FromResult(result);
        }
    }
}
