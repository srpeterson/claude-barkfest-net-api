namespace Barkfest.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? OwnerId { get; }
    Guid? AdminId { get; }
    bool IsAdmin { get; }
}
