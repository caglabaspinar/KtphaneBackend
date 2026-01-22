

# KtphaneBackend – Library Management System Web API

Bu proje, bir Kütüphane Yönetim Sistemi (Library Management System) için geliştirilmiş katmanlı bir RESTful Web API uygulamasıdır. Projede kütüphane, kitap ve öğrenci kayıtları yönetilmekte; öğrencilerin kitap ödünç alma işlemleri gerçekleştirilmektedir. Geliştirilen API, Android tabanlı frontend uygulaması tarafından kullanılmaktadır.

Yazılım Dili = C#
Framework = ASP.NET Core (.NET 8)
Veritabanı = MS SQL Server LocalDB
Veri Erişim Teknolojisi = Entity Framework Core
Güvenlik: JWT (JSON Web Token) tabanlı temel kimlik doğrulama altyapısı
API Belgeleme = Swagger UI (OpenAPI)


# API Kullanım Akışı

>Kütüphane Kaydı
POST /api/Library
Sistem içinde yeni bir kütüphane oluşturmak için kullanılır.
Body olarak LibraryCreateDto gönderilir.

>Kütüphane Listeleme
GET /api/Libraries
Sistemde kayıtlı tüm kütüphaneleri listelemek için kullanılır.
Bu endpoint, her bir kütüphaneye ait temel bilgileri (id, ad vb.) döndürür ve
istemci tarafında kütüphane listesinin görüntülenmesini sağlar.

>Kitap Kaydı
POST /api/Books
Sistem içine yeni kitap eklemek için kullanılır.
Body olarak BookCreateDto (veya projendeki kitap oluşturma DTO’su) gönderilir.
Kitap bir kütüphaneye bağlı tutuluyorsa ilgili LibraryId bu aşamada gönderilir.

>Kitap Listeleme 
GET /api/Books
Sistemde kayıtlı tüm kitapları listeler.

>PUT /api/Books/{id}
Var olan bir kitabın bilgilerini güncellemek için kullanılır.
Body’de kitabın güncel hali gönderilir. Başarılı olursa kitap kaydı güncellenir.

>Öğrenci Kaydı
POST /api/Students
Sisteme yeni öğrenci kaydı oluşturur.
Body olarak öğrenci kayıt DTO’su gönderilir.

>Öğrenci Giriş (Login)
POST /api/Students/Login
Kullanıcı adı/şifre ile giriş yapılır.
Başarılı olursa response olarak JWT token üretilir ve döndürülür.

>Kitap Ödünç Alma
POST /api/StudentBooks
Öğrenci–kitap ödünç alma kaydı oluşturur.
Body’de (projendeki DTO’ya göre) en az şu bilgiler bulunur: StudentId, BookId

>Öğrenci Detay Görüntüleme
GET /api/Students/{id}
Belirli bir öğrenciye ait detay bilgileri getirir.

>Reports (Raporlama)

GET /api/Reports/library/{libraryId}
Belirli bir kütüphaneye ait ödünç alma kayıtlarını raporlamak için kullanılır.
Bu endpoint, ilgili kütüphanede bulunan kitapların hangi öğrenciler tarafından
ödünç alındığına dair bilgileri listelemek amacıyla tasarlanmıştır.

GET /api/Reports/student/{studentId}
Belirli bir öğrenciye ait ödünç alma geçmişini raporlamak için kullanılır.
İlgili öğrencinin hangi kitapları, hangi kütüphanelerden ödünç aldığına
dair kayıtlar bu endpoint üzerinden görüntülenebilir.

# Kurulum

Projenin temel mimarisini oluşturmak amacıyla öncelikle Entity Framework Core paketleri kullanılmıştır.
C# model sınıflarının veritabanı tablolarına dönüştürülmesi için Microsoft.EntityFrameworkCore paketi,
MS SQL Server ile iletişim kurulabilmesi için ise Microsoft.EntityFrameworkCore.SqlServer paketi
projeye dahil edilmiştir.

Veritabanı migration işlemlerinin ve ilgili komutların çalıştırılabilmesi amacıyla
Microsoft.EntityFrameworkCore.Tools paketi kullanılmıştır.

API endpoint’lerinin, DTO şemalarının ve modellerin otomatik olarak dokümante edilmesi ve
interaktif test edilebilmesi amacıyla Swashbuckle.AspNetCore ve
Microsoft.AspNetCore.OpenApi paketleri kullanılmıştır.


# Temel NuGet paketlerinin kurulmasının ardından, projenin veri yapısı oluşturulmuştur.

Varlık Modelleri (Models):  
Projenin ana varlıklarını temsil eden Book, Library, Student ve StudentBook sınıfları
C# model sınıfları olarak tanımlanmıştır.

Veritabanı Bağlantısı (DbContext):  
Tanımlanan modellerin veritabanı ile ilişkilendirilebilmesi amacıyla LMSDbContext.cs sınıfı,
Entity Framework Core’un DbContext sınıfından türetilmiştir. İlgili modeller DbSet olarak
bu sınıfa eklenmiştir.

Bağlantı Dizesi (Connection String):  
Veritabanı adresi ve bağlantı bilgileri, DefaultConnection adıyla appsettings.json
dosyasında yapılandırılmıştır.

Veritabanı Oluşturma (Migration):  
Entity Framework Core migration komutları (dotnet ef database update) çalıştırılarak
veritabanı şeması fiziksel olarak oluşturulmuştur.


# Model ve veritabanı yapısı hazırlandıktan sonra, uygulamanın çalışması için gerekli olan temel
# hizmetler ve yapılandırmalar Program.cs dosyasında tanımlanmıştır.

Veritabanı Hizmetinin Eklenmesi:  
appsettings.json dosyasında tanımlanan bağlantı dizesi kullanılarak,
Entity Framework Core’a ait LMSDbContext sınıfı bir servis olarak uygulamaya eklenmiştir
(builder.Services.AddDbContext<LMSDbContext>).

Controller ve Swagger Hizmetleri:  
API Controller’larının çalışabilmesi ve endpoint’lerin test edilebilmesi amacıyla
AddControllers ve AddSwaggerGen servisleri uygulamaya dahil edilmiştir.

JWT Kimlik Doğrulama (Authentication) Yapılandırması:  
Projede kullanıcı giriş işlemi sonrasında JWT (JSON Web Token) üretimi yapılacak şekilde temel bir kimlik doğrulama altyapısı hazırlanmıştır. Bu yapı, uygulamada ileride yetkilendirme mekanizmalarının eklenebilmesi için temel bir altyapı oluşturmayı amaçlamaktadır.


# Veritabanı ve temel servislerin tanımlanmasının ardından, API’nin iş mantığını ve
# dış dünyaya açılan uç noktalarını (endpoint’leri) oluşturma sürecine geçilmiştir.

Veri Transfer Nesneleri (DTO’lar):  
DTOs klasörü altında, veritabanı modellerinin doğrudan dışarı açılmasını engellemek amacıyla
belirli işlemler için sadeleştirilmiş veri yapıları (BookCreateDto, StudentLoginDto, vb.) tanımlanmıştır. Bu yaklaşım, gereksiz veri taşınmasını önlemiş ve
hassas alanların (örneğin şifre bilgileri) korunmasını sağlamıştır.

Controller’lar ve Endpoint’ler:  
Controllers klasöründe, API isteklerini karşılayan controller sınıfları
(BooksController, StudentsController vb.) oluşturulmuştur. Bu controller’lar,
HTTP metotları (GET, POST, PUT) üzerinden temel CRUD işlemlerini ve login sürecini yürütmektedir.

Proje çalıştırılmadan önce connection string ayarlarının yapılması ve migration işlemlerinin çalıştırılması gerekmektedir. Ardından uygulama çalıştırılarak Swagger arayüzü üzerinden endpoint’ler test edilebilir.


