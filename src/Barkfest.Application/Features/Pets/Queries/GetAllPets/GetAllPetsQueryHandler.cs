using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetAllPets;

public class GetAllPetsQueryHandler(IPetRepository petRepository)
    : IRequestHandler<GetAllPetsQuery, IEnumerable<PetDto>>
{
    public async Task<IEnumerable<PetDto>> Handle(GetAllPetsQuery request, CancellationToken cancellationToken)
    {
        var pets = await petRepository.GetAllAsync(cancellationToken);
        return pets.ToDtoList();
    }
}
