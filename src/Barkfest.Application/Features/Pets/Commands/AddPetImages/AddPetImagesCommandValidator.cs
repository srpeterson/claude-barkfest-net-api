using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImages;

public class AddPetImagesCommandValidator : AbstractValidator<AddPetImagesCommand>
{
    public AddPetImagesCommandValidator()
    {
        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one image is required.");

        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(SupportedImageType.IsContentTypeSupported)
                .WithMessage(
                    $"Content type is not supported. Allowed types: {string.Join(", ", SupportedImageType.AllowedContentTypes)}.");

            image.RuleFor(x => x.FileName)
                .NotEmpty()
                .Must(SupportedImageType.IsFileExtensionSupported)
                .WithMessage(
                    $"File extension is not supported. Allowed extensions: {string.Join(", ", SupportedImageType.AllowedExtensions)}.");
        });
    }
}
