using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;

public class BatchDeletePetImagesCommandValidator : AbstractValidator<BatchDeletePetImagesCommand>
{
    public BatchDeletePetImagesCommandValidator()
    {
        RuleFor(x => x.ImageIds)
            .NotEmpty().WithMessage("At least one image ID is required.");
    }
}
