using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Models;

namespace PhoneBookRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneBookController : ControllerBase
    {
        private readonly PhoneBookContext _context;

        public PhoneBookController(PhoneBookContext context)
        {
            _context = context;
        }

        // GET: api/PhoneBook
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhoneBookEntry>>> GetPhoneBookEntries()
        {
            return await _context.PhoneBookEntries.ToListAsync();
        }

        // GET: api/PhoneBook/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhoneBookEntry>> GetPhoneBookEntry(int id)
        {
            var phoneBookEntry = await _context.PhoneBookEntries.FindAsync(id);

            if (phoneBookEntry == null)
            {
                return NotFound();
            }

            return phoneBookEntry;
        }

        // GET: api/PhoneBook/ByName/{name}
        [HttpGet("ByName/{name}")]
        public async Task<ActionResult<PhoneBookEntry>> GetPhoneBookEntryByName(string name)
        {
            var phoneBookEntry = await _context.PhoneBookEntries
                .FirstOrDefaultAsync(p => p.Name == name);

            if (phoneBookEntry == null)
            {
                return NotFound();
            }

            return phoneBookEntry;
        }

        // POST: api/PhoneBook
        [HttpPost]
        public async Task<ActionResult<PhoneBookEntry>> PostPhoneBookEntry(PhoneBookEntry phoneBookEntry)
        {
            _context.PhoneBookEntries.Add(phoneBookEntry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhoneBookEntry), new { id = phoneBookEntry.Id }, phoneBookEntry);
        }

        // PUT: api/PhoneBook/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoneBookEntry(int id, PhoneBookEntry phoneBookEntry)
        {
            if (id != phoneBookEntry.Id)
            {
                return BadRequest();
            }

            _context.Entry(phoneBookEntry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhoneBookEntryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/PhoneBook/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoneBookEntry(int id)
        {
            var phoneBookEntry = await _context.PhoneBookEntries.FindAsync(id);
            if (phoneBookEntry == null)
            {
                return NotFound();
            }

            _context.PhoneBookEntries.Remove(phoneBookEntry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PhoneBookEntryExists(int id)
        {
            return _context.PhoneBookEntries.Any(e => e.Id == id);
        }
    }
}
