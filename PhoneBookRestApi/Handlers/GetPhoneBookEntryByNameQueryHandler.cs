using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Handlers
{
    public class GetPhoneBookEntryByNameQueryHandler : IRequestHandler<GetPhoneBookEntryByNameQuery, PhoneBookEntry?>
    {
        private readonly PhoneBookContext _context;

        public GetPhoneBookEntryByNameQueryHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<PhoneBookEntry?> Handle(GetPhoneBookEntryByNameQuery request, CancellationToken cancellationToken)
        {
            return await _context.PhoneBookEntries
                .FirstOrDefaultAsync(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }
    }
}
