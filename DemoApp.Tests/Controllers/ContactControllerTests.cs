using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Controllers;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DemoApp.Tests.Controllers;

[TestClass]
public sealed class ContactControllerTests
{
    [TestMethod]
    public void Contact_GetRequest_ReturnsContactView()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);

        var result = controller.Contact();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsNull(viewResult.ViewName);
        Assert.IsInstanceOfType(viewResult.Model, typeof(ContactFormViewModel));
    }

    [TestMethod]
    public void Contact_GetRequest_ReturnsViewWithSubmissionToken()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);

        var result = controller.Contact();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType(viewResult.Model, typeof(ContactFormViewModel));
        var model = (ContactFormViewModel)viewResult.Model!;
        Assert.IsFalse(string.IsNullOrWhiteSpace(model.SubmissionToken));
        Assert.AreEqual(32, model.SubmissionToken.Length);
    }

    [TestMethod]
    public async Task Contact_ValidModel_CallsServiceWithSubmissionData()
    {
        var service = new FakeContactService
        {
            NextResult = ContactSubmissionResult.Success()
        };
        var controller = CreateController(service);
        var viewModel = CreateValidViewModel();

        await controller.Contact(viewModel, CancellationToken.None);

        Assert.AreEqual(1, service.SubmitCallCount);
        Assert.IsNotNull(service.LastRequest);
        Assert.AreEqual(viewModel.Name, service.LastRequest.Name);
        Assert.AreEqual(viewModel.Email, service.LastRequest.Email);
        Assert.AreEqual(viewModel.Phone, service.LastRequest.Phone);
        Assert.AreEqual(viewModel.Message, service.LastRequest.Message);
        Assert.AreEqual(viewModel.SubmissionToken, service.LastRequest.SubmissionToken);
    }

    [TestMethod]
    public async Task Contact_ServiceReturnsSuccess_RedirectsToThankYou()
    {
        var service = new FakeContactService
        {
            NextResult = ContactSubmissionResult.Success()
        };
        var controller = CreateController(service);
        var viewModel = CreateValidViewModel();

        var result = await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(ThankYouController.Index), redirectResult.ActionName);
        Assert.AreEqual("ThankYou", redirectResult.ControllerName);
        Assert.AreEqual(1, service.SubmitCallCount);
        Assert.IsNotNull(service.LastRequest);
        Assert.AreEqual(viewModel.Name, service.LastRequest.Name);
    }

    [TestMethod]
    public async Task Contact_ModelStateInvalid_ReturnsSameViewAndDoesNotCallService()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);
        controller.ModelState.AddModelError(nameof(ContactFormViewModel.Name), "Name is required.");
        var viewModel = new ContactFormViewModel
        {
            Name = string.Empty,
            Email = "ana@example.com",
            Message = "short",
            SubmissionToken = string.Empty
        };

        var result = await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var viewResult = (ViewResult)result;
        Assert.IsNull(viewResult.ViewName);
        Assert.IsInstanceOfType(viewResult.Model, typeof(ContactFormViewModel));
        var returnedModel = (ContactFormViewModel)viewResult.Model!;
        Assert.AreEqual(0, service.SubmitCallCount);
        Assert.IsFalse(string.IsNullOrWhiteSpace(returnedModel.SubmissionToken));
    }

    [TestMethod]
    public async Task Contact_ModelStateInvalid_PreservesValidationErrors()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);
        controller.ModelState.AddModelError(nameof(ContactFormViewModel.Email), "Enter a valid email address.");
        var viewModel = CreateValidViewModel();

        await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.IsTrue(controller.ModelState.ContainsKey(nameof(ContactFormViewModel.Email)));
        Assert.IsTrue(controller.ModelState.ContainsKey(string.Empty));
    }

    [TestMethod]
    public async Task Contact_ServiceReturnsDuplicate_RedirectsToContactAndSetsInfoMessage()
    {
        var service = new FakeContactService
        {
            NextResult = ContactSubmissionResult.Duplicate()
        };
        var controller = CreateController(service);
        var viewModel = CreateValidViewModel();

        var result = await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        var redirectResult = (RedirectToActionResult)result;
        Assert.AreEqual(nameof(ContactController.Contact), redirectResult.ActionName);
        Assert.IsNull(redirectResult.ControllerName);
        Assert.IsTrue(controller.TempData.ContainsKey("ContactInfoMessage"));
    }

    [TestMethod]
    public async Task Contact_ServiceReturnsDuplicate_DoesNotReportSuccess()
    {
        var service = new FakeContactService
        {
            NextResult = ContactSubmissionResult.Duplicate()
        };
        var controller = CreateController(service);

        await controller.Contact(CreateValidViewModel(), CancellationToken.None);

        Assert.AreEqual(1, service.SubmitCallCount);
        Assert.IsFalse(service.NextResult.IsSuccess);
        Assert.IsTrue(service.NextResult.IsDuplicate);
    }

    [TestMethod]
    public async Task Contact_ValidModel_PreservesWhitespaceForServiceLayerToTrim()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);
        var viewModel = new ContactFormViewModel
        {
            Name = "  Ana  ",
            Email = "  ana@example.com  ",
            Phone = "  +12345  ",
            Message = "  This is a valid contact message.  ",
            SubmissionToken = "abcdefabcdefabcdefabcdefabcdefab"
        };

        await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsNotNull(service.LastRequest);
        Assert.AreEqual("  Ana  ", service.LastRequest.Name);
        Assert.AreEqual("  ana@example.com  ", service.LastRequest.Email);
        Assert.AreEqual("  +12345  ", service.LastRequest.Phone);
        Assert.AreEqual("  This is a valid contact message.  ", service.LastRequest.Message);
    }

    [TestMethod]
    public async Task Contact_ValidModel_WithNullPhone_CallsServiceWithNullPhone()
    {
        var service = new FakeContactService();
        var controller = CreateController(service);
        var viewModel = CreateValidViewModel();
        viewModel.Phone = null;

        await controller.Contact(viewModel, CancellationToken.None);

        Assert.IsNotNull(service.LastRequest);
        Assert.IsNull(service.LastRequest.Phone);
    }

    [TestMethod]
    public void Contact_PostAction_HasValidateAntiForgeryTokenAttribute()
    {
        var method = typeof(ContactController).GetMethods()
            .Single(m => m.Name == nameof(ContactController.Contact)
                && m.GetParameters().Length == 2
                && m.GetParameters()[0].ParameterType == typeof(ContactFormViewModel));

        Assert.IsTrue(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: false).Any());
        Assert.IsTrue(method.GetCustomAttributes(typeof(HttpPostAttribute), inherit: false).Any());
    }

    private static ContactController CreateController(FakeContactService service)
    {
        var controller = new ContactController(service)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
        };

        return controller;
    }

    private static ContactFormViewModel CreateValidViewModel()
    {
        return new ContactFormViewModel
        {
            Name = "Ana",
            Email = "ana@example.com",
            Phone = "+12345",
            Message = "This is a valid contact message.",
            SubmissionToken = "abcdefabcdefabcdefabcdefabcdefab"
        };
    }

    private sealed class FakeContactService : IContactService
    {
        public int SubmitCallCount { get; private set; }

        public ContactSubmissionRequest? LastRequest { get; private set; }

        public ContactSubmissionResult NextResult { get; set; } = ContactSubmissionResult.Success();

        public Task<ContactSubmissionResult> SubmitContactAsync(ContactSubmissionRequest request, CancellationToken cancellationToken)
        {
            SubmitCallCount++;
            LastRequest = request;
            return Task.FromResult(NextResult);
        }

        public Task<IReadOnlyList<ContactSubmissionSummary>> GetAllSubmissionsAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ContactSubmissionSummary> result = Array.Empty<ContactSubmissionSummary>();
            return Task.FromResult(result);
        }

        public Task<ContactSubmissionSummary?> GetSubmissionByIdAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<ContactSubmissionSummary?>(null);
        }

        public Task<int> CreateSubmissionAsync(AdminSubmissionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(1);
        }

        public Task<bool> UpdateSubmissionAsync(int id, AdminSubmissionRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> DeleteSubmissionAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> SetReplyAsync(int id, string reply, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object?> _values = new Dictionary<string, object?>();

        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            return _values;
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            _values = new Dictionary<string, object?>(values);
        }
    }
}
