KtphaneBackend – Library Management System Web API

Bu proje, modern ve katmanlı mimariye sahip bir Library Management System RESTful Web API uygulamasıdır.

Sistem; öğrenci kayıt ve giriş işlemleri, rol bazlı yetkilendirme (Admin ve Student), kitap ve kütüphane yönetimi, kitap ödünç alma ve teslim etme süreçleri, raporlama mekanizması ve şifre sıfırlama akışlarını JWT tabanlı güvenlik altyapısı ile yönetmektedir.

Bu proje hem güvenlik hem de sürdürülebilirlik odaklı tasarlanmıştır.

Kullanılan Teknolojiler
Yazılım Dili: C# Framework: ASP.NET Core (.NET 8) Veritabanı: MS SQL Server LocalDB ORM: Entity Framework Core Kimlik Doğrulama: JWT (JSON Web Token) Yetkilendirme: Role Based Authorization Şifre Güvenliği: PBKDF2 (Salt + Iteration) Doğrulama: FluentValidation Mail Servisi: MailKit (SMTP) API Dokümantasyonu: Swagger (OpenAPI)

Sistem Özellikleri
Öğrenci Kaydı ve Giriş Yeni kullanıcı kaydı oluşturulabilir. Giriş işlemi başarılı olduğunda JWT token üretilir. Token içerisinde kullanıcı kimliği ve rol bilgisi taşınır.

Rol Bazlı Yetkilendirme Admin ve Student rolleri bulunmaktadır. Kitap ekleme, güncelleme ve silme işlemleri yalnızca Admin yetkisine sahiptir. Öğrenciler yalnızca kendi ödünç kayıtlarını görüntüleyebilir.

Şifre Güvenliği Şifreler veritabanında düz metin olarak tutulmaz. PBKDF2 algoritması ile salt kullanılarak hashlenir. Şifre doğrulama işlemi sabit zamanlı karşılaştırma ile yapılır.

Şifre Sıfırlama Sistemi Kullanıcı e-posta adresi ile şifre sıfırlama talebinde bulunabilir. Sisteme kayıtlı e-posta adresine 6 haneli doğrulama kodu gönderilir. Kodun geçerlilik süresi vardır. Yeni şifre eski şifre ile aynı olamaz.

Kütüphane ve Kitap Yönetimi Kütüphaneler listelenebilir, eklenebilir ve güncellenebilir. Kitaplar listelenebilir, eklenebilir, güncellenebilir ve silinebilir. ISBN bilgisi normalize edilir ve benzersizlik kontrolü yapılır. Aktif ödünçte bulunan veya geçmişi olan kitaplar silinemez.

Ödünç Alma Sistemi Giriş yapan öğrenci kitap ödünç alabilir. Öğrenci kitabı teslim edebilir. Aktif ödünçler ve geçmiş kayıtlar görüntülenebilir.

Raporlama Belirli bir kütüphaneye ait kitaplar listelenebilir. Öğrenci bazlı ödünç geçmişi görüntülenebilir. Admin tüm ödünç kayıtlarını sistem genelinde görüntüleyebilir.

Mimari Yapı
Proje katmanlı mimari yaklaşımı ile geliştirilmiştir.

Controllers katmanı API uç noktalarını içerir. Services katmanı iş mantığını ve yardımcı servisleri içerir. Validators katmanı FluentValidation ile doğrulama kurallarını barındırır. Middleware katmanı global hata yönetimini sağlar. Data katmanı DbContext ve veritabanı işlemlerini içerir. Models ve DTOs katmanları veri yapısını temsil eder.

Veritabanı Yapısı
Sistemde dört ana varlık bulunmaktadır:

Library Book Student StudentBook

Bir kütüphane birden fazla kitaba sahip olabilir. Bir öğrenci birden fazla kitap ödünç alabilir. Ödünç ilişkileri StudentBook tablosu üzerinden yönetilir.

Kurulum
Projeyi klonladıktan sonra appsettings.json dosyasındaki bağlantı dizesi kontrol edilmelidir.

Migration işlemleri için:
Add-Migration MigrationName Update-Database

Ardından proje çalıştırılabilir.

Swagger arayüzü üzerinden endpoint’ler test edilebilir.

Proje Amacı:
Katmanlı mimari uygulamak JWT tabanlı güvenli kimlik doğrulama sistemi kurmak Rol bazlı yetkilendirme implementasyonu yapmak Güvenli şifre saklama yöntemlerini uygulamak Sürdürülebilir ve genişletilebilir bir backend mimarisi oluşturmak.

