namespace Journey.Api.Models;

public sealed record ShareByEmailRequest(string[] Emails,string? ShareMessage);
