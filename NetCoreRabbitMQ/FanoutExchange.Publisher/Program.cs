using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace Fanout.FanoutExchange.Publisher
{
    class Program
    {
        /// <summary>
        /// Publisher: Fanout exchange'e mesaj gönderendir.
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
                exchange: "vehicles-fanout", //exchange adı,
                durable: true, //false: exchange memoryde tutulur. || true: exchange fiziksel olarak kaydedilir. Rabbitmq restart yerse bile exchange kaybolmaz.
                type: ExchangeType.Fanout //Exchange tipi
                );

            Enumerable.Range(1, 70).ToList().ForEach(year => //Kuyruga 70 adet mesaj gönder
            {
                string message = string.Concat("Volkswagen: ", year);
                byte[] messageBody = Encoding.UTF8.GetBytes(message); //Rabbitmq byte[] dizisi kabul eder.

                channel.BasicPublish(
                    exchange: "vehicles-fanout", //yukarıda tanımladığım exchange ismini veriyorum
                    routingKey: string.Empty, //routeKey mesajları filtereleme için kullanılır. Bu senaryoda herhangi bir filtreleme yapmayacagım.
                    basicProperties: null,
                    body: messageBody //Gönderilecek mesaj
                    );

                Console.WriteLine($"Araç yılı gönderilmiştir: {message}");
            });

            Console.ReadKey();
        }
    }
}

