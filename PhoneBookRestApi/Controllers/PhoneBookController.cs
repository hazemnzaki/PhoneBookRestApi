using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Dtos;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneBookController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public PhoneBookController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        // GET: api/PhoneBook
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhoneBookEntryDto>>> GetPhoneBookEntries()
        {
            var entries = await _mediator.Send(new GetAllPhoneBookEntriesQuery());
            var dtos = _mapper.Map<IEnumerable<PhoneBookEntryDto>>(entries);
            return Ok(dtos);
        }

        // GET: api/PhoneBook/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhoneBookEntryDto>> GetPhoneBookEntry(int id)
        {
            var phoneBookEntry = await _mediator.Send(new GetPhoneBookEntryByIdQuery(id));

            if (phoneBookEntry == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<PhoneBookEntryDto>(phoneBookEntry);
            return dto;
        }

        // GET: api/PhoneBook/ByName/{name}
        [HttpGet("ByName/{name}")]
        public async Task<ActionResult<PhoneBookEntryDto>> GetPhoneBookEntryByName(string name)
        {
            var phoneBookEntry = await _mediator.Send(new GetPhoneBookEntryByNameQuery(name));

            if (phoneBookEntry == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<PhoneBookEntryDto>(phoneBookEntry);
            return dto;
        }

        // POST: api/PhoneBook
        [HttpPost]
        public async Task<ActionResult<PhoneBookEntryDto>> PostPhoneBookEntry(CreatePhoneBookEntryDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phoneBookEntry = _mapper.Map<PhoneBookEntry>(createDto);
            var createdEntry = await _mediator.Send(new CreatePhoneBookEntryCommand(phoneBookEntry));
            var dto = _mapper.Map<PhoneBookEntryDto>(createdEntry);

            return CreatedAtAction(nameof(GetPhoneBookEntry), new { id = dto.Id }, dto);
        }

        // PUT: api/PhoneBook/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhoneBookEntry(int id, UpdatePhoneBookEntryDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phoneBookEntry = _mapper.Map<PhoneBookEntry>(updateDto);
            phoneBookEntry.Id = id;

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
