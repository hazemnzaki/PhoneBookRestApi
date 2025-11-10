namespace PhoneBookRestApi.Queries
{
    public class GetPhoneBookEntryByNameQuery
    {
        public string Name { get; }

        public GetPhoneBookEntryByNameQuery(string name)
        {
            Name = name;
        }
    }
}
