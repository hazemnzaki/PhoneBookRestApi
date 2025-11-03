using PhoneBookRestApi.CQRS;

namespace PhoneBookRestApi.Commands
{
    public class DeletePhoneBookEntryCommand : IRequest<bool>
    {
        public int Id { get; }

        public DeletePhoneBookEntryCommand(int id)
        {
            Id = id;
        }
    }
}
