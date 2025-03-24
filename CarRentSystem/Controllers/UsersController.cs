using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentSystem.Data;
using CarRentSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/users")]
[ApiController]
public class UsersApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users (Get all users)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/users/{id} (Get user by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return user;
    }

    // POST: api/users (Create user)
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/users/{id} (Update user)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id) return BadRequest();
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/users/{id} (Delete user)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

[Route("users")] // Change "user" to "users" for consistency
public class UsersController : Controller
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("AdminManageUsers")]
    public IActionResult AdminManageUsers()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    // GET: Show Create User form
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create new user
    [HttpPost("Create")]
    public IActionResult Create(User user)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is INVALID!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Validation Error: {error.ErrorMessage}");
            }

            return View(user);
        }

        Console.WriteLine("Creating new user...");

        _context.Users.Add(user);
        _context.SaveChanges();

        Console.WriteLine("User successfully created!");

        return RedirectToAction("AdminManageUsers");
    }

    [HttpGet("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost("Edit/{id}")]
    public IActionResult Edit(int id, User user)
    {
        Console.WriteLine("Edit method called");

        if (id != user.Id)
        {
            Console.WriteLine("User ID mismatch!");
            return BadRequest("User ID mismatch.");
        }

        var existingUser = _context.Users.Find(id);
        if (existingUser == null)
        {
            Console.WriteLine("User not found!");
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is INVALID!");

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Validation Error: {error.ErrorMessage}");
            }

            return View(user);
        }

        Console.WriteLine("Updating user info...");

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Role = user.Role;
        existingUser.Password = user.Password; // ✅ Directly updating password without hashing

        _context.Users.Update(existingUser);
        _context.SaveChanges();

        Console.WriteLine("User successfully updated!");

        return RedirectToAction("AdminManageUsers"); // ✅ Redirect after success
    }


    [HttpPost("Delete/{id}")]
    public IActionResult Delete(int id)
    {
        Console.WriteLine($"Delete method called for User ID: {id}");

        var user = _context.Users.Find(id);
        if (user == null)
        {
            Console.WriteLine("User not found!");
            return Json(new { success = false, message = "User not found." });
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        Console.WriteLine("User successfully deleted!");

        return Json(new { success = true, message = "User deleted successfully." });
    }

}
