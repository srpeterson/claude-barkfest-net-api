namespace Barkfest.Domain.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Access denied.") { }
}
