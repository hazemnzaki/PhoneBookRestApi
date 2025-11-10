using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Commands
{
    public class UpdatePhoneBookEntryCommand
    {
        public int Id { get; }
        public PhoneBookEntry Entry { get; }

        public UpdatePhoneBookEntryCommand(int id, PhoneBookEntry entry)
        {
            Id = id;
            Entry = entry;
        }
    }
}
