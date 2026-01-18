using API.Shared.Helpers;

namespace API.Application.Services.ReviewaDataFilter
{
    public interface IScraperHttpClientService
    {
        Task<TResponse?> GetReviewsAsync<TResponse>(
            string baseUrl,
            string endpoint,
            List<QueryParam> queryParameters,
            CancellationToken cancellationToken = default) where TResponse : class;
    }
}
