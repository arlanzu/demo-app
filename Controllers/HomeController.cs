using System.Diagnostics;
using DemoApp.Application.Contracts;
using DemoApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DemoApp.Models;
using DemoApp.Models.ViewModels;

namespace DemoApp.Controllers;

public class HomeController : Controller
{
    private readonly IContactService _contactService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IContactService contactService, ILogger<HomeController> logger)
    {
        _contactService = contactService;
        _logger = logger;
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
        var viewModel = new ContactFormViewModel
        {
            SubmissionToken = Guid.NewGuid().ToString("N")
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            viewModel.SubmissionToken = string.IsNullOrWhiteSpace(viewModel.SubmissionToken)
                ? Guid.NewGuid().ToString("N")
                : viewModel.SubmissionToken;
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
}
