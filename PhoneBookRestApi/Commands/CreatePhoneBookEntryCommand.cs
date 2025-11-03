using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Commands
{
    public class CreatePhoneBookEntryCommand : IRequest<PhoneBookEntry>
    {
        public PhoneBookEntry Entry { get; }

        public CreatePhoneBookEntryCommand(PhoneBookEntry entry)
        {
            Entry = entry;
        }
    }
}
