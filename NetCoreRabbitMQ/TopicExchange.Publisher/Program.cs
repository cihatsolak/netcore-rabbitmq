using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace TopicExchange.Publisher
{
    /// <summary>
    /// Her bir mesaj gönderiminde farklı farklı routeKey oluşturacağım
    /// </summary>
    public enum Model
    {
        Polo = 1,
        Golf = 2,
        Passat = 3,
        Tiguan = 4
    }

    class Program
    {
        /// <summary>
        /// Publisher: Topic exchange'e mesaj gönderendir.
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
                exchange: "vehicles-topic", //exchange adı
                durable: true, //false: exchange memoryde tutulur. || true: exchange fiziksel olarak kaydedilir. Rabbitmq restart yerse bile exchange kaybolmaz.
                type: ExchangeType.Topic //Exchange tipi
                );

            var random = new Random();

            Enumerable.Range(1, 50).ToList().ForEach(year => //Kuyruga 50 adet mesaj gönder
            {
                Model modelName1 = (Model)random.Next(1, 4);
                Model modelName2 = (Model)random.Next(1, 4);
                Model modelName3 = (Model)random.Next(1, 4);

                string routeKey = string.Concat(modelName1, ".", modelName2, ".", modelName3);

                string message = string.Concat("Volkswagen Modeli: ", $"{modelName1}-{modelName2}-{modelName3}");
                byte[] messageBody = Encoding.UTF8.GetBytes(message); //Rabbitmq byte[] dizisi kabul eder.

                channel.BasicPublish(
                    exchange: "vehicles-topic", //yukarıda tanımladığım exchange ismini veriyorum
                    routingKey: routeKey, //Direct exchange'e gelen mesajlar belirlediğim key değerlerine göre kuyruklara dağıtılsın.
                    basicProperties: null,
                    body: messageBody //Gönderilecek mesaj
                    );

                Console.WriteLine($"Araç modeli gönderilmiştir: {message}");
            });

            Console.ReadKey();
        }


    }
}
