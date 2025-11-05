using Microsoft.EntityFrameworkCore;
using PhoneBookRestApi.Commands;
using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data;

namespace PhoneBookRestApi.Handlers
{
    public class UpdatePhoneBookEntryCommandHandler : IRequestHandler<UpdatePhoneBookEntryCommand, bool>
    {
        private readonly PhoneBookContext _context;

        public UpdatePhoneBookEntryCommandHandler(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdatePhoneBookEntryCommand request, CancellationToken cancellationToken)
        {
            var existingEntry = await _context.PhoneBookEntries.FindAsync(new object[] { request.Id }, cancellationToken);

            if (existingEntry == null)
            {
                return false;
            }

            existingEntry.Name = request.Entry.Name;
            existingEntry.PhoneNumber = request.Entry.PhoneNumber;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PhoneBookEntryExists(request.Id, cancellationToken))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> PhoneBookEntryExists(int id, CancellationToken cancellationToken)
        {
            return await _context.PhoneBookEntries.AnyAsync(e => e.Id == id, cancellationToken);
        }
    }
}
