using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.RemovePetImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class RemovePetImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemovePetImageCommandHandler _sut;

    public RemovePetImageCommandHandlerTests()
    {
        _sut = new RemovePetImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_When_PetAndImageExist_Deletes_BlobAndRemovesFromPet()
    {
        var petId = Guid.NewGuid();
        var petImage = new PetImageBuilder().Build();
        var pet = new PetBuilder().WithImage(petImage).Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new RemovePetImageCommand(petId, petImage.Id);

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).DeleteAsync(
            "pet-images", petImage.BlobName, CancellationToken.None);
        pet.Images.ShouldBeEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _sut.Handle(new RemovePetImageCommand(petId, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_ImageNotFoundOnPet_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new RemovePetImageCommand(petId, Guid.NewGuid());

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

}
