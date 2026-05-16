using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid Id) : IRequest<PetDto>;

public class GetPetByIdQueryHandler(IPetRepository petRepository)
    : IRequestHandler<GetPetByIdQuery, PetDto>
{
    public async Task<PetDto> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        return pet.ToDto();
    }
}
