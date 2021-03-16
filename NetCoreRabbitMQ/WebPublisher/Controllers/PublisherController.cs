using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using WebPublisher.Models;
using WebPublisher.Models.Settings;

namespace WebPublisher.Controllers
{
    public class PublisherController : Controller
    {
        private readonly RabbitMQSettings _rabbitMQSettings;
        public PublisherController(IOptions<RabbitMQSettings> rabbitMQSettings)
        {
            _rabbitMQSettings = rabbitMQSettings.Value;
        }

        /// <summary>
        /// Word to pdf
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Converter()
        {
            return View();
        }

        /// <summary>
        /// Word to pdf
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Converter(ConverterViewModel converterViewModel)
        {
            var messageBodyParameter = new MessageBodyParameterModel
            {
                Email = converterViewModel.Email,
                FileName = Path.GetFileNameWithoutExtension(converterViewModel.File.FileName),
                File = FileToByteArray(converterViewModel.File)
            };

            string messageBodyParameterSerilazed = JsonSerializer.Serialize(messageBodyParameter);
            byte[] bodyMessageArray = Encoding.UTF8.GetBytes(messageBodyParameterSerilazed);

            KuyrugaMesajiGonder(bodyMessageArray);

            ViewBag.Message = "Word dosyanız pdf dosyasına dönüştürüldükten sonra, size email olarak gönderilecektir.";

            return RedirectToAction(nameof(Converter));
        }

        private byte[] FileToByteArray(IFormFile formFile)
        {
            using MemoryStream memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);

            byte[] fileByteArray = memoryStream.ToArray();

            return fileByteArray;
        }

        private void KuyrugaMesajiGonder(byte[] bodyMessageArray)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_rabbitMQSettings.CloudUrl)
            };

            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();

            /* EXCHANGE OLUŞTURMA
             * exchage: belirleyeceğimiz özel exchange ismi
             * type: exchange tipi (fanout, direct, topic, header)
             * durable: true -> mesajlar otomatik olarak silinmesin.
             */

            channel.ExchangeDeclare(
                exchange: "convert-exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
                );

            /* KUYRUK OLUŞTUR
             * Libraries içerisinde publisher'da hiçbir zaman kuyruk oluşturmadık. Çünkü kuyrugu dinleyecek olan consumer tarafı oluşturur. Fakat burada kuyruk oluşturma sebebim, beni dinleyecek kimse yoksa bile
             * kuyruğa veriyi göndermek için.
             * 
             * exclusive: birden fazla bağlantı bu kuyrugu kullanabilsin.
             * autoDelete: false -> otomatik silinmesin
             */
            channel.QueueDeclare(
                queue: "file",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );

            /*
             * Mesajlarımın kaybolmaması için kuyruğu bind edeceğim, lbiraries içerisinde yaptıgım örneklerde kuyruk bind işlemini consumer tarafında yapmıştım.
             * Fakat burada gönderdiğim mesajların havada kalıp kaybolmasını istemediğim için kuyruğu bind ediyorum.
             * 
             * queue: Kuyruk ismi
             * exchange: kuyruk hangi exchange'e bağlancak?
             * routingKey: kuyrugun route parametresi
             */

            channel.QueueBind(
                queue: "file",
                exchange: "convert-exchange",
                routingKey: "wordtopdf"
                );

            //Kanal'ın özellikleri
            IBasicProperties basicProperties = channel.CreateBasicProperties();
            basicProperties.Persistent = true;

            //Kanalı yayınlıyorum
            channel.BasicPublish(
                exchange: "convert-exchange",
                routingKey: "wordtopdf",
                basicProperties: basicProperties,
                body: bodyMessageArray
                );
        }
    }
}
