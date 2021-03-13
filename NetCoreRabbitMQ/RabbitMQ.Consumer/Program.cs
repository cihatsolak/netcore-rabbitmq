using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.Consumer
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

            using (var connection = connectionFactory.CreateConnection()) //Bağlantımı oluşturuyorum.
            {
                using (var channel = connection.CreateModel()) //Bağlantımız üzerinden kanalımızı açıyoruz.
                {
                    // PUBLISHER(gönderici) tarafındaki kuyruk ile CONSUMER(tüketici) tarafındaki kuyruk ismi ve parametreler birebir aynı olmalıdır. Aksi halde eşleşmez ve mesajları alamayız.

                    /*
                    * queue: Kuyruğumuzun ismi
                    * durable: false yaparsak, rabbitmq instance ımız restart atarsa mesajların hepsi gider. True yaparsak rabbitmq bunu fiziksel diske yazar.
                    * exclusive: bu kuyruğa sadece 1 tane mi kanal bağlansın yoksa başka kanallarda bağlanabilsin mi? false: diğer kanallarda bağlansın anlamına gelir.
                    * autoDelete: bir kuyrukta diyelim ki 20 tane mesaj var, eğer son mesajda kuyruktan çıkarsa yani kuyrukta mesaj kalmazsa bu kuyruk silinsin mi?
                    */
                    channel.QueueDeclare(
                         queue: "rabbitMqKuyruk",
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null
                         ); //Kuyruk oluşturalım. (Publisher'daki kuyruk ile birebir aynı.)

                    //Oluşturduğum kanalın özelliklerini belirtiyorum.
                    channel.BasicQos(
                        prefetchSize: 0,  //Gelen mesajın boyutuyla ilgilenmiyorum
                        prefetchCount: 1, //Gelen mesajı consumerlar arasında 1-1 dağıt. 1 ona 1 ona şeklinde. Eğer 2 yazarsam tek seferde 2 adet mesaj(iş) gelir.
                        global: false   // prefetchCounta 10 atadığımızı consumer adedimizinde 3 tane olduğunu düşünelim. Eğer global'i false işaretlersem 3 consumer'da ayrı ayrı 10 görev alır, eğer true dersem 3 consumer toplamda 10 adet iş alır.
                        );

                    var eventingBasicConsumer = new EventingBasicConsumer(channel); //Oluşturduğum kanalı dinle diyorum.

                    /*
                     * autoAck: true -> mesaj gönderiminden sonra otomotik olarak silisin. false -> sen otomotik silme ben silmen için sana haber vericem.
                     * örneğin bir resim işleyeceğiz ve hata aldık, hata aldığımzda kuyruktan silinmesin, ne zamanki işlemi başarıyla tamamlarız o zaman silinsin.
                     */
                    channel.BasicConsume(
                        queue: "rabbitMqKuyruk", //Dinlecek kuyruk ismi
                        autoAck: true, //True: işlemi doğru ya da yanlış farketmez denedikten sonra kuyruktan silecektir. False: ben işlemin bittiğini sana söyleyeceğim. sana söyledikten sonra silersin.
                        consumer: eventingBasicConsumer
                        );

                    eventingBasicConsumer.Received += (model, basicDeliverEventArgs) =>
                    { 
                        var bodyByte = basicDeliverEventArgs.Body.ToArray(); //Publisher tarafından göndermiş olduğum mesajı alıyorum.
                        var message = Encoding.UTF8.GetString(bodyByte);

                        //işlendi bildirimi gönderiyoruz. Bu bilgiyi göndermediğimizde bir sonraki mesajı bu consumer'a iletmez.
                        channel.BasicAck(
                            deliveryTag: basicDeliverEventArgs.DeliveryTag, //İşlemi tamamladım bildirimi gönderiyorum. Yeni bir mesaj alabilirim.
                            multiple: false //tüm işlemler için değil sadece bu işlem için
                            );

                        Console.WriteLine("Mesaj Alındı: {0}", message);
                    };
                }

                Console.ReadKey();
            }

        }
    }
}
