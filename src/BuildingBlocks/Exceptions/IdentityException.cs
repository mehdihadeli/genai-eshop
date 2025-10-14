using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class IdentityException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    Exception? innerException = null,
    params string[] errors
) : CustomException(message, statusCode, innerException, errors);
