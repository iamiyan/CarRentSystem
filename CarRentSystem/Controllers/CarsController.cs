using CarRentSystem.Data;
using CarRentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CarRentSystem.Controllers;

[Route("api/cars")]
[ApiController]
public class CarsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Car>>> GetCars()
    {
        return await _context.Cars.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Car>> GetCar(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();
        return car;
    }

    [HttpPost]
    public async Task<ActionResult<Car>> CreateCar(Car car)
    {
        _context.Cars.Add(car);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCar), new { id = car.Id }, car);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCar(int id, Car car)
    {
        if (id != car.Id) return BadRequest();
        _context.Entry(car).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var car = await _context.Cars.FindAsync(id);
        if (car == null) return NotFound();
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

[Route("cars")] // Removed leading slash for correct routing
public class CarsController : Controller
{
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    public IActionResult Create(Car car)
    {
        if (ModelState.IsValid)
        {
            _context.Cars.Add(car);
            _context.SaveChanges();
            return RedirectToAction("AdminManageCars");
        }
        return View(car);
    }


    [HttpGet("AdminManageCars")]
    public IActionResult AdminManageCars()
    {
        var cars = _context.Cars.ToList();
        return View(cars);
    }

    [HttpGet("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var car = _context.Cars.Find(id);
        if (car == null)
        {
            return NotFound();
        }
        return View(car);
    }

    [HttpPost("Edit/{id}")]
    public IActionResult Edit(int id, Car car)
    {
        if (id != car.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            _context.Cars.Update(car);
            _context.SaveChanges();
            return RedirectToAction("AdminManageCars");
        }
        return View(car);
    }

    [HttpPost("Delete/{id}")]
    public IActionResult Delete(int id)
    {
        var car = _context.Cars.Find(id);
        if (car == null)
        {
            return Json(new { success = false, message = "Car not found." });
        }

        _context.Cars.Remove(car);
        _context.SaveChanges();

        return Json(new { success = true, message = "Car deleted successfully." });
    }

}

