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
                        exchange: "direct-exchange",
                        durable: true,
                        type: ExchangeType.Direct
                        );

                    

                    for (int index = 0; index < 10; index++) //Örnek olarak 10 adet mesaj göndermek için
                    {
                        Log log = GetRandomLog(); //log'un türünü random olarak ayarlıyorum.

                        byte[] bodyByte = Encoding.UTF8.GetBytes($"Log: {log.ToString()}"); //mesajlarımızı byte olarak göndermeliyiz.

                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.Persistent = true; //Mesajımızın herhangi bir durumda silinmemesi için

                        /*
                         * exchange: Yukarıda tanımladığım exchange ismini gönder yayınlıyorum.
                         * routingKey: direct-exchange de routeKey verilir. Consumer tarafında da routeKey aynı olan consumer'la aralarında iletişim kurarlar.
                         * body: göndereceğimiz mesaj. (mesajlar her zaman byte türünde olmalıdır.)
                         */
                        channel.BasicPublish( // Kuyruğa mesajı gönderiyoruz.
                            exchange: "direct-exchange",
                            routingKey: log.ToString(),
                            basicProperties: properties,
                            body: bodyByte
                            );

                        Console.WriteLine("Log mesajı gönderilmiştir: Message:{0}", log.ToString());
                    }
                }
                Console.WriteLine("Çıkış yapmak için tıklayınız..");
                Console.ReadKey();
            }
        }

        static Log GetRandomLog()
        {
            Array logNames = Enum.GetValues(typeof(Log));
            int random = new Random().Next(logNames.Length);

           return (Log)logNames.GetValue(random);
        }
    }
}
