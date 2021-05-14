using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace DirectExchange.Publisher
{
    /// <summary>
    /// Her bir model için farklı bir kuyruk oluşturacağım
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
        /// Publisher: Direct exchange'e mesaj gönderendir.
        /// Bu örnekde kuyruğu burada yani publisher'da oluşturuyorum. İlgili kuyruk ismine göre subscriber mesajları alabilir.
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
                exchange: "vehicles-direct", //exchange adı
                durable: true, //false: exchange memoryde tutulur. || true: exchange fiziksel olarak kaydedilir. Rabbitmq restart yerse bile exchange kaybolmaz.
                type: ExchangeType.Direct //Exchange tipi
                );

            /// <summary>
            /// Her bir enum modeli için farklı bir kuyruk oluşturacağım
            /// </summary>
            Enum.GetNames(typeof(Model)).ToList().ForEach(modelName =>
            {
                string queueName = string.Concat("direct-queue-", modelName);

                //Mesajı kuyruğa gönderebilmek için kuyruğa ihtiyacım var. Bu kuyruğu tanımlıyorum.
                channel.QueueDeclare(
                    queue: queueName, //model ismine göre kuyruk ismi
                    durable: true, //false: rabbitmq'da oluşan kuyruklar memoryde tutulur. || true: kuyruklar fiziksel olarak kaydedilir. Rabbitmq restart yerse bile kuyruk kaybolmaz.
                    exclusive: false, //true: sadece burda oluşturmuş olduğum kanal üzerinden bağlanabilirim. || false: farklı kanallarda bu kuyruğa bağlanabilsin.
                    autoDelete: false //true: bağlı olan son subscriber'da bağlantısını keserse otomatik olarak kuyruğu sil. || false: bağlı bulunan subscriber olmasa bile kuyruk silinmesin.
                    );

                string routeKey = $"route-{modelName}";

                //Oluşturduğum kuyruğa exchange'e ekliyorum.
                channel.QueueBind(
                     queue: queueName, //model ismine göre kuyruk ismi
                     exchange: "vehicles-direct", //exchange adı
                     routingKey: routeKey //routeKey'e göre filtreleme yapacağım
                    );
            });

            Enumerable.Range(1, 30).ToList().ForEach(year => //Kuyruga 30 adet mesaj gönder
            {
                Model modelName = (Model)new Random().Next(1, 5);
                string routeKey = $"route-{modelName}";

                string message = string.Concat("Volkswagen Modeli: ", modelName);
                byte[] messageBody = Encoding.UTF8.GetBytes(message); //Rabbitmq byte[] dizisi kabul eder.

                channel.BasicPublish(
                    exchange: "vehicles-direct", //yukarıda tanımladığım exchange ismini veriyorum
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
