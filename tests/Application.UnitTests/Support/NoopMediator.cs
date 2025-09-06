using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Application.UnitTests.Support;
public sealed class NoopMediator : IMediator
{
    public Task Publish(object notification, CancellationToken ct = default) => Task.CompletedTask;
    public Task Publish<T>(T notification, CancellationToken ct = default) where T : INotification => Task.CompletedTask;
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
        => Task.FromResult(default(TResponse)!);
    public Task<object?> Send(object request, CancellationToken ct = default)
        => Task.FromResult<object?>(null);

    IAsyncEnumerable<TResponse> ISender.CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    IAsyncEnumerable<object?> ISender.CreateStream(object request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task ISender.Send<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}