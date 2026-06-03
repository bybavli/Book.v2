# Book.v2 - Okuma Platformu ve API Projesi 📚

Modern web teknolojileri kullanılarak geliştirilmiş, N-Tier (Çok Katmanlı) mimari prensiplerine sadık kalan, veritabanı odaklı bir kitap okuma platformu ve yönetim sistemidir. 

Bu proje hem gerçek bir web uygulamasının özelliklerini (3D sayfa çevirme, ilerleme kaydetme vb.) barındırır, hem de ileri düzey **Entity Framework Core (SQL Server)** veritabanı işlemlerini (Çoka-Çok ilişkiler, LINQ sorguları, CRUD) kapsar.

## 🎯 Projenin Amacı
Projenin temel amacı, kullanıcıların kitap okuyabileceği, okuma listeleri oluşturabileceği ve kaldıkları yeri (okuma yüzdelerini) takip edebileceği bir sistem yaratmaktır. Arka planda ise Nesne Yönelimli Programlama (OOP) ve İlişkisel Veritabanı Mantığını kullanarak en iyi kodlama standartlarını (Clean Code, DTO kullanımı, Repository & Service katmanları) göstermeyi hedefler.

## ✨ Öne Çıkan Özellikler
* **3D Etkileşimli Okuma:** Kitap sayfaları, gerçek bir kitap okuyormuş hissi veren `StPageFlip` animasyon motoru ile ekrana çizilir.
* **Akıllı İlerleme Takibi (Debounce API):** Kullanıcı sayfaları hızlıca geçse dahi, sunucu yorulmaz. İlerleme verisi gecikmeli (debounce) ve güvenli bir şekilde arka planda veritabanına işlenir.
* **Tam Ekran (Fullscreen) Desteği:** Dikkat dağıtıcı unsurları kaldırarak sürükleyici bir okuma deneyimi sunar.
* **İçerik Tabanlı Öneri Algoritması (Recommendation):** Kullanıcının okuma listesindeki kitapların türlerine ve etiketlerine bakarak (Jaccard Benzerliği) ona yeni kitaplar önerir.
* **Gelişmiş Veritabanı Mimarisi:** Tablolar arası `virtual ICollection` bağlantıları, Bire-Çok (One-to-Many) ve Çoka-Çok (Many-to-Many) ilişki kurguları ile boşta tablo bırakılmayan sağlam bir SQL Server altyapısı mevcuttur.

## 🛠️ Kullanılan Teknolojiler
* **Backend:** C# / .NET 10.0
* **API:** ASP.NET Core Web API
* **Veritabanı (ORM):** Entity Framework Core (Code-First)
* **Veritabanı Sunucusu:** Microsoft SQL Server (MSSQL / LocalDB)
* **Frontend:** Saf HTML5, CSS3, Vanilla JavaScript (Framework kullanılmamıştır)
* **Kütüphaneler:** StPageFlip (Sayfa çevirme), System.Text.Json (ReferenceHandler.IgnoreCycles)

---

## 🚀 Kurulum ve Çalıştırma Rehberi

Projeyi kendi bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyebilirsiniz.

### Gereksinimler
1. [**.NET 10.0 SDK**](https://dotnet.microsoft.com/download) veya üzeri yüklü olmalıdır.
2. **Microsoft SQL Server** veya Visual Studio ile birlikte gelen **LocalDB** kurulu olmalıdır.
3. Projeyi açmak için Visual Studio 2022 (veya VS Code) tavsiye edilir.

### Adım Adım Kurulum

**1. Projeyi Bilgisayarınıza İndirin:**
```bash
git clone https://github.com/KULLANICI_ADINIZ/Book.v2.git
cd Book.v2/Book.v2
```

**2. Veritabanını Ayağa Kaldırın (Migration İşlemi):**
Proje Code-First mimarisi ile yazıldığı için, aşağıdaki komutu çalıştırarak tabloların SQL Server'da otomatik oluşmasını sağlayın:
```bash
dotnet ef database update
```
*(Not: Hata alırsanız `dotnet tool install --global dotnet-ef` komutu ile Entity Framework CLI aracını kurun).*

**3. Projeyi Çalıştırın:**
Terminal üzerinden başlatmak için:
```bash
dotnet run
```
Veya **Visual Studio 2022** kullanıyorsanız, yukarıdaki yeşil "Run" (Oynat / F5) butonuna basmanız yeterlidir.

**4. Tarayıcıda Açın:**
Proje başarıyla derlendikten sonra tarayıcınızda otomatik olarak açılacaktır (Eğer açılmazsa `http://localhost:5258/` adresine gidebilirsiniz). 

---

## 📋 Ödev ve Test Uç Noktaları
Projenin Entity Framework ilişkilerini ve LINQ yeteneklerini test etmek için özel bir test arayüzü yazılmıştır.
Proje çalışırken tarayıcınızın adres çubuğuna şunu yazarak test ekranına ulaşabilirsiniz:
👉 **`http://localhost:5258/assignment.html`**

Bu ekrandan C# tarafındaki `AssignmentController`'a istek atarak aracı (junction) tablolar üzerindeki `INNER JOIN` işlemlerini ve Filtreleme (`WHERE`) işlemlerini canlı olarak görebilirsiniz.
