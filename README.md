# Net Core RabbitMQ

## Nedir?

RabbitMQ mesaj kuyruk sistemidir.

![Screenshot_3](https://user-images.githubusercontent.com/54249736/111044957-9fee8a00-845c-11eb-8c5c-9c9736ccd905.png)


Uygulamadan mesajı alır ve sırası geldiğinde iletir. Bunu aslında kargo firması gibi düşünebiliriz. Kargo firmasına bir kargo gelir ve kargonun vakti geldiğinde dağıtıma çıkarılır. Burada da mantık aynı şekildedir. RabbitMQ bir mesaj gelir ve bu zaman yeri geldiği zaman iletilir.

Yukarıdaki görselde Publisher(Yayımcı) mesajı gönderir, Exchange bu mesajı karşılar daha sonra routes ile beraber kuyruğa gelir. Sonra da Consumer(Tüketici) bu kuyruktaki mesajları tüketir.

## RabbitMQ nasıl kurulur? (Cloud ortama ya da windows ortama kurabiliriz.)

### Windows işletim sistemine kurmak için;

1. "https://www.erlang.org/" ve "https://www.rabbitmq.com/" adreslerinden exe dosyalarını indiriyoruz. 
2. İlk olarak erlang kurumunu yapıyoruz ve sonrasında rabbitmq kurulumunu yapıyoruz.
3. Kurulum sonrasında RabbitMQ windows servisinin kurulumunun başarılı olup olmadığına "services.msc" adresinden kontrol ediyoruz. Çalışmıyorsa start ediyoruz.
4. Tarayıcı üzerinden yönetim paneline erişmek için komut penceresinden rabbitmq'nun kurulu olduğu dosya dizinininden sbin klasörü içerisinde "rabbimq-plugins enable rabbitmq_management" komutu çalıştırılır.
5. Komut çalıştırıldıktan sonra rabbitmq paneline "http://localhost:15672/" adresinden erişebilirsiniz. Username ve password default olarak "guest" gelmektedir.

![1](https://user-images.githubusercontent.com/54249736/111049405-9534f480-845e-11eb-91e3-fce5292bd403.png)

### Cloud ortama kurmak için;
1. https://www.cloudamqp.com/ adresine gidiyoruz ve kayıt oluyoruz, giriş yapıyoruz.
2. "Create New Instance" diyip gerekli bilgileri doldurduktan sonra kendimize bir instance oluşturuyoruz. Artık hazırız!

## Work Queues
* **Message Durability**: Mesajların dayanıklılığı
* **Message acknowledgment**: Mesajın alındı haberi
* **Fair Dispatch**: Eşit Dağılımı

![image](https://user-images.githubusercontent.com/54249736/111050717-4b004300-845f-11eb-84fa-3ba8377ad111.png)

### Fair Dispatch
Örneğin bizim iki tane consumer(c1, c2) miz var. C1 in c2 den daha güçlü bir yapıya sahip olduğu için sürekli c1 kuyruktaki mesajları işliyor. Aralarındaki dağılımın eşit olması için fair dispatch özelliğini kullanıyoruz. Başka bir örnek vermem gerekirse, c1’in işlemcisi ve ram oranı c2 ye göre fazla olabilir yani daha güçlü donanıma sahip olabilir. Bu durumda kuyruktaki mesajlar devamlı olarak c1 tarafından işlenecektir. Bunu dengelemek için fair dispatch’i kullanıyoruz.
Ayrıca bunlar haricinde bir mesaj kuyruktayken yeni bir mesaj consumer tarafından alınmamalı.
