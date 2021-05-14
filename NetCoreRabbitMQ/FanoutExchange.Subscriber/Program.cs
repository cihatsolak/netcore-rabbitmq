using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Fanout.FanoutExchange.Subscriber
{
    class Program
    {
        /// <summary>
        /// Subscriber(Consumer): Kuyruktan mesajları işleyendir.
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

            //Exchange oluşturuyorum. Eğer publisher tarafında bu exchange oluşturulduğuna eminsek, burada oluşturmayadabiliriz.
            //Eğer hem publisher'da hemde consumer tarafında aynı exchange'i oluşturacaksak parametreleri birebir aynı olmalıdır.
            channel.ExchangeDeclare(
                exchange: "vehicles-fanout", //exchange adı,
                durable: true, //false: exchange memoryde tutulur. || true: exchange fiziksel olarak kaydedilir. Rabbitmq restart yerse bile exchange kaybolmaz.
                type: ExchangeType.Fanout //Exchange tipi
                );

            //Kuyruk isimlerini random veriyorum ki, 3 adet consumer ayağı kaldırdığımda her bir consumer'ın kendisine ait bir kuyruğu olsun.
            var randomQueueName = channel.QueueDeclare().QueueName;

            // Yeni bir kuyruk oluşturmuyorum(declare etmiyorum). Çünkü subscriber kuyrukla işi bittiğinde kuyruk silinsin.
            // Uygulama her ayağa kalktıgında random name e sahip kuyruk oluşacak, uygulama kapandığında ise kuyruk silinecek.
            // channel.QueueDeclare() declare etseydim uygulama kapandığındanda dahi bu kuyruk silinmez.
            // QueueBind -> Geçiçi kuyruk
            channel.QueueBind(
                queue: randomQueueName,
                exchange: "vehicles-fanout", //Yukarıdaki exchange ismi
                routingKey: string.Empty //routeKey mesajları filtereleme için kullanılır. Bu senaryoda herhangi bir filtreleme yapmayacagım
                );

            var consumer = new EventingBasicConsumer(channel); //Consumer oluşturuyorum

            //Kuyruğun gönderim özelliklerini belirliyorum
            channel.BasicQos(
                prefetchSize: 0, //Herhangi bir boyuttaki mesajı gönderebilirsin
                prefetchCount: 1, // Mesajları 1-1 işleyeceğim. 2 verirsem 2'şer 2şer alım yapar.
                /// <global>
                /// TRUE: prefetchCount=6 dersem ve 3 subscriberım varsa, 6/3=2 yani her bir subcriber'a 2 adet mesajı tek seferde iletir.
                /// FALSE: prefetchCount=6 dersem ve 3 subscriberım varsa, her bir subscriber'a 6 mesaj göndermeye çalışır. 3x6 = 18 mesaj gibi. 
                /// </global>
                global: false
                );

            //Subscriber(Consumer) mesaj işleme özellikleri
            channel.BasicConsume(
                queue: randomQueueName, //dinleyecek olduğum kuyruk adı
                /// <AutoAck>
                /// TRUE: rabbitmq subscriber'a bir mesaj gönderdiğinde bu mesaj dogruda işlense yanlışda işlense bu mesajı kuyruktan siler. 
                /// FALSE: Sen bunu kuyruktan silme ben gelen mesajı doğru bir şekilde işlersem sana bu konuda haber vereceğim.
                /// </AutoAck>
                autoAck: false,
                consumer: consumer
                );

            Console.WriteLine("Araç yıllarını bekliyorum..");

            //Dinleme işlemi
            consumer.Received += (object sender, BasicDeliverEventArgs eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine($"Gelen Araç Bilgisi =  {message}");

                //Mesajı doğru işlediğimi bildiriyorum
                channel.BasicAck(
                    deliveryTag: eventArgs.DeliveryTag, //teslim bildirim tag'i
                    /// <multiple>
                    // TRUE: O anda memory'de işlenmiş ama rabbitmq'e bildirilmemiş işlemleride rabbitmq'ya haberdar eder.
                    // FALSE: Sadece ilgili mesajın durumunu rabbitmq'ya bildir.
                    /// </multiple>
                    multiple: false
                    );
            };

            Console.ReadKey();
        }
    }
}
