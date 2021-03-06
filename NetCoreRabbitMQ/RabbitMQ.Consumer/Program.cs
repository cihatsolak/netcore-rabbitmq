using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ.Consumer
{
    internal class Program
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

            using (var connection = connectionFactory.CreateConnection()) //Bağlantımı oluşturuyorum.
            {
                using (var channel = connection.CreateModel()) //Bağlantımız üzerinden kanalımızı açıyoruz.
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

                    /*
                    * İstekleri artık bir exchange'e göndereceğimiz için kuyruk oluşmaz. Ne zamanki Consumer yani tüketici ayağa kalkarsa o zaman bir kuyruk oluşur.
                    * Consumer ayağa kalktıgında farklı farklı kuyruklar oluşssun hep aynı kuyruk oluşmaması için random kuyruk ismi oluşssun istiyorum.
                    */
                    var queueName = "kuyruk1"; //Random kuyruk ismi oluşturuyorum.

                    channel.QueueDeclare(
                        queue: queueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                        );

                    Dictionary<string, object> headers = new Dictionary<string, object>();
                    headers.Add("format", ".pdf");
                    headers.Add("shape", "a4");
                    headers.Add("x-match", "all"); //publisher tarafından gelen header ile birebir uyuşsun.
                    //headers.Add("x-match", "any"); //publisher tarafından gelen headerdan 1 tanesi uyuşssa yeterli.

                    channel.QueueBind(
                             queue: queueName,
                             exchange: "header-exchange",
                             routingKey: string.Empty,
                             arguments: headers
                             );

                    //Oluşturduğum kanalın özelliklerini belirtiyorum.
                    channel.BasicQos(
                        prefetchSize: 0,  //Gelen mesajın boyutuyla ilgilenmiyorum
                        prefetchCount: 1, //Gelen mesajı consumerlar arasında 1-1 dağıt. 1 ona 1 ona şeklinde. Eğer 2 yazarsam tek seferde 2 adet mesaj(iş) gelir.
                        global: false   // prefetchCounta 10 atadığımızı consumer adedimizinde 3 tane olduğunu düşünelim. Eğer global'i false işaretlersem 3 consumer'da ayrı ayrı 10 görev alır, eğer true dersem 3 consumer toplamda 10 adet iş alır.
                        );

                    Console.WriteLine("Header mesajları bekleniyor..");

                    var eventingBasicConsumer = new EventingBasicConsumer(channel); //Oluşturduğum kanalı dinle diyorum.

                    /*
                     * autoAck: true -> mesaj gönderiminden sonra otomotik olarak silisin. false -> sen otomotik silme ben silmen için sana haber vericem.
                     * örneğin bir resim işleyeceğiz ve hata aldık, hata aldığımzda kuyruktan silinmesin, ne zamanki işlemi başarıyla tamamlarız o zaman silinsin.
                     */
                    channel.BasicConsume(
                        queue: queueName, //Dinlecek kuyruk ismi
                        autoAck: false, //True: işlemi doğru ya da yanlış farketmez denedikten sonra kuyruktan silecektir. False: ben işlemin bittiğini sana söyleyeceğim. sana söyledikten sonra silersin.
                        consumer: eventingBasicConsumer
                        );

                    eventingBasicConsumer.Received += (model, basicDeliverEventArgs) =>
                    {
                        byte[] bodyByte = basicDeliverEventArgs.Body.ToArray(); //Publisher tarafından göndermiş olduğum mesajı alıyorum.
                        string message = Encoding.UTF8.GetString(bodyByte);
                        Console.WriteLine("Header exchange mesajı alındı: {0}", message);

                        //işlendi bildirimi gönderiyoruz. Bu bilgiyi göndermediğimizde bir sonraki mesajı bu consumer'a iletmez.
                        channel.BasicAck(
                            deliveryTag: basicDeliverEventArgs.DeliveryTag, //İşlemi tamamladım bildirimi gönderiyorum. Yeni bir mesaj alabilirim.
                            multiple: false //tüm işlemler için değil sadece bu işlem için
                            );
                    };

                    Console.WriteLine("Çıkış yapmak tıklayınız..");
                    Console.ReadLine();
                }
            }
        }
    }
}
