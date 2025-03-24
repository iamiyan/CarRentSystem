namespace CarRentSystem.Controllers;
using CarRentSystem.Data;
using CarRentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/reservations")]
[ApiController]
public class ReservationsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationsApiController(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Admin can view all reservations
    [HttpGet]
    public IActionResult GetAllReservations()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
            return Unauthorized(new { Message = "Only admins can view reservations!" });

        var reservations = _context.Reservations
            .Include(r => r.Car)  // ✅ Join with Cars table
            .Include(r => r.User) // ✅ Join with Users table
            .Select(r => new
            {
                r.Id,
                UserName = r.User.Name,  // ✅ Fetch User Name
                CarModel = r.Car.Model,  // ✅ Fetch Car Model
                StartDate = r.StartDate.ToString("yyyy-MM-dd"),
                EndDate = r.EndDate.ToString("yyyy-MM-dd"),
                r.TotalPrice,
                r.Status
            })
            .ToList();

        return Ok(reservations);
    }


    // ✅ Users can view only their own reservations
    [HttpGet("user")]
    public IActionResult GetUserReservations()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");

        if (string.IsNullOrEmpty(userRole) || userId == null)
        {
            return Unauthorized(new { Message = "User session is invalid!", UserRole = userRole, UserId = userId });
        }

        if (userRole != "User")
        {
            return Unauthorized(new { Message = "Only users can view their reservations!" });
        }

        var userReservations = _context.Reservations
            .Where(r => r.UserId == userId)
            .Join(
                _context.Cars,  // Join with Cars table
                reservation => reservation.CarId,
                car => car.Id,
                (reservation, car) => new  // Select required fields
                {
                    reservation.Id,
                    CarModel = car.Model,  // ✅ Fetch Car Model instead of CarId
                    StartDate = reservation.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = reservation.EndDate.ToString("yyyy-MM-dd"),
                    reservation.TotalPrice,
                    reservation.Status
                })
            .ToList();

        return Ok(userReservations);
    }


    // ✅ Users can create reservations for themselves
    [HttpPost]
    public IActionResult Create([FromBody] Reservation reservation)
    {
        if (reservation == null)
        {
            return BadRequest(new { Message = "Invalid reservation data!" });
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized(new { Message = "User not logged in!" });
        }

        // Check if the UserId exists
        var userExists = _context.Users.Any(u => u.Id == userId);
        if (!userExists)
        {
            return BadRequest(new { Message = "User does not exist!" });
        }

        // Check if the CarId exists
        var car = _context.Cars.Find(reservation.CarId);
        if (car == null)
        {
            return BadRequest(new { Message = "Car does not exist!" });
        }

        // Validate date selection
        if (reservation.StartDate >= reservation.EndDate)
        {
            return BadRequest(new { Message = "End date must be after start date!" });
        }

        // Calculate total price
        int days = (int)(reservation.EndDate - reservation.StartDate).TotalDays;
        reservation.TotalPrice = days * car.PricePerDay;
        reservation.UserId = (int)userId;
        reservation.Status = "Pending"; // Default status

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        return Ok(new { Message = "Reservation created successfully!", ReservationId = reservation.Id });
    }

    // ✅ Admin can update reservations
    [HttpPost("UpdateReservation")]
    public IActionResult UpdateReservation([FromForm] Reservation updatedReservation)
    {
        var reservation = _context.Reservations.Find(updatedReservation.Id);
        if (reservation == null)
        {
            return NotFound(new { Message = "Reservation not found!" });
        }

        reservation.StartDate = updatedReservation.StartDate;
        reservation.EndDate = updatedReservation.EndDate;
        reservation.Status = updatedReservation.Status; // ✅ Allow status update

        var car = _context.Cars.Find(reservation.CarId);
        if (car != null)
        {
            reservation.TotalPrice = (int)(updatedReservation.EndDate - updatedReservation.StartDate).TotalDays * car.PricePerDay;
        }

        _context.SaveChanges();
        return RedirectToAction("AdminView");
    }

    [HttpDelete("Delete/{id}")]
    public IActionResult Delete(int id)
    {
        Console.WriteLine($"Deleting reservation with ID: {id}"); // ✅ Log ID for debugging

        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
        if (reservation == null)
        {
            return NotFound(new { message = $"Reservation with ID {id} not found." });
        }

        _context.Reservations.Remove(reservation);
        _context.SaveChanges();

        return Ok(new { message = "Reservation deleted successfully." });
    }

}


[Route("reservations")]
public class ReservationsController : Controller
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet] // Explicitly handle GET requests
    public IActionResult Index()
    {
        return View("Index"); // Returns the Reservations page
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create([FromForm] Reservation reservation)
    {
        Console.WriteLine("✅ Received Reservation Submission");
        Console.WriteLine($"CarId: {reservation.CarId}, StartDate: {reservation.StartDate}, EndDate: {reservation.EndDate}, TotalPrice: {reservation.TotalPrice}");

        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            Console.WriteLine("❌ Session expired or user not logged in!");
            ModelState.AddModelError("", "Session expired. Please log in again.");
            return RedirectToAction("Login", "Auth");
        }

        if (ModelState.IsValid)
        {
            var car = _context.Cars.Find(reservation.CarId);
            if (car == null)
            {
                Console.WriteLine("❌ Car not found!");
                ModelState.AddModelError("", "Car not found!");
                return View(reservation);
            }

            if (reservation.StartDate >= reservation.EndDate)
            {
                Console.WriteLine("❌ Invalid date selection!");
                ModelState.AddModelError("", "End date must be after start date!");
                return View(reservation);
            }

            // Assign UserId automatically
            reservation.UserId = userId.Value;
            reservation.TotalPrice = (int)(reservation.EndDate - reservation.StartDate).TotalDays * car.PricePerDay;

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            Console.WriteLine("✅ Reservation successfully created!");
            return RedirectToAction("CustView");
        }

        if (!ModelState.IsValid)
        {
            Console.WriteLine("❌ ModelState Errors:");
            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    Console.WriteLine($"Field: {error.Key}, Error: {subError.ErrorMessage}");
                }
            }
            return View(reservation);
        }


        Console.WriteLine("❌ ModelState is invalid!");
        return View(reservation);
    }

    [HttpGet("Create")]
    public IActionResult Create(int carId)
    {
        var car = _context.Cars.FirstOrDefault(c => c.Id == carId);
        if (car == null)
        {
            return NotFound();
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth"); // Redirect if user is not logged in
        }

        ViewBag.Car = car;
        ViewBag.UserId = userId; // Pass UserId to the view

        return View(new Reservation
        {
            CarId = car.Id,
            UserId = (int)userId // Auto-fill the UserId
        });
    }



    [HttpGet("CustView")]
    public IActionResult CustView()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "User")
        {
            return RedirectToAction("Login", "Auth");
        }
        return View();
    }

    [HttpGet("AdminView")]
    public IActionResult AdminView()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "Admin")
        {
            return RedirectToAction("Login", "Auth");
        }
        return View();
    }


    [HttpGet("AvailableCars")]
    public IActionResult AvailableCars()
    {
        var reservedCarIds = _context.Reservations
            .Where(r => r.Status == "Pending" || r.Status == "Approved")
            .Select(r => r.CarId)
            .Distinct()
            .ToList();

        var availableCars = _context.Cars
            .Where(c => !reservedCarIds.Contains(c.Id))
            .ToList();

        return View(availableCars); // Pass available cars to the view
    }

    [HttpGet("AdminEdit")]
    public IActionResult AdminEdit(int id)
    {
        var reservation = _context.Reservations.Find(id);
        if (reservation == null)
        {
            return NotFound();
        }

        // Retrieve the car associated with this reservation
        var car = _context.Cars.Find(reservation.CarId);
        if (car == null)
        {
            return NotFound("Car not found for this reservation.");
        }

        // Pass car details to the view
        ViewBag.Car = car;

        return View(reservation);
    }

    [HttpPost]
    public IActionResult UpdateReservation(Reservation model)
    {
        if (ModelState.IsValid)
        {
            var reservation = _context.Reservations.Find(model.Id);
            if (reservation == null)
            {
                return NotFound();
            }

            // ✅ Ensure the status is updated
            reservation.StartDate = model.StartDate;
            reservation.EndDate = model.EndDate;
            reservation.TotalPrice = model.TotalPrice;
            reservation.Status = model.Status; // ✅ This prevents resetting to "Pending"

            _context.SaveChanges();
            return RedirectToAction("AdminView");
        }

        return View(model);
    }


    public IActionResult Delete(int id)
    {
        var reservation = _context.Reservations.Find(id);
        if (reservation == null)
        {
            return NotFound();
        }

        _context.Reservations.Remove(reservation);
        _context.SaveChanges();

        return NoContent();
    }
}
