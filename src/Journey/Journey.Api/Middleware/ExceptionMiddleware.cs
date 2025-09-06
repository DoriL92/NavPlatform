using System.Net;
using System.Text;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using Journey.Api.Models;

namespace Journey.Api.Middleware;

public class ExceptionMiddleware
{

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;


    

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICurrentUser currentUser)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext), nameof(currentUser));

        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error (UserId: {UserId})", currentUser.UserId);

            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        string resposeMessage;

        switch (exception)
        {


            case ForbiddenAccessException:
                resposeMessage = exception.ToString();
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;

            case NotFoundException:
                var ex = (NotFoundException)exception;
                resposeMessage = $"{ex.Message}";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ValidationException:

                var exp = (ValidationException)exception;
                resposeMessage = $"{exp}";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case BadRequestException:

                resposeMessage = $"{exception.Message}";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            default:
                resposeMessage = exception.ToString();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var bytes = Encoding.UTF8.GetBytes(new ResponseNotOKSchema()
        {
            ErrorCode = context.Response.StatusCode.ToString(),
            Message = resposeMessage
        }.ToString());

        await context.Response.Body.WriteAsync(bytes.AsMemory(0, bytes.Length));


    }

}