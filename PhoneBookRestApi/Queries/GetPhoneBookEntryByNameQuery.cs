using MediatR;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Queries
{
    public class GetPhoneBookEntryByNameQuery : IRequest<PhoneBookEntry?>
    {
        public string Name { get; }

        public GetPhoneBookEntryByNameQuery(string name)
        {
            Name = name;
        }
    }
}
