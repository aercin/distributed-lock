# distributed-lock
Single Node Distributed Lock ve RedLock Algorithm içeren 2 örnek uygulamalı solution

* Single Node Distributed Lock; tek bir master'a veya bir kaç tane master replicasının bulunduğu bir redis topoloji üzerine kuruludur.
Tek bir master node'un olduğu kurguda master node down olursa locklar bütünen ortadan kalkacaktır. Master replicalarının olduğu kurguda;
Master node üzerindeki lockları, replica masterlara projection edemeden down olduğunda yine locklar ortadan kalkacaktır.(Single point of Failure) 
Yüzeysel olarak distributed lock çözümü olarak yaygın bir sekilde bu yapı kullanılmaktadır.
Mantık; Redisde expire süresi olan unique değerli bir key store etmek. Ve herhangi bir işlem yaparken bu key'in varlığını kontrol etmek diyebiliriz.
Ben bu örnekte Polly kütüphanesi kullanarak lock olması durumunda direk isteği reject etmek yerine; 3 kez deneme şansı veriyorum ve her bir deneme arasına
200ms'e kadar bir gecikme koyuyorum.
Aynı zamanda cachein otomatik silinmesini beklemeden de işlem gerçekleşmesi sonrası lockın kaldırılabilmesi için bir endpointde ekledim.
Redis Distributed Lock official dokumantasyonda belirtilen safety özelliğini korumak adına; direk key'i cacheden remove etmiyorum.
Lock'ı koyan kişinin sadece lock'u manuel kaldırmasına dair bir çözüm sunuyorum.

* Redlock Algoritması; Single Node Distributed Lock ile %90 map bir implimantasyonu var fakat ilave getirdiği tek yetenek Single point Of Failure durumuna 
çözüm sunması. Bu algoritmada tek bir redis master yok N adet birbirinden tamamen bağımsız redis masterlar söz konusu ve lock'un oluşabilmesi için
Master nodeların yeterli sayısında (N/2+1) lock itemı oluşması gerekiyor.
Farklı programlama dilleri için bu algoritmanın bir sürü kabul görmüş implimantasyonları söz konusu official dokumantasyonda neler olduguna bakılabilir.
.Net için ben redlock-cs implimantasyonu tercih ettim arka planında stackexchange.redis frameworku kullanmaktadır.
Tercih sebebim oluşan lock'ı bize export etmesi ve arzu ettiğimiz herhangi bir flowda bizim bu lock'i kaldırma sansımızın olmasıdır.
Özellikle microservis mimarisinde bir işlemin 3 servis üzerinden geçtikten sonra tamamladığını düşünecek olursak; ilk serviste lock'ın işlem bazlı oluşturulup,
ucuncu servise kadar çözülmemesi ve işlem sonlandıgında cozebiliyor olmak bu paketle mumkun.
