using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Commands
{
    public class CreatePhoneBookEntryCommand
    {
        public PhoneBookEntry Entry { get; }

        public CreatePhoneBookEntryCommand(PhoneBookEntry entry)
        {
            Entry = entry;
        }
    }
}
