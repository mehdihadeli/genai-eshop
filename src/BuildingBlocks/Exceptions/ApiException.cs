using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Exceptions;

public class ApiException : CustomException
{
    public ApiException(string message, int statusCode = StatusCodes.Status500InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
