using cwiczenia.Data;
using Microsoft.AspNetCore.Mvc;

namespace cwiczenia.Controllers;

[ApiController]
[Route(("api/[controller]"))]

public class TripsController : ControllerBase
{
    private readonly TempdbContext _context;

    public TripsController(TempdbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = _context.Trips.ToList();
        
        return Ok();
    }

}