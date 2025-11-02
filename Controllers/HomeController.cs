using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LaptopStore.Models;

namespace LaptopStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Contact()
    {
        ViewData["Title"] = "Contact Us";
        return View();
    }

    public IActionResult Shipping()
    {
        ViewData["Title"] = "Shipping Information";
        return View();
    }

    public IActionResult Returns()
    {
        ViewData["Title"] = "Returns & Refunds";
        return View();
    }

    public IActionResult FAQ()
    {
        ViewData["Title"] = "Frequently Asked Questions";
        return View();
    }

    public IActionResult Warranty()
    {
        ViewData["Title"] = "Warranty Information";
        return View();
    }

    public IActionResult About()
    {
        ViewData["Title"] = "About Us";
        return View();
    }

    public IActionResult Privacy()
    {
        ViewData["Title"] = "Privacy Policy";
        return View();
    }

    public IActionResult Terms()
    {
        ViewData["Title"] = "Terms of Service";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}