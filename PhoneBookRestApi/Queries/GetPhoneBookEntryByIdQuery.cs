using MediatR;
using PhoneBookRestApi.Data.Models;

namespace PhoneBookRestApi.Queries
{
    public class GetPhoneBookEntryByIdQuery : IRequest<PhoneBookEntry?>
    {
        public int Id { get; set; }

        public GetPhoneBookEntryByIdQuery(int id)
        {
            Id = id;
        }
    }
}
