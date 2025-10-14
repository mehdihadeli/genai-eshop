using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class UnAuthorizedException(string message, Exception? innerException = null)
    : IdentityException(message, StatusCodes.Status401Unauthorized, innerException);
