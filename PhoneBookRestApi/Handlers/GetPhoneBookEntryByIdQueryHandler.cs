using MediatR;
using PhoneBookRestApi.Data;
using PhoneBookRestApi.Data.Models;
using PhoneBookRestApi.Queries;

namespace PhoneBookRestApi.Handlers
{
    public class GetPhoneBookEntryByIdQueryHandler : IRequestHandler<GetPhoneBookEntryByIdQuery, PhoneBookEntry?>
    {
        private readonly PhoneBookContext _context;

        public GetPhoneBookEntryByIdQueryHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<PhoneBookEntry?> Handle(GetPhoneBookEntryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.PhoneBookEntries.FindAsync(new object[] { request.Id }, cancellationToken);
        }
    }
}
