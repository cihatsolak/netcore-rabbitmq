using Spire.Doc;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using WebConsumer.Models;

namespace WebConsumer
{
    public static class Utilities
    {
        public static bool EmailSender(string email, MemoryStream memoryStream, string fileName)
        {
            try
            {
                memoryStream.Position = 0; //Dosyayı okuması için çok önemli. Dosyayı ilk satırdan itibaren oku.

                ContentType contentType = new ContentType(MediaTypeNames.Application.Pdf);

                Attachment attachment = new Attachment(memoryStream, contentType);
                attachment.ContentDisposition.FileName = $"{fileName}.pdf";

                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress("kobiticariotomasyon@hotmail.com"),
                    Subject = "RabbitMQ ile Pdf Dosyası Gönderme",
                    Body = "Pdf dosyanız ektedir.",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Attachments.Add(attachment);

                SmtpClient smtpClient = new SmtpClient()
                {
                    Credentials = new NetworkCredential("****", "****"),
                    Port = 587,
                    Host = "smtp.live.com",
                    EnableSsl = true
                };

                smtpClient.Send(mailMessage);

                Console.WriteLine("Sonuç: {0} adresine  pdf gönderilmiştir.", email);

                memoryStream.Close();
                memoryStream.Dispose();

                return true;
            }
            catch(Exception)
            {
                Console.WriteLine("Mesaj Gönderilemedi!");
                return false;
            }
        }

        public static MemoryStream ConvertWordToPdf(MessageBodyParameterModel messageBodyParameterModel)
        {
            try
            {
                //Nuget Package Manager üzerinden kurdugum framework'ü kullanıyorum convert için.
                Document document = new Document();
                document.LoadFromStream(new MemoryStream(messageBodyParameterModel.File), FileFormat.Docx2013);

                using MemoryStream memoryStream = new MemoryStream();
                document.SaveToStream(memoryStream, FileFormat.PDF);

                return memoryStream;
            }
            catch (Exception)
            {
                throw new Exception("Dosya çevirme işlem başarısız.");
            }
        }
    }
}
