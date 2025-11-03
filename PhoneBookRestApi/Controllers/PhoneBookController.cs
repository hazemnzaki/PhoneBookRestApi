using MediatR;
using Microsoft.AspNetCore.Mvc;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneBookController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PhoneBookController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/PhoneBook
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhoneBookEntry>>> GetPhoneBookEntries()
        {
            var entries = await _mediator.Send(new GetAllPhoneBookEntriesQuery());
            return Ok(entries);
        }

        // GET: api/PhoneBook/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhoneBookEntry>> GetPhoneBookEntry(int id)
        {
            var phoneBookEntry = await _mediator.Send(new GetPhoneBookEntryByIdQuery(id));

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
            var phoneBookEntry = await _mediator.Send(new GetPhoneBookEntryByNameQuery(name));

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEntry = await _mediator.Send(new CreatePhoneBookEntryCommand(phoneBookEntry));

            return CreatedAtAction(nameof(GetPhoneBookEntry), new { id = createdEntry.Id }, createdEntry);
        }

        // PUT: api/PhoneBook/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoneBookEntry(int id, PhoneBookEntry phoneBookEntry)
        {
            if (id != phoneBookEntry.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _mediator.Send(new UpdatePhoneBookEntryCommand(id, phoneBookEntry));

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/PhoneBook/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoneBookEntry(int id)
        {
            var success = await _mediator.Send(new DeletePhoneBookEntryCommand(id));

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
