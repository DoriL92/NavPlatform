using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.Abstractions.Messaging;
using CleanArchitecture.Application.Rewards;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CleanArchitecture.Infrastructure.Messaging;
public sealed class RabbitRewardsBus(IConnection conn) : IRewardsBus
{
     const string Exchange = "rewards"; 
     const string RoutingKey = "recalc"; 

    public Task PublishRecalcAsync(string userId, DateTimeOffset dayUtc, CancellationToken ct)
    {
        using var ch = conn.CreateModel();
        ch.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Topic, durable: true);

        var payload = new RecalcDailyGoalMessage(userId, dayUtc);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        var props = ch.CreateBasicProperties();
        props.DeliveryMode = 2;
        props.ContentType = "application/json";
        props.Type = RoutingKey;

        ch.BasicPublish(exchange: Exchange, routingKey: RoutingKey, basicProperties: props, body: body);
        return Task.CompletedTask;
    }

    public Task PublishAsync(string routingKey, object payload, CancellationToken ct = default)
    {
        using var ch = conn.CreateModel();
        ch.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Topic, durable: true);

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        var props = ch.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        props.MessageId = Guid.NewGuid().ToString("N");
        props.Type = routingKey;
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        ch.BasicPublish(exchange: Exchange, routingKey: routingKey, basicProperties: props, body: body);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string topic, Func<T, CancellationToken, Task> handler, CancellationToken stoppingToken)
    {
        var ch = conn.CreateModel();
        ch.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Topic, durable: true);

        var queueName = $"rewards.{topic}";
        ch.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        ch.QueueBind(queue: queueName, exchange: Exchange, routingKey: topic);

        ch.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(ch);
        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var text = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<T>(text, jsonOpts)!;

                await handler(msg, stoppingToken);
                ch.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch
            {
                ch.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        var tag = ch.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        var tcs = new TaskCompletionSource();
        stoppingToken.Register(() =>
        {
            try { ch.BasicCancel(tag); } catch { }
            try { ch.Close(); } catch { }
            ch.Dispose();
            tcs.TrySetResult();
        });

        return tcs.Task;
    }
}
