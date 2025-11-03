using MediatR;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Commands
{
    public class UpdatePhoneBookEntryCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public PhoneBookEntry Entry { get; set; }

        public UpdatePhoneBookEntryCommand(int id, PhoneBookEntry entry)
        {
            Id = id;
            Entry = entry;
        }
    }
}
