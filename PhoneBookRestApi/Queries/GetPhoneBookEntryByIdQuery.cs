using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Queries
{
    public class GetPhoneBookEntryByIdQuery : IRequest<PhoneBookEntry?>
    {
        public int Id { get; }

        public GetPhoneBookEntryByIdQuery(int id)
        {
            Id = id;
        }
    }
}
