using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.AddLocation
{
    public record AddLocationCommand(
        long ID,
        string Name,
        List<int> TalabatLocationIDs,
        long CompanyID,
        DateTime CreatedAt,
        long CreatedBy,
        DateTime? UpdatedAt = null,
        long? UpdatedBy = null) : IRequest<RequestResult<Unit>>;

    public class AddLocationCommandHandler : IRequestHandler<AddLocationCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> repository;

        public AddLocationCommandHandler(IRepository<Location> _repository)
        {
            repository = _repository;
        }

        public Task<RequestResult<Unit>> Handle(AddLocationCommand request, CancellationToken cancellationToken)
        {
            repository.Add(new Location
            {
                ID = request.ID,
                Name = request.Name,
                TalabatLocationIDs = request.TalabatLocationIDs,
                CompanyID = request.CompanyID,
                CreatedAt = request.CreatedAt,
                CreatedBy = request.CreatedBy,
                UpdatedAt = request.UpdatedAt,
                UpdatedBy = request.UpdatedBy
            });

            return Task.FromResult(RequestResult<Unit>.Success());
        }
    }
}
