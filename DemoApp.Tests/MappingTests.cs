using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Application.Services;
using DemoApp.Controllers;
using DemoApp.Domain.Entities;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Tests;

[TestClass]
public sealed class MappingTests
{
    [TestMethod]
    public async Task AdminIndex_MapsSubmissionSummaryToListItemViewModel()
    {
        var createdAt = new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc);
        var service = new FakeContactService
        {
            Submissions =
            [
                new ContactSubmissionSummary(7, "Ana", "ana@example.com", "+12345", "Message", "Reply", createdAt)
            ]
        };
        var controller = new AdminController(service);

        var result = await controller.Index(CancellationToken.None);

        var model = (AdminSubmissionsViewModel)((ViewResult)result).Model!;
        var item = model.Submissions.Single();
        Assert.AreEqual(7, item.Id);
        Assert.AreEqual("Ana", item.Name);
        Assert.AreEqual("ana@example.com", item.Email);
        Assert.AreEqual("+12345", item.Phone);
        Assert.AreEqual("Message", item.Message);
        Assert.AreEqual("Reply", item.Reply);
        Assert.AreEqual(createdAt, item.CreatedAtUtc);
    }

    [TestMethod]
    public async Task AdminEditGet_MapsSubmissionIdAndEditableFields()
    {
        var createdAt = new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc);
        var service = new FakeContactService
        {
            SubmissionById = new ContactSubmissionSummary(7, "Ana", "ana@example.com", "+12345", "Message", "Reply", createdAt)
        };
        var controller = new AdminController(service);

        var result = await controller.Edit(7, CancellationToken.None);

        var model = (AdminSubmissionFormViewModel)((ViewResult)result).Model!;
        Assert.AreEqual(7, model.Id);
        Assert.AreEqual("Ana", model.Name);
        Assert.AreEqual("ana@example.com", model.Email);
        Assert.AreEqual("+12345", model.Phone);
        Assert.AreEqual("Message", model.Message);
    }

    [TestMethod]
    public async Task AdminReplyGet_MapsExistingReply()
    {
        var service = new FakeContactService
        {
            SubmissionById = new ContactSubmissionSummary(7, "Ana", "ana@example.com", "+12345", "Message", "Reply", DateTime.UtcNow)
        };
        var controller = new AdminController(service);

        var result = await controller.Reply(7, CancellationToken.None);

        var model = (AdminReplyViewModel)((ViewResult)result).Model!;
        Assert.AreEqual(7, model.Id);
        Assert.AreEqual("Ana", model.Name);
        Assert.AreEqual("ana@example.com", model.Email);
        Assert.AreEqual("Message", model.Message);
        Assert.AreEqual("Reply", model.Reply);
    }

    [TestMethod]
    public async Task ContactService_GetSubmissionById_MapsEntityToSummaryWithoutSubmissionToken()
    {
        var createdAt = new DateTime(2026, 4, 20, 8, 0, 0, DateTimeKind.Utc);
        var entity = new ContactSubmission
        {
            Id = 7,
            Name = "Ana",
            Email = "ana@example.com",
            Phone = "+12345",
            Message = "Message",
            Reply = "Reply",
            SubmissionToken = "abcdefabcdefabcdefabcdefabcdefab",
            CreatedAt = createdAt
        };
        var repository = new FakeRepository { Submission = entity };
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

    private sealed class FakeContactService : IContactService
    {
        public IReadOnlyList<ContactSubmissionSummary> Submissions { get; set; } = Array.Empty<ContactSubmissionSummary>();

        public ContactSubmissionSummary? SubmissionById { get; set; }

        public Task<ContactSubmissionResult> SubmitContactAsync(ContactSubmissionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(ContactSubmissionResult.Success());
        }

        public Task<IReadOnlyList<ContactSubmissionSummary>> GetAllSubmissionsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Submissions);
        }

        public Task<ContactSubmissionSummary?> GetSubmissionByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(SubmissionById?.Id == id ? SubmissionById : null);
        }

        public Task<int> CreateSubmissionAsync(AdminSubmissionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }

        public Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SetReplyAsync(int id, string reply, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class FakeRepository : IContactSubmissionRepository
    {
        public ContactSubmission? Submission { get; set; }

        public Task<bool> TryAddAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<ContactSubmission>> GetAllAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ContactSubmission> submissions = Submission is null
                ? Array.Empty<ContactSubmission>()
                : new[] { Submission };
            return Task.FromResult(submissions);
        }

        public Task<ContactSubmission?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Submission?.Id == id ? Submission : null);
        }

        public Task<int> AddAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }

        public Task UpdateAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ContactSubmission submission, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
