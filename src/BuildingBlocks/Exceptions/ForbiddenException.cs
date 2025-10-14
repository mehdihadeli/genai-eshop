using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class ForbiddenException(string message, Exception? innerException = null)
    : IdentityException(message, statusCode: StatusCodes.Status403Forbidden, innerException);
