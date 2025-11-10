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
        private readonly IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>> _getAllHandler;
        private readonly IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?> _getByIdHandler;
        private readonly IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?> _getByNameHandler;
        private readonly IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry> _createHandler;
        private readonly IRequestHandler<UpdatePhoneBookEntryCommand, bool> _updateHandler;
        private readonly IRequestHandler<DeletePhoneBookEntryCommand, bool> _deleteHandler;
        private readonly IMapper _mapper;

        public PhoneBookController(
            IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>> getAllHandler,
            IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?> getByIdHandler,
            IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?> getByNameHandler,
            IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry> createHandler,
            IRequestHandler<UpdatePhoneBookEntryCommand, bool> updateHandler,
            IRequestHandler<DeletePhoneBookEntryCommand, bool> deleteHandler,
            IMapper mapper)
        {
            _getAllHandler = getAllHandler;
            _getByIdHandler = getByIdHandler;
            _getByNameHandler = getByNameHandler;
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _deleteHandler = deleteHandler;
            _mapper = mapper;
        }

        // GET: api/PhoneBook
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhoneBookEntryDto>>> GetPhoneBookEntries()
        {
            var entries = await _getAllHandler.Handle(new GetAllPhoneBookEntriesQuery(), CancellationToken.None);
            var dtos = _mapper.Map<IEnumerable<PhoneBookEntryDto>>(entries);
            return Ok(dtos);
        }

        // GET: api/PhoneBook/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PhoneBookEntryDto>> GetPhoneBookEntry(int id)
        {
            var phoneBookEntry = await _getByIdHandler.Handle(new GetPhoneBookEntryByIdQuery(id), CancellationToken.None);

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
            var phoneBookEntry = await _getByNameHandler.Handle(new GetPhoneBookEntryByNameQuery(name), CancellationToken.None);

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
            var createdEntry = await _createHandler.Handle(new CreatePhoneBookEntryCommand(phoneBookEntry), CancellationToken.None);
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

            var success = await _updateHandler.Handle(new UpdatePhoneBookEntryCommand(id, phoneBookEntry), CancellationToken.None);

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
            var success = await _deleteHandler.Handle(new DeletePhoneBookEntryCommand(id), CancellationToken.None);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
