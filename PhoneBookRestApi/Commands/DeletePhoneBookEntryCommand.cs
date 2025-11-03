using MediatR;

namespace PhoneBookRestApi.Commands
{
    public class DeletePhoneBookEntryCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeletePhoneBookEntryCommand(int id)
        {
            Id = id;
        }
    }
}
