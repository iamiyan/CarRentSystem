using Microsoft.AspNetCore.Mvc;
using CarRentSystem.Data;
using CarRentSystem.Models;

[Route("Auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Display Login Page
    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }

    // ✅ Process Login Form
    [HttpPost("Login")]
    public IActionResult Login(User user)
    {
        var existingUser = _context.Users
            .FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

        if (existingUser != null)
        {
            HttpContext.Session.SetInt32("UserId", existingUser.Id);
            HttpContext.Session.SetString("UserRole", existingUser.Role);
            HttpContext.Session.SetString("Username", existingUser.Name);

            return existingUser.Role == "Admin"
                ? RedirectToAction("AdminDashboard", "Home")
                : RedirectToAction("CustomerDashboard", "Home");
        }

        ViewData["Error"] = "Invalid email or password.";
        return View();
    }

    // ✅ Display Register Page
    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View();
    }

    // ✅ Process Register Form
    [HttpPost("Register")]
    public IActionResult Register(User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ViewBag.ErrorMessage = "Email already registered!";
            return View();
        }

        if (string.IsNullOrWhiteSpace(user.Name)) // Ensure Name is not null
        {
            ViewBag.ErrorMessage = "Name is required!";
            return View();
        }

        user.Role = "User"; // Default role
        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }


    public IActionResult AdminDashboard()
    {
        if (HttpContext.Session.GetString("UserRole") == "Admin")
            return Content("Welcome, Admin!");
        return Unauthorized();
    }

    public IActionResult CustomerDashboard()
    {
        if (HttpContext.Session.GetString("UserRole") == "User")
            return Content("Welcome, Customer!");
        return Unauthorized();
    }

    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Clears all session data
        return RedirectToAction("Login", "Auth"); // Redirect to login page
    }

}

