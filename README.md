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

## Exchange(Değiş-Tokuş) Tipleri
Not: Publisher === Producer demektir.

Mesajları exchange'e göndeririz, exchange'in tipine göre ilgili kuyruklara bu mesajlar iletilir. Bu işlem sonrasında consumer(tüketici)'ler mesajları alıp işler.
4 farklı exchange tipi vardır;
1- Fanout Exchange
2- Direct Exchange
3- Topic Exchange
4- Header Exchange

Exhange tipine göre route’lama yana kuyruğa gönderim işlemi farklılaşmaktadır.

### Fanout Exchange

![exchange-fanout](https://user-images.githubusercontent.com/54249736/111082575-0edde880-851a-11eb-8211-17f08d1b3b33.png)

Gelen mesjaların tümü kuyruklara (queue) dağıtılır, hiçbir ayrım söz konusu değildir. Ayrıca gelen mesaj tüm kuyrulara iletilir. 
Örnek olarak hangi senaryolarda kullanılır?
* Güncel oyun sonuçlarının tüm oyuncalara bildirilmesinde,
* Hava durumunun haber kanallarında yayınlanması
* Güncel oyun durumlarının tüm subcribe olanlara dagıtılması 

Bu exchange tipinde ne kadar kuyruğa bağlantı sağlamış consumer yani tüketici varsa gelen mesajların hepsi tüm consumer'lara iletilir. Örneğin kuyruğa 10 adet mesaj geldi ve bu kuyruğu dinleyen 5 adet consumer var ise bu 5 consumer da ayrı ayrı 10 adet mesaj alır.

### Direct Exchange (RouteKey Değerine Göre Dağıtılır)

![exchange-direct](https://user-images.githubusercontent.com/54249736/111082726-c70b9100-851a-11eb-8403-766803ea66e7.png)

Birebir gönderme durumudur yani adrese teslim diyebiliriz. Exchange'e mesaj gönderirken, bir tane de routeKey gönderiyoruz. Bu routeKey sayesinde exchange ilgili kuyruklara routeKey durumuna göre iletim yapıyor.

Görseldeki örnekten gidelim. Örneğin bir resim göndereceğiz ve exchange tipinide "direct-exchange" yaptık. Mesajlarımızı gönderirken routeKey'i "image_archive" belirttiğinizi düşünelim. Bu durumda consumer yani mesajları tüketecek olan routeKey olarak "image_archive" vermesi durumunda sadece o routeKey'e ait mesajları dinlemeye başlar.

Bir örnek daha vermem gerekirse, images’a(exchange) routeKey olarak “image_resize” verdik, resizer’a(consumer) da routeKey olarak “image_resize”  verirsek, consumer Exchange tarafındaki routeKey ‘image_resize’ olan kanalı dinleyecektir, diğer 3 kanalı (görselde görünen) dinlemeyecek yani mesajları almayacaktır.

### Topic Exchange (RouteKey Değerine Göre Dağıtılır)

![image](https://user-images.githubusercontent.com/54249736/111083031-857be580-851c-11eb-9c57-55a52ed46e77.png)

Mesaj dinleme olayını biraz daha özelleştirdiğimiz(customize) hale getirdiğimiz exchange tipidir.
Producer(Publisher) => RoutingKey = "Critical.Error.Warning"

(Yıldız = *) : Tek bir noktadaki ifadeyi daha doğrusu noktalardaki tek bir ifadeyi temsil etmektedir.

(Diyez = #) : Örneğin "#.Warning" -> Son noktası Warning olan, başı ne olursa olsun hiç farketmez yeterki son noktası warning ile bitsin.

Consumer => Routing Key = "(yıldız).Error.(yıldız)" : İlk ve son karakter ne olursa olsun ortaki karakter "Error" olmalı

Görselde;
BK = *.vegatable -> ilk kelime ne olursa olsun, noktadan sonra vegetable ile bitenleri dinle.
BK = # -> routingKey ne olursa olsun dinle.
BK = green.# -> ilk kelimesi green olan sonrasında ne olursa olsun kaç tane nokta gelirse gelsin dinle. Önemli olan başlangıcı green olması.

### Header Exchange

![image](https://user-images.githubusercontent.com/54249736/111083308-fec80800-851d-11eb-9a6d-caf65e642fdd.png)

Topic Exchange de olduğu gibi, burda da yine kuyruktaki mesajlarda seçiciliği arttırmak için kullanılan exchange tipidir.
Topic de routingKey belirliyorduk, burda da header'da (key,value) şekilde belirleme yapıyoruz yani header da dictionary gönderiyoruz.

###### Örnek
* "Metarial":"wood"
* "Type" : "cıpboard"

Publisher tarafından gönderilen, consumer tarafından alınan header kısmı.
* x-match:"any" dersek, key-value çiftlerinden en az birtanesi eşleşirse onu dinle.
* x-match:"all" dersek, key-value ların tamamının uyması beklenir.



