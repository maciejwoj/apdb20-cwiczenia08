using cwiczenia.Data;
using cwiczenia.Models;
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
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Countries = t.IdCountries.Select(c => new { c.Name }),
                Clients = t.ClientTrips.Select(ct => new { ct.IdClientNavigation.FirstName, ct.IdClientNavigation.LastName })
            })
            .ToListAsync();

        var response = new
        {
            pageNum = page,
            pageSize,
            allPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            trips
        };

        return Ok(response);
    }



    [HttpDelete("clients/{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
        {
            return NotFound("Client not found.");
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest("Client is assigned to one or more trips and cannot be deleted.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip,  Client newClient)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);

        if (trip == null || trip.DateFrom <= DateTime.Now)
        {
            return BadRequest("Trip does not exist or has already started.");
        }

        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == newClient.Pesel);

        if (existingClient != null)
        {
            var clientTrip = await _context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);

            if (clientTrip != null)
            {
                return BadRequest("Client is already assigned to this trip.");
            }

            _context.ClientTrips.Add(new ClientTrip
            {
                IdClient = existingClient.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = newClient.ClientTrips.FirstOrDefault()?.PaymentDate
            });
        }
        else
        {
            newClient.ClientTrips = new List<ClientTrip>
            {
                new ClientTrip
                {
                    IdTrip = idTrip,
                    RegisteredAt = DateTime.Now,
                    PaymentDate = newClient.ClientTrips.FirstOrDefault()?.PaymentDate
                }
            };
            _context.Clients.Add(newClient);
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(AssignClientToTrip), new { idTrip, idClient = newClient.IdClient }, newClient);
    }


}