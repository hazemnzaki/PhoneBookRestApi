using PhoneBookRestApi.CQRS;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Queries
{
    public class GetAllPhoneBookEntriesQuery : IRequest<IEnumerable<PhoneBookEntry>>
    {
    }
}
