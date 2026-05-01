using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Controllers;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Tests.Controllers;

[TestClass]
public sealed class AdminControllerTests
{
    [TestMethod]
    public async Task Index_ReturnsAllSubmissions()
    {
        var service = new FakeContactService
        {
            Submissions =
            [
                CreateSummary(1, "Ana", DateTime.UtcNow),
                CreateSummary(2, "Ben", DateTime.UtcNow.AddMinutes(-1))
            ]
        };
        var controller = new AdminController(service);

        var result = await controller.Index(CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var model = AssertViewModel<AdminSubmissionsViewModel>((ViewResult)result);
        Assert.AreEqual(2, model.Submissions.Count);
        Assert.AreEqual("Ana", model.Submissions[0].Name);
        Assert.AreEqual("Ben", model.Submissions[1].Name);
    }

    [TestMethod]
    public async Task Index_PreservesServiceOrderingForNewestFirstList()
    {
        var newest = new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc);
        var oldest = newest.AddDays(-2);
        var service = new FakeContactService
        {
            Submissions =
            [
                CreateSummary(2, "Newest", newest),
                CreateSummary(1, "Oldest", oldest)
            ]
        };
        var controller = new AdminController(service);

        var result = await controller.Index(CancellationToken.None);

        var model = AssertViewModel<AdminSubmissionsViewModel>((ViewResult)result);
        Assert.AreEqual(2, model.Submissions[0].Id);
        Assert.AreEqual(newest, model.Submissions[0].CreatedAtUtc);
        Assert.AreEqual(1, model.Submissions[1].Id);
    }

    [TestMethod]
    public async Task Details_ExistingId_ReturnsCorrectSubmission()
    {
        var service = new FakeContactService
        {
            SubmissionById = CreateSummary(7, "Ana", DateTime.UtcNow, reply: "Thanks")
        };
        var controller = new AdminController(service);

        var result = await controller.Details(7, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var model = AssertViewModel<AdminSubmissionDetailsViewModel>((ViewResult)result);
        Assert.AreEqual(7, model.Id);
        Assert.AreEqual("Ana", model.Name);
        Assert.AreEqual("Thanks", model.Reply);
    }

    [TestMethod]
    public async Task Details_MissingId_ReturnsNotFound()
    {
        var controller = new AdminController(new FakeContactService());

        var result = await controller.Details(999, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public void Create_Get_ReturnsCreateView()
    {
        var controller = new AdminController(new FakeContactService());

        var result = controller.Create();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsNull(viewResult.ViewName);
        Assert.IsInstanceOfType(viewResult.Model, typeof(AdminSubmissionFormViewModel));
    }

    [TestMethod]
    public async Task Create_ValidModel_CreatesSubmissionAndRedirectsToIndex()
    {
        var service = new FakeContactService();
        var controller = new AdminController(service);
        var viewModel = CreateFormViewModel();

        var result = await controller.Create(viewModel, CancellationToken.None);

        Assert.AreEqual(1, service.CreateCallCount);
        Assert.IsNotNull(service.LastAdminRequest);
        Assert.AreEqual(viewModel.Name, service.LastAdminRequest.Name);
        AssertRedirectsToIndex(result);
    }

    [TestMethod]
    public async Task Create_InvalidModelState_ReturnsCreateView()
    {
        var service = new FakeContactService();
        var controller = new AdminController(service);
        controller.ModelState.AddModelError(nameof(AdminSubmissionFormViewModel.Email), "Email is required.");
        var viewModel = CreateFormViewModel();

        var result = await controller.Create(viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.AreEqual(0, service.CreateCallCount);
        Assert.AreSame(viewModel, ((ViewResult)result).Model);
    }

    [TestMethod]
    public async Task Edit_Get_ExistingId_ReturnsCorrectSubmission()
    {
        var service = new FakeContactService
        {
            SubmissionById = CreateSummary(3, "Ana", DateTime.UtcNow)
        };
        var controller = new AdminController(service);

        var result = await controller.Edit(3, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var model = AssertViewModel<AdminSubmissionFormViewModel>((ViewResult)result);
        Assert.AreEqual(3, model.Id);
        Assert.AreEqual("Ana", model.Name);
    }

    [TestMethod]
    public async Task Edit_Get_MissingId_ReturnsNotFound()
    {
        var controller = new AdminController(new FakeContactService());

        var result = await controller.Edit(3, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Edit_Post_ValidModel_UpdatesSubmissionAndRedirectsToIndex()
    {
        var service = new FakeContactService { UpdateResult = true };
        var controller = new AdminController(service);
        var viewModel = CreateFormViewModel(id: 4);

        var result = await controller.Edit(4, viewModel, CancellationToken.None);

        Assert.AreEqual(1, service.UpdateCallCount);
        Assert.AreEqual(4, service.LastUpdatedId);
        Assert.IsNotNull(service.LastAdminRequest);
        Assert.AreEqual(viewModel.Message, service.LastAdminRequest.Message);
        AssertRedirectsToIndex(result);
    }

    [TestMethod]
    public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
    {
        var controller = new AdminController(new FakeContactService());
        var viewModel = CreateFormViewModel(id: 4);

        var result = await controller.Edit(5, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task Edit_Post_InvalidModelState_ReturnsEditView()
    {
        var service = new FakeContactService();
        var controller = new AdminController(service);
        controller.ModelState.AddModelError(nameof(AdminSubmissionFormViewModel.Name), "Name is required.");
        var viewModel = CreateFormViewModel(id: 4);

        var result = await controller.Edit(4, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.AreEqual(0, service.UpdateCallCount);
        Assert.AreSame(viewModel, ((ViewResult)result).Model);
    }

    [TestMethod]
    public async Task Edit_Post_MissingSubmission_ReturnsNotFound()
    {
        var service = new FakeContactService { UpdateResult = false };
        var controller = new AdminController(service);
        var viewModel = CreateFormViewModel(id: 4);

        var result = await controller.Edit(4, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Delete_Post_DeletesSubmissionAndRedirectsToIndex()
    {
        var service = new FakeContactService { DeleteResult = true };
        var controller = new AdminController(service);

        var result = await controller.Delete(8, CancellationToken.None);

        Assert.AreEqual(1, service.DeleteCallCount);
        Assert.AreEqual(8, service.LastDeletedId);
        AssertRedirectsToIndex(result);
    }

    [TestMethod]
    public async Task Delete_Post_MissingSubmission_ReturnsNotFound()
    {
        var service = new FakeContactService { DeleteResult = false };
        var controller = new AdminController(service);

        var result = await controller.Delete(8, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Reply_Get_ExistingId_ReturnsReplyView()
    {
        var service = new FakeContactService
        {
            SubmissionById = CreateSummary(9, "Ana", DateTime.UtcNow, reply: "Existing reply")
        };
        var controller = new AdminController(service);

        var result = await controller.Reply(9, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var model = AssertViewModel<AdminReplyViewModel>((ViewResult)result);
        Assert.AreEqual(9, model.Id);
        Assert.AreEqual("Ana", model.Name);
        Assert.AreEqual("Existing reply", model.Reply);
    }

    [TestMethod]
    public async Task Reply_Get_MissingId_ReturnsNotFound()
    {
        var controller = new AdminController(new FakeContactService());

        var result = await controller.Reply(9, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Reply_Post_ValidModel_SavesReplyAndRedirectsToIndex()
    {
        var service = new FakeContactService { ReplyResult = true };
        var controller = new AdminController(service);
        var viewModel = CreateReplyViewModel(id: 9, reply: "Thanks for writing.");

        var result = await controller.Reply(9, viewModel, CancellationToken.None);

        Assert.AreEqual(1, service.ReplyCallCount);
        Assert.AreEqual(9, service.LastReplyId);
        Assert.AreEqual("Thanks for writing.", service.LastReply);
        AssertRedirectsToIndex(result);
    }

    [TestMethod]
    public async Task Reply_Post_IdMismatch_ReturnsBadRequest()
    {
        var controller = new AdminController(new FakeContactService());
        var viewModel = CreateReplyViewModel(id: 9, reply: "Thanks");

        var result = await controller.Reply(10, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task Reply_Post_InvalidModelState_ReturnsReplyView()
    {
        var service = new FakeContactService();
        var controller = new AdminController(service);
        controller.ModelState.AddModelError(nameof(AdminReplyViewModel.Reply), "Reply is required.");
        var viewModel = CreateReplyViewModel(id: 9, reply: string.Empty);

        var result = await controller.Reply(9, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.AreEqual(0, service.ReplyCallCount);
        Assert.AreSame(viewModel, ((ViewResult)result).Model);
    }

    [TestMethod]
    public async Task Reply_Post_MissingSubmission_ReturnsNotFound()
    {
        var service = new FakeContactService { ReplyResult = false };
        var controller = new AdminController(service);
        var viewModel = CreateReplyViewModel(id: 9, reply: "Thanks");

        var result = await controller.Reply(9, viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public void MutatingPostActions_HaveValidateAntiForgeryTokenAttribute()
    {
        var actionSignatures = new[]
        {
            new { Name = nameof(AdminController.Create), Parameters = new[] { typeof(AdminSubmissionFormViewModel), typeof(CancellationToken) } },
            new { Name = nameof(AdminController.Edit), Parameters = new[] { typeof(int), typeof(AdminSubmissionFormViewModel), typeof(CancellationToken) } },
            new { Name = nameof(AdminController.Delete), Parameters = new[] { typeof(int), typeof(CancellationToken) } },
            new { Name = nameof(AdminController.Reply), Parameters = new[] { typeof(int), typeof(AdminReplyViewModel), typeof(CancellationToken) } }
        };

        foreach (var signature in actionSignatures)
        {
            var method = typeof(AdminController).GetMethod(signature.Name, signature.Parameters);

            Assert.IsNotNull(method, signature.Name);
            Assert.IsTrue(method!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Any(), signature.Name);
            Assert.IsTrue(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: false).Any(), signature.Name);
        }
    }

    private static AdminSubmissionFormViewModel CreateFormViewModel(int id = 0)
    {
        return new AdminSubmissionFormViewModel
        {
            Id = id,
            Name = "Ana",
            Email = "ana@example.com",
            Phone = "+12345",
            Message = "This is a valid admin-created message."
        };
    }

    private static AdminReplyViewModel CreateReplyViewModel(int id, string reply)
    {
        return new AdminReplyViewModel
        {
            Id = id,
            Name = "Ana",
            Email = "ana@example.com",
            Message = "Original message",
            Reply = reply
        };
    }

    private static ContactSubmissionSummary CreateSummary(int id, string name, DateTime createdAtUtc, string? reply = null)
    {
        return new ContactSubmissionSummary(
            id,
            name,
            $"{name.ToLowerInvariant()}@example.com",
            "+12345",
            $"Message from {name}",
            reply,
            createdAtUtc);
    }

    private static T AssertViewModel<T>(ViewResult result)
    {
        Assert.IsNull(result.ViewName);
        Assert.IsInstanceOfType(result.Model, typeof(T));
        return (T)result.Model!;
    }

    private static void AssertRedirectsToIndex(IActionResult result)
    {
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(AdminController.Index), redirectResult.ActionName);
        Assert.IsNull(redirectResult.ControllerName);
    }

    private sealed class FakeContactService : IContactService
    {
        public IReadOnlyList<ContactSubmissionSummary> Submissions { get; set; } = Array.Empty<ContactSubmissionSummary>();

        public ContactSubmissionSummary? SubmissionById { get; set; }

        public int CreateCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public int ReplyCallCount { get; private set; }

        public AdminSubmissionRequest? LastAdminRequest { get; private set; }

        public int LastUpdatedId { get; private set; }

        public int LastDeletedId { get; private set; }

        public int LastReplyId { get; private set; }

        public string? LastReply { get; private set; }

        public bool UpdateResult { get; set; } = true;

        public bool DeleteResult { get; set; } = true;

        public bool ReplyResult { get; set; } = true;

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
            CreateCallCount++;
            LastAdminRequest = request;
            return Task.FromResult(42);
        }

        public Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken)
        {
            UpdateCallCount++;
            LastUpdatedId = id;
            LastAdminRequest = request;
            return Task.FromResult(UpdateResult);
        }

        public Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            LastDeletedId = id;
            return Task.FromResult(DeleteResult);
        }

        public Task<bool> SetReplyAsync(int id, string reply, CancellationToken cancellationToken)
        {
            ReplyCallCount++;
            LastReplyId = id;
            LastReply = reply;
            return Task.FromResult(ReplyResult);
        }
    }
}
