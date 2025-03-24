using System.Diagnostics;
using CarRentSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarRentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Auth()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult CustomerDashboard()
        {
            return View();
        }

        public IActionResult Reservations()
        {
            return View();
        }

        public IActionResult Cars()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        [HttpGet("AboutPage")]
        public IActionResult AboutPage()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
