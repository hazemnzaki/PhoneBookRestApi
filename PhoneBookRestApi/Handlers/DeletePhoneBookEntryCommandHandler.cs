using MediatR;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.Data;

namespace PhoneBookRestApi.Handlers
{
    public class DeletePhoneBookEntryCommandHandler : IRequestHandler<DeletePhoneBookEntryCommand, bool>
    {
        private readonly PhoneBookContext _context;

        public DeletePhoneBookEntryCommandHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeletePhoneBookEntryCommand request, CancellationToken cancellationToken)
        {
            var phoneBookEntry = await _context.PhoneBookEntries.FindAsync(new object[] { request.Id }, cancellationToken);
            if (phoneBookEntry == null)
            {
                return false;
            }

            _context.PhoneBookEntries.Remove(phoneBookEntry);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
