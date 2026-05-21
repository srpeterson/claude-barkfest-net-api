using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class BatchDeletePetImagesCommandHandlerTests
{
    private readonly IPetRepository _petRepository = Substitute.For<IPetRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly BatchDeletePetImagesCommandHandler _batchDeletePetImagesCommandHandler;

    public BatchDeletePetImagesCommandHandlerTests()
    {
        _batchDeletePetImagesCommandHandler = new BatchDeletePetImagesCommandHandler(
            _petRepository, _unitOfWork, _currentUserService);
    }

    [Fact]
    public async Task Handle_When_PetNotFound_Throws_NotFoundException()
    {
        _petRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Pet?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            _batchDeletePetImagesCommandHandler.Handle(
                new BatchDeletePetImagesCommand(Guid.NewGuid(), [Guid.NewGuid()]),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PetBelongsToAnotherOwner_Throws_ForbiddenException()
    {
        var pet = new PetBuilder().Build();
        _currentUserService.OwnerId.Returns((Guid?)Guid.NewGuid());
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await Should.ThrowAsync<ForbiddenException>(() =>
            _batchDeletePetImagesCommandHandler.Handle(
                new BatchDeletePetImagesCommand(pet.Id, [Guid.NewGuid()]),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_AllImagesExist_Removes_AllAndSaves()
    {
        var pet = new PetBuilder().Build();
        var image1 = new PetImageBuilder().WithDisplayOrder(0).Build();
        var image2 = new PetImageBuilder().WithDisplayOrder(1).Build();
        var image3 = new PetImageBuilder().WithDisplayOrder(2).Build();
        pet.AddImage(image1);
        pet.AddImage(image2);
        pet.AddImage(image3);
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await _batchDeletePetImagesCommandHandler.Handle(
            new BatchDeletePetImagesCommand(pet.Id, [image1.Id, image2.Id]),
            CancellationToken.None);

        pet.Images.Count.ShouldBe(1);
        pet.Images.ShouldContain(i => i.Id == image3.Id);
        await _unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_When_AnyImageNotFound_Throws_DomainException()
    {
        var pet = new PetBuilder().Build();
        var image = new PetImageBuilder().Build();
        pet.AddImage(image);
        _currentUserService.OwnerId.Returns((Guid?)pet.OwnerId);
        _petRepository.GetByIdAsync(pet.Id, CancellationToken.None).Returns(pet);

        await Should.ThrowAsync<DomainException>(() =>
            _batchDeletePetImagesCommandHandler.Handle(
                new BatchDeletePetImagesCommand(pet.Id, [image.Id, Guid.NewGuid()]),
                CancellationToken.None));
    }
}
