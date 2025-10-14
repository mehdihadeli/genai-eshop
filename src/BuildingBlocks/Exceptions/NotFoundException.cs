using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class NotFoundException(string message, Exception? innerException = null)
    : CustomException(message, StatusCodes.Status404NotFound, innerException);

public class NotFoundAppException(string message, Exception? innerException = null)
    : AppException(message, StatusCodes.Status404NotFound, innerException);

public class NotFoundDomainException : DomainException
{
    public NotFoundDomainException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public NotFoundDomainException(Type businessRuleType, string message)
        : base(businessRuleType, message, StatusCodes.Status404NotFound) { }
}
