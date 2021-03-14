using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitMQ.Publisher
{
    class Program
    {
        static void Main(string[] args) //Dışardan parametre alıyorum (Powershell'den göndereceğim)
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
                    /* Mesajları direk kuyruğa değil bir exchange'e gönderiyorum.
                     * 
                     * exchange: Exchange İsmi
                     * durable: false yaparsak, rabbitmq instance ımız restart atarsa mesajların hepsi gider. True yaparsak rabbitmq bunu fiziksel diske yazar.
                     * type: Exchange tipi
                     */
                    channel.ExchangeDeclare(
                        exchange: "topic-exchange",
                        durable: true,
                        type: ExchangeType.Topic
                        );

                    for (int index = 0; index < 10; index++) //Örnek olarak 10 adet mesaj göndermek için
                    {
                        string randomRoutingKey = GetRandomRoutingKey(); //rastgele bir routingKey üretiyorum.

                        byte[] bodyByte = Encoding.UTF8.GetBytes($"Log: {randomRoutingKey}"); //mesajlarımızı byte olarak göndermeliyiz.

                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.Persistent = true; //Mesajımızın herhangi bir durumda silinmemesi için

                        /*
                         * exchange: Yukarıda tanımladığım exchange ismini gönder yayınlıyorum.
                         * routingKey: direct-exchange de routeKey verilir. Consumer tarafında da routeKey aynı olan consumer'la aralarında iletişim kurarlar.
                         * body: göndereceğimiz mesaj. (mesajlar her zaman byte türünde olmalıdır.)
                         */
                        channel.BasicPublish( // Kuyruğa mesajı gönderiyoruz.
                            exchange: "topic-exchange",
                            routingKey: randomRoutingKey,
                            basicProperties: properties,
                            body: bodyByte
                            );

                        Console.WriteLine("Log mesajı gönderilmiştir: Message:{0}", randomRoutingKey);
                    }
                }
                Console.WriteLine("Çıkış yapmak için tıklayınız..");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Rastgele bir routingKey üretiyorum.
        /// </summary>
        /// <returns>routing Key</returns>
        static string GetRandomRoutingKey()
        {
            Array logs = Enum.GetValues(typeof(Log));

            Random random = new Random();

            Log log1 = (Log)logs.GetValue(random.Next(logs.Length));
            Log log2 = (Log)logs.GetValue(random.Next(logs.Length));
            Log log3 = (Log)logs.GetValue(random.Next(logs.Length));

            string routingKey = $"{log1}.{log2}.{log3}";

            return routingKey;
        }
    }
}
