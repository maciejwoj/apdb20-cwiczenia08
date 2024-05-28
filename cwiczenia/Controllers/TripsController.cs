using cwiczenia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var trips = await _context.Trips.Select(e => new
        {
            Name = e.Name,
            Countries = e.IdCountries.Select(c => new
            {
                Name = c.Name
            })
        }).ToListAsync();
        
        return Ok(trips);
    }

}