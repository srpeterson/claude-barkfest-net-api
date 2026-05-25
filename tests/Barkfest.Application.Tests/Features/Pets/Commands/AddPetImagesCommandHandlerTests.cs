using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.AddPetImages;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class AddPetImagesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IBlobStorageService _blobStorageService = Substitute.For<IBlobStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IContentModerationService _contentModerationService = Substitute.For<IContentModerationService>();
    private readonly AddPetImagesCommandHandler _addPetImagesCommandHandler;

    public AddPetImagesCommandHandlerTests()
    {
        _contentModerationService
            .IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _addPetImagesCommandHandler = new AddPetImagesCommandHandler(
            _petRepository, _blobStorageService, _unitOfWork, _currentUserService, _contentModerationService);
    }

    // -----------------------------------------------------------------------
    // Guard clauses
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        _petRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            _addPetImagesCommandHandler.Handle(
                BuildCommand(Guid.NewGuid(), ["photo.jpg"]), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await Should.ThrowAsync<ForbiddenException>(() =>
            _addPetImagesCommandHandler.Handle(
                BuildCommand(pet.Id, ["photo.jpg"]), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_SubmittedCountExceedsAvailableSlots_Throws_DomainException()
    {
        var pet = new PetBuilder().Build();
        for (var i = 0; i < Pet.MaxImages - 1; i++)
            pet.AddImage(new PetImageBuilder().WithDisplayOrder(i).Build());

        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        // 1 slot remaining, submitting 2
        await Should.ThrowAsync<DomainException>(() =>
            _addPetImagesCommandHandler.Handle(
                BuildCommand(pet.Id, ["photo1.jpg", "photo2.jpg"]), CancellationToken.None));
    }

    // -----------------------------------------------------------------------
    // Success
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_AllImagesPassModeration_Returns_AllSuccessResults()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        var result = await _addPetImagesCommandHandler.Handle(
            BuildCommand(pet.Id, ["photo1.jpg", "photo2.jpg"]), CancellationToken.None);

        result.Results.Count.ShouldBe(2);
        result.Results.ShouldAllBe(r => r.Success);
        result.Results.ShouldAllBe(r => r.ImageId.HasValue);
    }

    [Fact]
    public async Task Handle_When_AllImagesPassModeration_Uploads_AllBlobs()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await _addPetImagesCommandHandler.Handle(
            BuildCommand(pet.Id, ["photo1.jpg", "photo2.png"]), CancellationToken.None);

        await _blobStorageService.Received(2).UploadAsync(
            "pet-images",
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<string>(),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_AllImagesPassModeration_SavesOnce()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await _addPetImagesCommandHandler.Handle(
            BuildCommand(pet.Id, ["photo1.jpg", "photo2.jpg"]), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    // -----------------------------------------------------------------------
    // Partial / moderation failure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_OneImageFailsModeration_Returns_PartialResults()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        var failStream = new MemoryStream([0x00]);
        _contentModerationService.IsImageSafeAsync(failStream, CancellationToken.None).Returns(false);

        var uploads = new List<PetImageUpload>
        {
            new("photo1.jpg", new MemoryStream([0xFF, 0xD8]), "image/jpeg", 1024),
            new("bad.jpg", failStream, "image/jpeg", 1024)
        };
        var command = new AddPetImagesCommand(pet.Id, uploads);

        var result = await _addPetImagesCommandHandler.Handle(command, CancellationToken.None);

        result.Results.Count(r => r.Success).ShouldBe(1);
        result.Results.Count(r => !r.Success).ShouldBe(1);
        result.Results.First(r => !r.Success).FailureReason.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_When_AllImagesFailModeration_DoesNotSave()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);
        _contentModerationService
            .IsImageSafeAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await _addPetImagesCommandHandler.Handle(
            BuildCommand(pet.Id, ["bad.jpg"]), CancellationToken.None);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -----------------------------------------------------------------------
    // Auto-feature
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_PetHasNoImages_FirstUploadedImage_IsAutoFeatured()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        var result = await _addPetImagesCommandHandler.Handle(
            BuildCommand(pet.Id, ["photo1.jpg", "photo2.jpg"]), CancellationToken.None);

        var firstImageId = result.Results[0].ImageId!.Value;
        pet.FeaturedImage!.Id.ShouldBe(firstImageId);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AddPetImagesCommand BuildCommand(Guid petId, IEnumerable<string> fileNames) =>
        new(petId, fileNames
            .Select(n => new PetImageUpload(n, new MemoryStream([0xFF, 0xD8]), "image/jpeg", 1024))
            .ToList());
}
