namespace Barkfest.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} with id '{key}' was not found.") { }

    public NotFoundException(string name, string keyName, object key)
        : base($"{name} with {keyName} '{key}' was not found.") { }
}
