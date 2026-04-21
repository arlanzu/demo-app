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

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AdminSubmissionFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminSubmissionFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var request = new Application.Contracts.AdminSubmissionRequest(
            viewModel.Name,
            viewModel.Email,
            viewModel.Phone,
            viewModel.Message);

        await _contactService.CreateSubmissionAsync(request, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var submission = await _contactService.GetSubmissionByIdAsync(id, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        var viewModel = new AdminSubmissionDetailsViewModel
        {
            Id = submission.Id,
            Name = submission.Name,
            Email = submission.Email,
            Phone = submission.Phone,
            Message = submission.Message,
            CreatedAtUtc = submission.CreatedAtUtc
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var submission = await _contactService.GetSubmissionByIdAsync(id, cancellationToken);

        if (submission is null)
        {
            return NotFound();
        }

        var viewModel = new AdminSubmissionFormViewModel
        {
            Id = submission.Id,
            Name = submission.Name,
            Email = submission.Email,
            Phone = submission.Phone,
            Message = submission.Message
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminSubmissionFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var request = new Application.Contracts.AdminSubmissionRequest(
            viewModel.Name,
            viewModel.Email,
            viewModel.Phone,
            viewModel.Message);

        var updated = await _contactService.UpdateSubmissionAsync(id, request, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _contactService.DeleteSubmissionAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
