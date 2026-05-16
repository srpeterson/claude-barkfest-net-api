using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.AddPetImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class AddPetImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AddPetImageCommandHandler _sut;

    public AddPetImageCommandHandlerTests()
    {
        _sut = new AddPetImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNonEmptyGuid()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet(petId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new AddPetImageCommand(petId, "gallery.jpg", content, "image/jpeg");

        var result = await _sut.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ValidCommand_UploadsBlobAndAddsImageToPet()
    {
        var petId = Guid.NewGuid();
        var pet = BuildPet(petId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new AddPetImageCommand(petId, "gallery.png", content, "image/png");

        await _sut.Handle(command, CancellationToken.None);

        await _blobStorageService.Received(1).UploadAsync(
            "pet-images",
            Arg.Is<string>(b => b.StartsWith($"pets/{petId}/gallery/")),
            content,
            "image/png",
            CancellationToken.None);
        pet.Images.Count.ShouldBe(1);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_PetNotFound_ThrowsNotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var command = new AddPetImageCommand(petId, "gallery.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    private static Pet BuildPet(Guid petId)
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetName("Buddy");
        pet.SetPetType(Barkfest.Domain.Enums.PetType.Dog);
        return pet;
    }
}
