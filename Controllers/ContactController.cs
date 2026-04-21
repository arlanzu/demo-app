using System.Security.Cryptography;
using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public IActionResult Contact()
    {
        return View(CreateContactFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please correct the highlighted fields and try again.");
            EnsureSubmissionToken(viewModel);
            return View(viewModel);
        }

        var request = new ContactSubmissionRequest(
            viewModel.Name,
            viewModel.Email,
            viewModel.Phone,
            viewModel.Message,
            viewModel.SubmissionToken);

        var result = await _contactService.SubmitContactAsync(request, cancellationToken);

        if (result.IsDuplicate)
        {
            TempData["ContactInfoMessage"] = "Your message has already been submitted. We ignored the duplicate request.";
            return RedirectToAction(nameof(Contact));
        }

        return RedirectToAction(nameof(ThankYouController.Index), "ThankYou");
    }

    private static string CreateSubmissionToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
    }

    private static ContactFormViewModel CreateContactFormViewModel()
    {
        return new ContactFormViewModel
        {
            SubmissionToken = CreateSubmissionToken()
        };
    }

    private static void EnsureSubmissionToken(ContactFormViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.SubmissionToken))
        {
            viewModel.SubmissionToken = CreateSubmissionToken();
        }
    }
}
