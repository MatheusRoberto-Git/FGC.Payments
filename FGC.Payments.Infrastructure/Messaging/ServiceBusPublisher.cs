using Azure.Messaging.ServiceBus;
using FGC.Payments.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FGC.Payments.Infrastructure.Messaging
{
    public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusPublisher> _logger;

        public ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
        {
            _logger = logger;

            var connectionString = configuration["ServiceBus:ConnectionString"];
            var queueName = configuration["ServiceBus:QueueName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("ServiceBus:ConnectionString não configurada");

            if (string.IsNullOrEmpty(queueName))
                throw new InvalidOperationException("ServiceBus:QueueName não configurada");

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(queueName);
        }

        public async Task PublishPaymentProcessedAsync(PaymentProcessedMessage message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(jsonMessage)
                {
                    ContentType = "application/json",
                    Subject = "PaymentProcessed",
                    MessageId = message.PaymentId.ToString()
                };

                await _sender.SendMessageAsync(serviceBusMessage);

                _logger.LogInformation(
                    "✅ Mensagem publicada no Service Bus. PaymentId: {PaymentId}, Status: {Status}",
                    message.PaymentId,
                    message.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Falha ao publicar mensagem. PaymentId: {PaymentId}",
                    message.PaymentId);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}