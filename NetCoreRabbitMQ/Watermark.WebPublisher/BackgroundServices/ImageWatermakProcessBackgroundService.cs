using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Watermark.WebPublisher.Services;

namespace Watermark.WebPublisher.BackgroundServices
{
    public class ImageWatermakProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermakProcessBackgroundService> _logger;

        private IModel _channel;

        public ImageWatermakProcessBackgroundService(
            ILogger<ImageWatermakProcessBackgroundService> logger,
            RabbitMQClientService rabbitMQClientService,
            IModel channel)
        {
            _logger = logger;
            _rabbitMQClientService = rabbitMQClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();

            _channel.BasicQos(
                prefetchSize: 0, //Mesajın boyutu önemli değil
                prefetchCount: 1, //1er 1er mesajlar gelsin. 5 dersek 5 tane gelir 4 tanesini memory'e alır ve 1-1 işler
                global: false
                );

            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var asyncEventingBasicConsumer = new AsyncEventingBasicConsumer(_channel); //mesajları asenkron okuyacağım.

            //Mesajları okuma işlemi
            _channel.BasicConsume(
                queue: _rabbitMQClientService.QueueName, //Kuyruk ismi
                autoAck: false, //Otomatik silinmesin, resmi başarılı şekilde işlersem haber vericem.
                consumer: asyncEventingBasicConsumer
                );

            asyncEventingBasicConsumer.Received += AsyncEventingBasicConsumer_Received;

            return Task.CompletedTask;
        }

        private Task AsyncEventingBasicConsumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                string message = Encoding.UTF8.GetString(@event.Body.ToArray());
                var vehicleImageCreatedEvent = JsonSerializer.Deserialize<VehicleImageCreatedEvent>(message);

                string addPictureText = "Cihat SOLAK";

                //Resmin Yolu
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", vehicleImageCreatedEvent.ImageName);

                //Resmmi Image class'ına alıyorum.
                using var image = Image.FromFile(imagePath);
                using var graphic = Graphics.FromImage(image);

                Font font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
                SizeF textSize = graphic.MeasureString(addPictureText, font); //Resmin üzerine yazılacak yazı
                Color color = Color.Red;

                var brush = new SolidBrush(color);

                int width = image.Width - ((int)textSize.Width + 30);
                int height = image.Height - ((int)textSize.Height + 30);
                var position = new Point(width, height);

                graphic.DrawString(addPictureText, font, brush, position); //resme yazıyı ekliyorum.

                image.Save("wwwroot/images/watermarks/" + vehicleImageCreatedEvent.ImageName);

                image.Dispose();
                graphic.Dispose();

                //Mesajı işlediğimi bildiriyorum.
                _channel.BasicAck( 
                    deliveryTag: @event.DeliveryTag,
                    multiple: false
                    );
            }
            catch (Exception)
            {
            }

            return Task.CompletedTask;
        }
    }
}
