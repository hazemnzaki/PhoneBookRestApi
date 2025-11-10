namespace PhoneBookRestApi.Queries
{
    public class GetPhoneBookEntryByIdQuery
    {
        public int Id { get; }

        public GetPhoneBookEntryByIdQuery(int id)
        {
            Id = id;
        }
    }
}
