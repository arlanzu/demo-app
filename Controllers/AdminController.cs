using DemoApp.Application.Interfaces;
using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Controllers;

public class AdminController : Controller
{
    private readonly IContactService _contactService;

    public AdminController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var submissions = await _contactService.GetAllSubmissionsAsync(cancellationToken);

        var viewModel = new AdminSubmissionsViewModel
        {
            Submissions = submissions
                .Select(s => new AdminSubmissionItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    Message = s.Message,
                    CreatedAtUtc = s.CreatedAtUtc
                })
                .ToList()
        };

        return View(viewModel);
    }
}
