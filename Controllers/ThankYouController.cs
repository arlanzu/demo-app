using DemoApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Controllers;

public class ThankYouController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var viewModel = new ContactThankYouViewModel
        {
            Heading = "Thank You",
            Message = "Your message has been submitted successfully."
        };

        return View(viewModel);
    }
}
