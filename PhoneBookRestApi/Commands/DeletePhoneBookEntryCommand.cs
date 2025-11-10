namespace PhoneBookRestApi.Commands
{
    public class DeletePhoneBookEntryCommand
    {
        public int Id { get; }

        public DeletePhoneBookEntryCommand(int id)
        {
            Id = id;
        }
    }
}
