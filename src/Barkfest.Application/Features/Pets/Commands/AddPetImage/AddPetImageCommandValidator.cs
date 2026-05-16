using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImage;

public class AddPetImageCommandValidator : AbstractValidator<AddPetImageCommand>
{
    public AddPetImageCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(SupportedImageType.IsAllowedContentType)
            .WithMessage($"Content type is not supported. Allowed types: {string.Join(", ", SupportedImageType.AllowedContentTypes)}.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(SupportedImageType.IsAllowedExtension)
            .WithMessage($"File extension is not supported. Allowed extensions: {string.Join(", ", SupportedImageType.AllowedExtensions)}.");
    }
}
