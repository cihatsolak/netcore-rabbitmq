using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace HeaderExchange.Subscriber
{
    class Program
    {
        /// <summary>
        /// Subscriber(Consumer): Kuyruktan mesajları işleyendir. 
        /// Publisher tarafında kuyruk tanımlanmadı. Subscriber ihtiyacı olan header dictionary'e göre exchange'den mesajları alacak.
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

            //Kanalın gönderim özelliklerini belirliyorum
            channel.BasicQos(
                prefetchSize: 0, //Herhangi bir boyuttaki mesajı gönderebilirsin
                prefetchCount: 1, // Mesajları 1-1 işleyeceğim. 2 verirsem 2'şer 2şer alım yapar.
                /// <global>
                /// TRUE: prefetchCount=6 dersem ve 3 subscriberım varsa, 6/3=2 yani her bir subcriber'a 2 adet mesajı tek seferde iletir. Diğer bir deyişle her mesaj gönderiminde toplamda 6 adet mesaj consumerlar arasında paylaştırılır.
                /// FALSE: prefetchCount=6 dersem ve 3 subscriberım varsa, her seferinde her bir subscriber'a 6 mesaj göndermeye çalışır.  
                /// </global>
                global: false
                );

            string randomQueueName = channel.QueueDeclare().QueueName;

            // Random isme sahip bir kuyruk bind ediyorum. Declare etmememin sebebi ise işim bittiğinde kuyruk silinsin.
            // Yeni bir kuyruk oluşturmuyorum(declare etmiyorum). Çünkü subscriber kuyrukla işi bittiğinde kuyruk silinsin.
            // Uygulama her ayağa kalktıgında random name e sahip kuyruk oluşacak, uygulama kapandığında ise kuyruk silinecek.
            // channel.QueueDeclare() declare etseydim uygulama kapandığındanda dahi bu kuyruk silinmez.
            // QueueBind -> Geçiçi kuyruk
            channel.QueueBind(
                queue: randomQueueName,
                exchange: "vehicles-header", //Publisherda oluşturulan exchange ismi
                routingKey: string.Empty, //header exchange routKey değil dictionar<string,object>'e göre filtreleme yapar,
                arguments: new Dictionary<string, object> //belirlediğim kriter
                {
                    { "brand", "volkswagen" },
                    { "color", "red" },
                    { "x-match", "all" }
                }
            );

            var consumer = new EventingBasicConsumer(channel); //Consumer oluşturuyorum

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

            //Dinleme işlemi
            consumer.Received += (object sender, BasicDeliverEventArgs eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                Console.WriteLine($"Gelen Mesaj =  {message}");

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
