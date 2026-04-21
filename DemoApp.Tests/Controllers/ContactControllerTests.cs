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
        Assert.IsInstanceOfType(viewResult.Model, typeof(ContactFormViewModel));
        var returnedModel = (ContactFormViewModel)viewResult.Model!;
        Assert.AreEqual(0, service.CallCount);
        Assert.IsFalse(string.IsNullOrWhiteSpace(returnedModel.SubmissionToken));
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
        Assert.AreEqual(1, service.CallCount);
        Assert.IsNotNull(service.LastRequest);
        Assert.AreEqual(viewModel.Name, service.LastRequest.Name);
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
        Assert.IsTrue(controller.TempData.ContainsKey("ContactInfoMessage"));
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
        public int CallCount { get; private set; }

        public ContactSubmissionRequest? LastRequest { get; private set; }

        public ContactSubmissionResult NextResult { get; set; } = ContactSubmissionResult.Success();

        public Task<ContactSubmissionResult> SubmitContactAsync(ContactSubmissionRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(NextResult);
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
