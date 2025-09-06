using System.Text.Json;

namespace CleanArchitecture.Application.Common.Exceptions;
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }

    public override string ToString()
    {
        return JsonSerializer.Serialize(new { errorCode = 400, message = Message });
    }
}
