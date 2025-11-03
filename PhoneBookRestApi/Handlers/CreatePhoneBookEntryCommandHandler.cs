using MediatR;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Handlers
{
    public class CreatePhoneBookEntryCommandHandler : IRequestHandler<CreatePhoneBookEntryCommand, PhoneBookEntry>
    {
        private readonly PhoneBookContext _context;

        public CreatePhoneBookEntryCommandHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<PhoneBookEntry> Handle(CreatePhoneBookEntryCommand request, CancellationToken cancellationToken)
        {
            _context.PhoneBookEntries.Add(request.Entry);
            await _context.SaveChangesAsync(cancellationToken);
            return request.Entry;
        }
    }
}
