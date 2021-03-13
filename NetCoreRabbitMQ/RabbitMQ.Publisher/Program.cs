using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitMQ.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory(); //RabbitMQ'e bağlanmak için
            connectionFactory.Uri = new Uri("amqps://tgvevtgl:0p-IZaJpup2kAp33W3QyTc0liO1Gi4Qf@moose.rmq.cloudamqp.com/tgvevtgl"); //Cloud ortamından altığım url adresi
            //connectionFactory.HostName = "localhost";  Localhost üzerinden bağlanmak istersek.
            
            /* Nedir using?
             * using -> class dan nesne örneği oluşturursanız ve o nesne IDisposable interface'ine sahipse using
             * bloğu sonrası oluşturulan örnek bellekten silinir.
             */

            using (IConnection connection = connectionFactory.CreateConnection()) //Bağlantımı oluşturuyorum.
            {
                using (IModel channel = connection.CreateModel()) //Bağlantımız üzerinden kanalımızı açıyoruz.
                {
                    /*
                     * queue: Kuyruğumuzun ismi
                     * durable: false yaparsak, rabbitmq instance ımız restart atarsa mesajların hepsi gider. True yaparsak rabbitmq bunu fiziksel diske yazar. rabbitmq yeniden başlasada mesajlar kaybolmaz.
                     * exclusive: bu kuyruğa sadece 1 tane mi kanal bağlansın yoksa başka kanallarda bağlanabilsin mi? false: diğer kanallarda bağlansın anlamına gelir.
                     * autoDelete: bir kuyrukta diyelim ki 20 tane mesaj var, eğer son mesajda kuyruktan çıkarsa yani kuyrukta mesaj kalmazsa bu kuyruk silinsin mi?
                     */
                    channel.QueueDeclare(
                         queue: "rabbitMqKuyruk",
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null
                         ); //Kuyruk oluşturalım.

                    string message = "RabbitMQ Deneme Mesaj";

                    for (int index = 0; index < 10; index++) //Örnek olarak 10 adet mesaj göndermek için
                    {
                        byte[] bodyByte = Encoding.UTF8.GetBytes($"No:{index} Message:{message}"); //mesajlarımızı byte olarak göndermeliyiz.
                        
                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.Persistent = true; //Mesajımızın herhangi bir durumda silinmemesi için

                        /*
                         * exchange: boş bırakırsanız default exchange anlamına geliyor.
                         * routingKey: default exchange kullanıyorsanız routing key, kuyruk isminiz ile aynı olmalıdır. (Kuyruk ile mesajı birbirine bağlıyoruz.)
                         * body: göndereceğimiz mesaj. (mesajlar her zaman byte türünde olmalıdır.)
                         */
                        channel.BasicPublish( // Kuyruğa mesajı gönderiyoruz.
                            exchange: string.Empty,
                            routingKey: "rabbitMqKuyruk",
                            basicProperties: properties,
                            body: bodyByte
                            );

                        Console.WriteLine("Mesajınız kuyruğa gönderilmiştir.");
                    }
                }

                Console.ReadKey();
            }

        }
    }
}
