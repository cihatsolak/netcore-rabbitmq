using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeaderExchange.Publisher
{
    class Program
    {
        /// <summary>
        /// Publisher: Header exchange'e mesaj gönderendir.
        /// Bu örnekde direk kuyruk oluşturmayıp exchange oluşturuyorum ve mesajları exchange gönderiyorum. Kuyruğu subscriber tarafında oluşturacağım.(performans)
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri("amqps://wgpkwzxn:m2DXQ3IKLkxBTvLTRIdwZdNk-PPjvpzi@baboon.rmq.cloudamqp.com/wgpkwzxn")
            }; //RabbitMQ bağlantısı özellikleri

            using IConnection connection = connectionFactory.CreateConnection(); //Bağlantı açalım.
            using IModel channel = connection.CreateModel(); //Açılan bağlantıya bir kanal oluşturuyorum. Rabbitmq ya bu kanal üzerinden ulaşacağız.

            //Exchange oluşturuyorum
            channel.ExchangeDeclare(
                exchange: "vehicles-header", //exchange adı
                durable: true, //false: exchange memoryde tutulur. || true: exchange fiziksel olarak kaydedilir. Rabbitmq restart yerse bile exchange kaybolmaz.
                type: ExchangeType.Headers //Exchange tipi
                );

            //Bir nevi routeKey
            var header = new Dictionary<string, object>
            {
                { "brand", "volkswagen" },
                { "color", "red" }
            };

            //Yukarıda oluştuduğum header'ı ekliyorum.
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.Headers = header;
            basicProperties.Persistent = true; //rabbitmq restart atarsa mesajlarım saklansın silinmesin.

            channel.BasicPublish(
                  exchange: "vehicles-header", //yukarıda tanımladığım exchange ismini veriyorum
                  routingKey: string.Empty, //header exchange routeKey kullanmaz, header da dictionary gönderir.
                  basicProperties: basicProperties,
                  body: Encoding.UTF8.GetBytes("Volkswagen Kırmızı Golf 1.6") //Gönderilecek mesaj
                  );

            Console.ReadKey();
        }
    }
}
