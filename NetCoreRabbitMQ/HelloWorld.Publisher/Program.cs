using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace HelloWorld.Publisher
{
    class Program
    {
        /// <summary>
        /// Publisher: Direk kuyruğa veya exchange'e mesaj gönderendir.
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

            //Mesaj gönderebilmek için kuyruğa ihtiyacım var. Bu kuyruğu tanımlıyorum.
            channel.QueueDeclare(
                queue: "hello-queue", //kuyruk ismi
                durable: true, //false: rabbitmq'da oluşan kuyruklar memoryde tutulur. || true: kuyruklar fiziksel olarak kaydedilir. Rabbitmq restart yerse bile kuyruk kaybolmaz.
                exclusive: false, //true: sadece burda oluşturmuş olduğum kanal üzerinden bağlanabilirim. || false: farklı kanallarda bu kuyruğa bağlanabilsin.
                autoDelete: false //true: bağlı olan son subscriber'da bağlantısını keserse otomatik olarak kuyruğu sil. || false: bağlı bulunan subscriber olmasa bile kuyruk silinmesin.
                );

            string message = "Hello World!";
            byte[] messageBody = Encoding.UTF8.GetBytes(message); //Rabbitmq byte[] dizisi kabul eder.

            Enumerable.Range(0, 50).ToList().ForEach(p => //Kuyruga 50 adet mesaj gönder
            {
                channel.BasicPublish(
                exchange: string.Empty, //Exchange kullanmıyorum, direk kuyruğa gönderiyorum
                routingKey: "hello-queue", //Exchange kullanmadığım için yukarıda tanımladığım kuyruğun ismini verdim.(zorunlu)
                basicProperties: null,
                body: messageBody //Gönderilecek mesaj
                );

                Console.WriteLine("Mesaj gönderilmiştir...");
            });
            
            Console.ReadKey();
        }
    }
}
