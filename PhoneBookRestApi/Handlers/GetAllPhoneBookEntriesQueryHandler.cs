using MediatR;
using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Handlers
{
    public class GetAllPhoneBookEntriesQueryHandler : IRequestHandler<GetAllPhoneBookEntriesQuery, IEnumerable<PhoneBookEntry>>
    {
        private readonly PhoneBookContext _context;

        public GetAllPhoneBookEntriesQueryHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhoneBookEntry>> Handle(GetAllPhoneBookEntriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.PhoneBookEntries.ToListAsync(cancellationToken);
        }
    }
}
