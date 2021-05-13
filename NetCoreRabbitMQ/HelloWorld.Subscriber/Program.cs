using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace HelloWorld.Subscriber
{
    class Program
    {
        /// <summary>
        /// Subscriber(Consumer): Kuyruktan mesajları okuyandır.
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

            //Mesajları okuyabilmek için kuyruğa ihtiyacım var. Bu kuyruğu tanımlıyorum. Eğer publisher tarafında kuyruğun tanımlandığına eminsek, burada ister tanımlarız istersek tanımlamayız.
            channel.QueueDeclare(
                queue: "hello-queue", //kuyruk ismi
                durable: true, //false: rabbitmq'da oluşan kuyruklar memoryde tutulur. || true: kuyruklar fiziksel olarak kaydedilir. Rabbitmq restart yerse bile kuyruk kaybolmaz.
                exclusive: false, //true: sadece burda oluşturmuş olduğum kanal üzerinden bağlanabilirim. || false: farklı kanallarda bu kuyruğa bağlanabilsin.
                autoDelete: false //true: bağlı olan son subscriber'da bağlantısını keserse otomatik olarak kuyruğu sil. || false: bağlı bulunan subscriber olmasa bile kuyruk silinmesin.
                );

            var consumer = new EventingBasicConsumer(channel); //Consumer oluşturuyorum

            channel.BasicConsume(
                queue: "hello-queue", //dinleyecek olduğum kuyruk adı
                /// <AutoAck>
                /// AutoAck
                /// TRUE: rabbitmq subscriber'a bir mesaj gönderdiğinde bu mesaj dogruda işlense yanlışda işlense bu mesajı kuyruktan siler. 
                /// FALSE: Sen bunu kuyruktan silme ben gelen mesajı doğru bir şekilde işlersem sana bu konuda haber vereceğim.
                /// </AutoAck>
                autoAck: true,
                consumer: consumer
                );

            //Dinleme işlemi
            consumer.Received += (object sender, BasicDeliverEventArgs eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.Write($"Gelen Mesaj: {message}");
            };

            Console.ReadKey();
        }
    }
}
