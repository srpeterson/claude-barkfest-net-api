using Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class BatchDeletePetImagesCommandValidatorTests
{
    private readonly BatchDeletePetImagesCommandValidator _batchDeletePetImagesCommandValidator = new();

    [Fact]
    public void Validate_When_ImageIdsIsEmpty_Fails_ForImageIds()
    {
        var command = new BatchDeletePetImagesCommand(Guid.NewGuid(), []);

        var result = _batchDeletePetImagesCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(BatchDeletePetImagesCommand.ImageIds));
    }

    [Fact]
    public void Validate_When_ImageIdsHasEntries_Passes()
    {
        var command = new BatchDeletePetImagesCommand(Guid.NewGuid(), [Guid.NewGuid(), Guid.NewGuid()]);

        var result = _batchDeletePetImagesCommandValidator.Validate(command);

        result.IsValid.ShouldBeTrue();
    }
}
