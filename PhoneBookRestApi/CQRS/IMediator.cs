namespace PhoneBookRestApi.CQRS
{
    /// <summary>
    /// Defines a mediator to send requests and receive responses
    /// </summary>
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
