using RabbitMQ.Client;
using System;
using System.Collections.Generic;
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
                        exchange: "header-exchange",
                        durable: true,
                        type: ExchangeType.Headers
                        );

                    Dictionary<string, object> headers = new Dictionary<string, object>();
                    headers.Add("format", ".pdf");
                    headers.Add("shape", "a4");
                    headers.Add("x-match", "all"); //consumer tarafında header birebir uyuşsun.
                    //headers.Add("x-match", "any"); //consumer tarafında headerdan 1 tanesi uyuşssa yeterli.

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true; //Mesajımızın herhangi bir durumda silinmemesi için
                    properties.Headers = headers; //exchange tipi header oldugu için belirlediğim header dictionary'sini veriyorum.

                    byte[] bodyByte = Encoding.UTF8.GetBytes("Header Mesajım"); //mesajlarımızı byte olarak göndermeliyiz.

                    /*
                     * exchange: Yukarıda tanımladığım exchange ismini gönder yayınlıyorum.
                     * routingKey: direct-exchange de routeKey verilir. Consumer tarafında da routeKey aynı olan consumer'la aralarında iletişim kurarlar.
                     * body: göndereceğimiz mesaj. (mesajlar her zaman byte türünde olmalıdır.)
                     */
                    channel.BasicPublish( // Kuyruğa mesajı gönderiyoruz.
                        exchange: "header-exchange",
                        routingKey: string.Empty,
                        basicProperties: properties,
                        body: bodyByte
                        );

                    Console.WriteLine("Header mesajı gönderilmiştir");
                }
                Console.WriteLine("Çıkış yapmak için tıklayınız..");
                Console.ReadKey();
            }
        }
    }
}
