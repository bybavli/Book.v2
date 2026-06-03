BookOku Web Platform

BookOku, modern web teknolojileri kullanilarak gelistirilmis kapsamli bir dijital kitap okuma platformudur. Sistem, kullanicilarin kitap okuma deneyimini artirmak icin yapay zeka tabanli yuz takibi, sesli okuma ve icerik tabanli oneriler gibi yenilikci ozellikler sunmaktadir.
Teknolojiler ve Altyapi

    Backend: ASP.NET Core 10.0 (Web API)
    Veritabani: Microsoft SQL Server (MSSQL), Entity Framework Core 10
    Frontend: HTML5, CSS3, Vanilla JavaScript (Moduler Yapi)
    Mimari: Katmanli Mimari (Servis ve Repository Pattern)
    Yapay Zeka: MediaPipe Tasks Vision (Yuz Takibi)
    Animasyon: St.PageFlip (Fiziksel Sayfa Cevirme Efekti)

Temel Ozellikler

    Kapsamli Okuyucu Arayuzu

    Fiziksel kitap hissiyati veren gercekci sayfa cevirme animasyonlari.
    Tam ekran okuma modu.
    Sayfa bazli ilerleme kaydi ve kitap bitirme yuzdesinin anlik hesaplanmasi.

    Yapay Zeka ile Temassiz Kontrol

    Kullanici kamerasini kullanarak gercek zamanli yuz takibi.
    Sadece hafif bas hareketleri ile sayfalari ileri veya geri cevirebilme.
    Optimizasyonlu ve yuksek hassasiyetli burun/yanak aci hesaplamasi ile donanimi yormayan performans.

    Sesli Okuma (Text-to-Speech)

    Kitap sayfalarini tarayicinin yerlesik ses sentezleyicisi ile sesli okutma.
    Okunmakta olan sayfalarin otomatik olarak takip edilmesi ve kalinan yerden devam edilebilmesi.

    Icerik Tabanli Onerme Sistemi (Recommendation Engine)

    Kullanicinin daha once okudugu kitaplarin tur ve etiketlerine dayali ozel algoritma.
    Jaccard benzerlik metrigi ve kategori ortusmeleri hesaplanarak kisisellestirilmis kitap tavsiyeleri sunulmasi.

    Dinamik Kutuphane ve Okuma Listesi

    Kullanicinin okumaya basladigi kitaplarin otomatik olarak okuma listesine eklenmesi.
    Yarida kalan kitaplar icin gercek zamanli yuzdelik ilerleme gosterimi ("Okumaya Devam Et" bolumu).
    Bitirilen kitaplarin ana sayfada ozel bir basari bolumunde listelenerek kullanici motivasyonunun artirilmasi.

Kurulum ve Calistirma

    Visual Studio 2022 veya daha yeni bir surum uzerinden projeyi acin.
    appsettings.json dosyasinda bulunan baglanti dizesinin (DefaultConnection) yerel SQL Server orneginize (Server=.;Database=BookDb) dogru isaret ettiginden emin olun.
    Projeyi derleyerek (Build) calistirin.
    Entity Framework Core, "BookDb" isimli veritabanini sunucunuzda otomatik olarak olusturacak ve icerisini baslangic icin gereken ornek kitaplar, sayfa icerikleri ve kullanici datalari ile dolduracaktir.
    Tarayicida acilan arayuz uzerinden platformu kullanmaya baslayabilirsiniz.
