using API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.DTOs;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Queries
{
    public record GetOnlineReviewsChannelByCompanyIDQuery(List<long> companiesIDs) :IRequest<RequestResult<List<OnlineReviewsChannelDTO>>>;
    public class GetOnlineReviewsChannelByCompanyIDQueryHandler:IRequestHandler<GetOnlineReviewsChannelByCompanyIDQuery, RequestResult<List<OnlineReviewsChannelDTO>>>
    {
        private readonly IRepository<Domain.Entities.Client.Channel> _repository;

        public GetOnlineReviewsChannelByCompanyIDQueryHandler(IRepository<Domain.Entities.Client.Channel> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<List<OnlineReviewsChannelDTO>>> Handle(GetOnlineReviewsChannelByCompanyIDQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Get(ch => request.companiesIDs.Contains(ch.CompanyID) && ch.Name.Contains("Online Reviews"))
                                .Select(ch => new OnlineReviewsChannelDTO ()
                                {
                                    ID = ch.ID,
                                    Name = ch.Name,
                                    CompanyID = ch.CompanyID
                                }).ToListAsync();
            
            if (result == null || result.Count == 0)
                return RequestResult<List<OnlineReviewsChannelDTO>>.Failure(ErrorCode.NotFound);

            return RequestResult<List<OnlineReviewsChannelDTO>>.Success(result);

        }
    }
}
