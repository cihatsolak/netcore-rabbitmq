using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Watermark.WebPublisher.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(VehicleImageCreatedEvent productImageCreatedEvent)
        {
            IModel channel = _rabbitMQClientService.Connect();

            string message = JsonSerializer.Serialize(productImageCreatedEvent);
            byte[] bodyByteMessage = Encoding.UTF8.GetBytes(message);

            IBasicProperties basicProperties = channel.CreateBasicProperties();  //Mesajım rabbitmq memory'de değil fiziksel diskde tutulsun
            basicProperties.Persistent = true; //Mesajım rabbitmq memory'de değil fiziksel diskde tutulsun

            channel.BasicPublish(
                exchange: _rabbitMQClientService.ExchangeName, //Exchange adı
                routingKey: _rabbitMQClientService.RoutingWatermark, //RouteKey
                basicProperties: basicProperties,
                body: bodyByteMessage
                );
        }
    }
}
