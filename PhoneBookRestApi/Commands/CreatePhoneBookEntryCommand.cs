using MediatR;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Commands
{
    public class CreatePhoneBookEntryCommand : IRequest<PhoneBookEntry>
    {
        public PhoneBookEntry Entry { get; set; }

        public CreatePhoneBookEntryCommand(PhoneBookEntry entry)
        {
            Entry = entry;
        }
    }
}
