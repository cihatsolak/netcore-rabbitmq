using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using WebConsumer.Models;

namespace WebConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri("amqps://tgvevtgl:0p-IZaJpup2kAp33W3QyTc0liO1Gi4Qf@moose.rmq.cloudamqp.com/tgvevtgl")
            };

            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: "converter-exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
                );

            channel.QueueBind(
                queue: "file",
                exchange: "converter-exchange",
                routingKey: "wordtopdf"
                );

            //Mesajlar bana 1-1 gelecek, aynı anda 2 mesaj gelmeyecek
            channel.BasicQos(
                prefetchSize:0,
                prefetchCount: 1,
                global: false
                );

            var eventingBasicConsumer = new EventingBasicConsumer(channel);

            /* Artık tüketme işlemini gerçekleştircem.
             * queue: kuyruk ismi,
             * autoAck: false -> otomatik bir mesaj alındıgı zaman o mesaj düşsün mü? hayır ben kendim göndericem. Örneğin eposta gönderemezsem mesaj düşmesin çünkü ben e-posta göndermemeişim. illa ki doğru olarak işlemem lazım.
             */
            channel.BasicConsume(
                queue: "file",
                autoAck: false,
                consumer: eventingBasicConsumer
                );


            eventingBasicConsumer.Received += (model, basicDeliverEventArgs) =>
            {
                byte[] bodyByte = basicDeliverEventArgs.Body.ToArray();
                string serilazedModel = Encoding.UTF8.GetString(bodyByte);
                MessageBodyParameterModel messageBodyParameterModel = JsonSerializer.Deserialize<MessageBodyParameterModel>(serilazedModel);

                MemoryStream stream = Utilities.ConvertWordToPdf(messageBodyParameterModel);

                bool isSuccess = Utilities.EmailSender(
                    email: messageBodyParameterModel.Email,
                    memoryStream: stream,
                    fileName: messageBodyParameterModel.FileName
                    );

                if (isSuccess)
                {
                    Console.WriteLine("Kuyruktan mesaj başarıyla işlendi");

                    //Mesajı başarıyla işlediğimi ve sıradaki işi istediğimi bildiriyorum. (Dönüştürmede hata alırsa bu kod çalışmayacağı için kuyruktan mesaj silinmeyecek.)
                    channel.BasicAck(
                        deliveryTag: basicDeliverEventArgs.DeliveryTag,
                        multiple: false //hepsi için başarılı değil, elimdeki iş için başarılı
                        );
                }
            };

            Console.WriteLine("Çıkış yapmak tıklayınız..");
            Console.ReadLine();
        }
    }
}
