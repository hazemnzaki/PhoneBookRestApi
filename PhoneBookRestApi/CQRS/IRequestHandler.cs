namespace PhoneBookRestApi.CQRS
{
    /// <summary>
    /// Defines a handler for a request
    /// </summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
