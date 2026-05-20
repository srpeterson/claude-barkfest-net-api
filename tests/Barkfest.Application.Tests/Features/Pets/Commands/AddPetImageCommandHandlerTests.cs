using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.AddPetImage;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class AddPetImageCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IContentModerationService _contentModerationService = Substitute.For<IContentModerationService>();
    private readonly AddPetImageCommandHandler _addPetImageCommandHandler;

    public AddPetImageCommandHandlerTests()
    {
        _contentModerationService.IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(true);
        _addPetImageCommandHandler = new AddPetImageCommandHandler(_petRepository, _blobStorageService, _unitOfWork, _currentUserService, _contentModerationService);
    }

    [Fact]
    public async Task Handle_When_ImageFailsModeration_Throws_DomainException()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        _contentModerationService.IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddPetImageCommand(petId, "gallery.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<DomainException>(() => _addPetImageCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Returns_ValidGuid()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new AddPetImageCommand(petId, "gallery.jpg", content, "image/jpeg");

        var result = await _addPetImageCommandHandler.Handle(command, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_When_CommandIsValid_Uploads_BlobAndAddsImageToPet()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);
        var content = new MemoryStream([0x89, 0x50]);

        var command = new AddPetImageCommand(petId, "gallery.png", content, "image/png");

        await _addPetImageCommandHandler.Handle(command, CancellationToken.None);

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
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        var petId = Guid.NewGuid();
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns((Pet?)null);

        var command = new AddPetImageCommand(petId, "gallery.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<NotFoundException>(() => _addPetImageCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var petId = Guid.NewGuid();
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(petId, CancellationToken.None).Returns(pet);

        var command = new AddPetImageCommand(petId, "gallery.jpg", Stream.Null, "image/jpeg");

        await Should.ThrowAsync<ForbiddenException>(() => _addPetImageCommandHandler.Handle(command, CancellationToken.None));
    }
}
