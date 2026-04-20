using System.Diagnostics;
using System.Security.Cryptography;
using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DemoApp.Models;
using DemoApp.Models.ViewModels;

namespace DemoApp.Controllers;

public class HomeController : Controller
{
    private readonly IContactService _contactService;

    public HomeController(IContactService contactService)
    {
        _contactService = contactService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
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

        return RedirectToAction(nameof(ThankYou));
    }

    [HttpGet]
    public IActionResult ThankYou()
    {
        var viewModel = new ContactThankYouViewModel
        {
            Heading = "Thank You",
            Message = "Your message has been submitted successfully."
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
