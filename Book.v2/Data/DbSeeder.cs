using Book.v2.Models.Entities;
using Book.v2.Services.External;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Data;

public static class DbSeeder
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");




    public static async Task SeedAsync(ContextDb context, GoogleBooksService googleBooksService, ILogger logger)
    {

        if (context.Users.Any()) return;

        logger.LogInformation("Seeding database with real book data...");

        var user = User.CreateWithId(DemoUserId, "demo_user", "demo@kitapoku.com");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var preference = UserPreference.Create(
            DemoUserId,
            ["Bilim Kurgu", "Fantastik", "Klasik", "Roman"],
            ["distopya", "macera", "psikoloji", "felsefe"]);
        context.UserPreferences.Add(preference);
        await context.SaveChangesAsync();

        var books = await FetchBooksFromGoogleAsync(googleBooksService, logger);

        if (books.Count < 10)
        {
            logger.LogWarning("Google Books API returned too few results ({Count}), using fallback data", books.Count);
            books = CreateFallbackBooks();
        }

        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        foreach (var book in books)
        {
            var pages = GeneratePages(book.Id, book.TotalPages);
            context.BookPages.AddRange(pages);
        }
        await context.SaveChangesAsync();

        var readingListBooks = books.Take(5).ToList();
        foreach (var book in readingListBooks)
        {
            var entry = ReadingListEntry.Create(DemoUserId, book.Id);
            context.ReadingListEntries.Add(entry);
        }
        await context.SaveChangesAsync();

        var progress1 = ReadingProgress.Create(DemoUserId, readingListBooks[0].Id);
        progress1.UpdateProgress(5, readingListBooks[0].TotalPages);
        context.ReadingProgresses.Add(progress1);

        var progress2 = ReadingProgress.Create(DemoUserId, readingListBooks[1].Id);
        progress2.UpdateProgress(3, readingListBooks[1].TotalPages);
        context.ReadingProgresses.Add(progress2);

        var progress3 = ReadingProgress.Create(DemoUserId, readingListBooks[2].Id);
        progress3.UpdateProgress(7, readingListBooks[2].TotalPages);
        context.ReadingProgresses.Add(progress3);

        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded with {Count} books", books.Count);
    }



    private static async Task<List<Models.Entities.Book>> FetchBooksFromGoogleAsync(
        GoogleBooksService googleBooksService, ILogger logger)
    {
        var books = new List<Models.Entities.Book>();
        var seenTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var queries = new (string Query, string Genre, string Tags)[]
        {
            ("türk edebiyatı roman", "Roman", "edebiyat,türk,roman"),
            ("bilim kurgu romanları", "Bilim Kurgu", "bilim,gelecek,teknoloji"),
            ("fantastik roman türkçe", "Fantastik", "fantezi,macera,epik"),
            ("klasik dünya edebiyatı", "Klasik", "klasik,edebiyat,dünya"),
            ("psikoloji kitapları", "Psikoloji", "psikoloji,zihin,davranış"),
            ("felsefe kitapları", "Felsefe", "felsefe,düşünce,yaşam"),
            ("tarih kitapları türkiye", "Tarih", "tarih,osmanlı,cumhuriyet"),
            ("polisiye roman türkçe", "Polisiye", "gerilim,suç,dedektif"),
            ("şiir türk edebiyatı", "Şiir", "şiir,lirik,duygu"),
            ("biyografi otobiyografi", "Biyografi", "yaşam,gerçek,tarih"),
            ("distopya romanları", "Bilim Kurgu", "distopya,gelecek,toplum"),
            ("macera romanları", "Macera", "macera,keşif,yolculuk"),
            ("aşk romanları türk", "Roman", "aşk,dram,ilişki"),
            ("dünya klasikleri türkçe", "Klasik", "klasik,başyapıt,edebiyat"),
        };

        foreach (var (query, genre, tags) in queries)
        {
            try
            {
                var results = await googleBooksService.SearchByQueryAsync(query, 10);

                foreach (var result in results)
                {

                    if (seenTitles.Contains(result.Title)) continue;

                    if (string.IsNullOrWhiteSpace(result.Title) || result.Title.Length < 2) continue;

                    seenTitles.Add(result.Title);

                    var totalPages = result.PageCount > 0
                        ? Math.Min(result.PageCount / 20, 15)  // Scale real page count to our chunk size
                        : Random.Shared.Next(8, 15);

                    if (totalPages < 5) totalPages = Random.Shared.Next(8, 12);

                    var bookRating = result.Rating > 0
                        ? result.Rating
                        : Math.Round(3.5 + Random.Shared.NextDouble() * 1.5, 1);

                    var bookGenre = genre;
                    if (result.Categories.Count > 0)
                    {
                        bookGenre = MapGoogleCategory(result.Categories[0], genre);
                    }

                    var book = Models.Entities.Book.Create(
                        result.Title,
                        result.Author,
                        bookGenre,
                        totalPages,
                        result.Description ?? $"{result.Title} — {result.Author} tarafından yazılmış bu eser, {bookGenre.ToLower()} türünde öne çıkan bir yapıttır.",
                        result.ThumbnailUrl,
                        tags,
                        bookRating);

                    books.Add(book);
                }

                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch books for query '{Query}'", query);
            }
        }

        logger.LogInformation("Fetched {Count} books from Google Books API", books.Count);
        return books;
    }



    private static string MapGoogleCategory(string category, string fallback)
    {
        var cat = category.ToLowerInvariant();

        if (cat.Contains("fiction") && cat.Contains("science")) return "Bilim Kurgu";
        if (cat.Contains("science fiction")) return "Bilim Kurgu";
        if (cat.Contains("fantasy")) return "Fantastik";
        if (cat.Contains("mystery") || cat.Contains("detective") || cat.Contains("thriller")) return "Polisiye";
        if (cat.Contains("history")) return "Tarih";
        if (cat.Contains("philosophy")) return "Felsefe";
        if (cat.Contains("psychology")) return "Psikoloji";
        if (cat.Contains("biography")) return "Biyografi";
        if (cat.Contains("poetry")) return "Şiir";
        if (cat.Contains("adventure")) return "Macera";
        if (cat.Contains("romance") || cat.Contains("love")) return "Roman";
        if (cat.Contains("literary") || cat.Contains("classic")) return "Klasik";
        if (cat.Contains("fiction")) return "Roman";

        return fallback;
    }



    private static List<Models.Entities.Book> CreateFallbackBooks()
    {
        return
        [

            Models.Entities.Book.Create("1984", "George Orwell", "Bilim Kurgu", 12,
                "Totaliter bir rejimin bireyler üzerindeki baskısını anlatan, edebiyat tarihinin en önemli distopya romanlarından biri.",
                "https://covers.openlibrary.org/b/isbn/9789750718533-L.jpg",
                "distopya,totalitarizm,gözetim,özgürlük", 4.8),

            Models.Entities.Book.Create("Cesur Yeni Dünya", "Aldous Huxley", "Bilim Kurgu", 10,
                "İnsanlığın geleceğine dair karanlık bir vizyon sunan, toplumsal kontrol mekanizmalarını sorgulayan başyapıt.",
                "https://covers.openlibrary.org/b/isbn/9789750719387-L.jpg",
                "distopya,gelecek,toplum,bilim", 4.5),

            Models.Entities.Book.Create("Dune", "Frank Herbert", "Bilim Kurgu", 14,
                "Çöl gezegeni Arrakis'te geçen, güç, din ve ekoloji temalarını işleyen bilim kurgu klasiği.",
                "https://covers.openlibrary.org/b/isbn/9780441013593-L.jpg",
                "uzay,çöl,güç,ekoloji,macera", 4.6),

            Models.Entities.Book.Create("Fahrenheit 451", "Ray Bradbury", "Bilim Kurgu", 9,
                "Kitapların yasaklandığı bir gelecekte geçen, düşünce özgürlüğünü savunan klasik.",
                "https://covers.openlibrary.org/b/isbn/9781451673319-L.jpg",
                "distopya,sansür,kitap,özgürlük", 4.4),

            Models.Entities.Book.Create("Solaris", "Stanisław Lem", "Bilim Kurgu", 10,
                "Gizemli bir okyanusla kaplı gezegende insan bilincinin sınırlarını sorgulayan felsefi bilim kurgu.",
                "https://covers.openlibrary.org/b/isbn/9780156027601-L.jpg",
                "uzay,bilinç,felsefe,yabancı", 4.3),

            Models.Entities.Book.Create("Yüzüklerin Efendisi: Yüzük Kardeşliği", "J.R.R. Tolkien", "Fantastik", 15,
                "Orta Dünya'da geçen destansı maceranın ilk kitabı.",
                "https://covers.openlibrary.org/b/isbn/9780618640157-L.jpg",
                "macera,fantezi,epik,savaş", 4.7),

            Models.Entities.Book.Create("Hobbit", "J.R.R. Tolkien", "Fantastik", 10,
                "Bilbo Baggins'in beklenmedik yolculuğu.",
                "https://covers.openlibrary.org/b/isbn/9780547928227-L.jpg",
                "macera,fantezi,ejderha,hazine", 4.6),

            Models.Entities.Book.Create("Harry Potter ve Felsefe Taşı", "J.K. Rowling", "Fantastik", 12,
                "Büyücülük dünyasının kapılarını aralayan efsanevi serinin ilk kitabı.",
                "https://covers.openlibrary.org/b/isbn/9780747532743-L.jpg",
                "büyü,macera,okul,dostluk", 4.7),

            Models.Entities.Book.Create("Narnia Günlükleri", "C.S. Lewis", "Fantastik", 10,
                "Sihirli bir gardırobun ardındaki büyülü dünya.",
                "https://covers.openlibrary.org/b/isbn/9780064471190-L.jpg",
                "fantezi,macera,çocuk,büyü", 4.4),

            Models.Entities.Book.Create("Suç ve Ceza", "Fyodor Dostoyevski", "Klasik", 14,
                "Raskolnikov'un psikolojik çöküşü ve ahlaki sorgulaması.",
                "https://covers.openlibrary.org/b/isbn/9780486415871-L.jpg",
                "psikoloji,ahlak,suç,rusya", 4.6),

            Models.Entities.Book.Create("Sefiller", "Victor Hugo", "Klasik", 15,
                "Jean Valjean'ın kurtuluş arayışı.",
                "https://covers.openlibrary.org/b/isbn/9780451419439-L.jpg",
                "adalet,merhamet,devrim,toplum", 4.5),

            Models.Entities.Book.Create("Savaş ve Barış", "Lev Tolstoy", "Klasik", 15,
                "Napolyon savaşları döneminde Rusya'yı anlatan başyapıt.",
                "https://covers.openlibrary.org/b/isbn/9780199232765-L.jpg",
                "savaş,tarih,aşk,toplum", 4.5),

            Models.Entities.Book.Create("Karamazov Kardeşler", "Fyodor Dostoyevski", "Klasik", 15,
                "İnanç, şüphe ve ahlak üzerine derinlikli bir aile dramı.",
                "https://covers.openlibrary.org/b/isbn/9780374528379-L.jpg",
                "aile,inanç,ahlak,felsefe", 4.7),

            Models.Entities.Book.Create("Don Kişot", "Miguel de Cervantes", "Klasik", 15,
                "Modern romanın başlangıcı kabul edilen ölümsüz macera.",
                "https://covers.openlibrary.org/b/isbn/9780060934347-L.jpg",
                "macera,hiciv,şövalye,hayal", 4.3),

            Models.Entities.Book.Create("Kürk Mantolu Madonna", "Sabahattin Ali", "Roman", 8,
                "Berlin'deki derin ve trajik bir aşkın romanı.",
                "https://covers.openlibrary.org/b/isbn/9786051850481-L.jpg",
                "aşk,berlin,sanat,yalnızlık", 4.4),

            Models.Entities.Book.Create("İnce Memed", "Yaşar Kemal", "Roman", 12,
                "Anadolu'da zulme karşı başkaldırının destansı hikayesi.",
                "https://covers.openlibrary.org/b/isbn/9789750709586-L.jpg",
                "anadolu,başkaldırı,eşkıya,doğa", 4.7),

            Models.Entities.Book.Create("Çalıkuşu", "Reşat Nuri Güntekin", "Roman", 12,
                "Feride'nin Anadolu'daki öğretmenlik macerası.",
                "https://covers.openlibrary.org/b/isbn/9789754580662-L.jpg",
                "anadolu,öğretmen,aşk,fedakarlık", 4.5),

            Models.Entities.Book.Create("Tutunamayanlar", "Oğuz Atay", "Roman", 15,
                "Türk edebiyatının postmodern başyapıtı.",
                "https://covers.openlibrary.org/b/isbn/9789754702033-L.jpg",
                "postmodern,aydın,yabancılaşma,toplum", 4.6),

            Models.Entities.Book.Create("Saatleri Ayarlama Enstitüsü", "Ahmet Hamdi Tanpınar", "Roman", 12,
                "Doğu-Batı arasında sıkışan Türk toplumunun hicivli portresi.",
                "https://covers.openlibrary.org/b/isbn/9789759038274-L.jpg",
                "hiciv,zaman,doğu-batı,bürokratik", 4.5),

            Models.Entities.Book.Create("Yaban", "Yakup Kadri Karaosmanoğlu", "Klasik", 9,
                "Kurtuluş Savaşı sırasında Anadolu'da yabancılaşma.",
                "https://covers.openlibrary.org/b/isbn/9789754585483-L.jpg",
                "savaş,anadolu,yabancılaşma,köy", 4.1),

            Models.Entities.Book.Create("Sinekli Bakkal", "Halide Edib Adıvar", "Klasik", 12,
                "İstanbul'un renkli mahallelerinden yükselen bir kadın hikayesi.",
                "https://covers.openlibrary.org/b/isbn/9789750503320-L.jpg",
                "istanbul,kadın,toplum,gelenek", 4.2),

            Models.Entities.Book.Create("Nutuk", "Mustafa Kemal Atatürk", "Tarih", 13,
                "Türkiye Cumhuriyeti'nin kuruluş sürecini Atatürk'ün kaleminden aktaran tarihi belge.",
                "https://covers.openlibrary.org/b/isbn/9789944888585-L.jpg",
                "cumhuriyet,kurtuluş,tarih,liderlik", 4.9),

            Models.Entities.Book.Create("Sapiens", "Yuval Noah Harari", "Tarih", 14,
                "İnsan türünün 70.000 yıllık serüveni.",
                "https://covers.openlibrary.org/b/isbn/9780062316097-L.jpg",
                "insanlık,evrim,tarih,toplum", 4.5),

            Models.Entities.Book.Create("Homo Deus", "Yuval Noah Harari", "Tarih", 13,
                "İnsanlığın geleceğine dair cesur öngörüler.",
                "https://covers.openlibrary.org/b/isbn/9780062464316-L.jpg",
                "gelecek,teknoloji,insanlık,yapay-zeka", 4.3),

            Models.Entities.Book.Create("Sofie'nin Dünyası", "Jostein Gaarder", "Felsefe", 11,
                "Felsefe tarihine sürükleyici bir yolculuk.",
                "https://covers.openlibrary.org/b/isbn/9780374530716-L.jpg",
                "felsefe,düşünce,tarih,bilgelik", 4.2),

            Models.Entities.Book.Create("Böyle Buyurdu Zerdüşt", "Friedrich Nietzsche", "Felsefe", 12,
                "Nietzsche'nin en iddialı ve etkileyici eseri.",
                "https://covers.openlibrary.org/b/isbn/9780140441185-L.jpg",
                "felsefe,varoluş,üstinsan,irade", 4.4),

            Models.Entities.Book.Create("Yabancı", "Albert Camus", "Felsefe", 8,
                "Varoluşçu felsefenin en çarpıcı romanı.",
                "https://covers.openlibrary.org/b/isbn/9780679720201-L.jpg",
                "absürt,varoluş,yalnızlık,ölüm", 4.3),

            Models.Entities.Book.Create("Dönüşüm", "Franz Kafka", "Psikoloji", 6,
                "Gregor Samsa bir sabah uyandığında kendini dev bir böceğe dönüşmüş bulur.",
                "https://covers.openlibrary.org/b/isbn/9780553213690-L.jpg",
                "absürt,yabancılaşma,aile,dönüşüm", 4.4),

            Models.Entities.Book.Create("İkna Sanatı", "Robert Cialdini", "Psikoloji", 12,
                "İkna ve etkileme psikolojisinin temel eseri.",
                "https://covers.openlibrary.org/b/isbn/9780061241895-L.jpg",
                "ikna,psikoloji,davranış,etkileme", 4.3),

            Models.Entities.Book.Create("Sherlock Holmes", "Arthur Conan Doyle", "Polisiye", 10,
                "Dünyanın en ünlü dedektifinin maceraları.",
                "https://covers.openlibrary.org/b/isbn/9780140439083-L.jpg",
                "dedektif,gizem,londra,mantık", 4.6),

            Models.Entities.Book.Create("Ve Perde İndi", "Agatha Christie", "Polisiye", 9,
                "Agatha Christie'nin ustalıklı gizem romanı.",
                "https://covers.openlibrary.org/b/isbn/9780062073563-L.jpg",
                "cinayet,gizem,dedektif,sürpriz", 4.3),

            Models.Entities.Book.Create("Simyacı", "Paulo Coelho", "Roman", 8,
                "Kişisel efsanesini gerçekleştirmek için yola çıkan bir çobanın alegorik yolculuğu.",
                "https://covers.openlibrary.org/b/isbn/9780062315007-L.jpg",
                "yolculuk,kader,rüya,macera", 4.0),

            Models.Entities.Book.Create("Küçük Prens", "Antoine de Saint-Exupéry", "Roman", 6,
                "Büyüklerin unuttuğu çocuksu bilgelik.",
                "https://covers.openlibrary.org/b/isbn/9780156012195-L.jpg",
                "çocuk,bilgelik,dostluk,hayal", 4.6),

            Models.Entities.Book.Create("Uçurtma Avcısı", "Khaled Hosseini", "Roman", 13,
                "Afganistan'da dostluk, ihanet ve kefaret hikayesi.",
                "https://covers.openlibrary.org/b/isbn/9781594631931-L.jpg",
                "dostluk,savaş,kefaret,afganistan", 4.5),

            Models.Entities.Book.Create("Beyaz Diş", "Jack London", "Macera", 10,
                "Vahşi doğada hayatta kalma mücadelesi.",
                "https://covers.openlibrary.org/b/isbn/9780486269689-L.jpg",
                "doğa,hayvan,hayatta-kalma,macera", 4.3),

            Models.Entities.Book.Create("Yeraltından Notlar", "Fyodor Dostoyevski", "Klasik", 7,
                "Modern yabancılaşma ve varoluşçuluğun öncü eseri.",
                "https://covers.openlibrary.org/b/isbn/9780679734529-L.jpg",
                "yabancılaşma,psikoloji,varoluş,toplum", 4.3),

            Models.Entities.Book.Create("Hayvan Çiftliği", "George Orwell", "Klasik", 7,
                "Totaliter rejimleri alegorik olarak eleştiren başyapıt.",
                "https://covers.openlibrary.org/b/isbn/9780451526342-L.jpg",
                "alegori,totalitarizm,devrim,özgürlük", 4.5),

            Models.Entities.Book.Create("Bülbülü Öldürmek", "Harper Lee", "Klasik", 12,
                "Irkçılık ve adalet temalı Amerikan edebiyat klasiği.",
                "https://covers.openlibrary.org/b/isbn/9780446310789-L.jpg",
                "adalet,ırkçılık,çocukluk,ahlak", 4.6),

            Models.Entities.Book.Create("Başını Vermeyen Şehit", "Ömer Seyfettin", "Klasik", 6,
                "Türk hikayeciliğinin öncü kalemi.",
                "https://covers.openlibrary.org/b/isbn/9789944880046-L.jpg",
                "hikaye,vatan,kahramanlık,türk", 4.1),

            Models.Entities.Book.Create("Ateşten Gömlek", "Halide Edib Adıvar", "Tarih", 10,
                "Kurtuluş Savaşı'nın destansı romanı.",
                "https://covers.openlibrary.org/b/isbn/9786051413518-L.jpg",
                "savaş,kurtuluş,vatan,kadın", 4.3),

            Models.Entities.Book.Create("Siddhartha", "Hermann Hesse", "Felsefe", 8,
                "Aydınlanma arayışında bir Hint gencinin yolculuğu.",
                "https://covers.openlibrary.org/b/isbn/9780553208849-L.jpg",
                "aydınlanma,doğu,ruhani,yolculuk", 4.4),

            Models.Entities.Book.Create("Kırmızı Pazartesi", "Gabriel García Márquez", "Roman", 7,
                "Bir cinayetin kronolojik olarak anlatıldığı büyülü gerçekçi başyapıt.",
                "https://covers.openlibrary.org/b/isbn/9781400034710-L.jpg",
                "cinayet,kader,toplum,latin-amerika", 4.3),

            Models.Entities.Book.Create("Vadideki Zambak", "Honoré de Balzac", "Klasik", 10,
                "Platonik aşkın en güzel anlatıldığı Fransız edebiyat klasiği.",
                "https://covers.openlibrary.org/b/isbn/9780140443004-L.jpg",
                "aşk,platonik,fransa,aristokrasi", 4.1),
        ];
    }

    private static List<BookPage> GeneratePages(Guid bookId, int totalPages)
    {
        var pages = new List<BookPage>();

        string[][] turkishParagraphs =
        [
            [
                "Güneş, ufuk çizgisinin ardından yavaşça yükselirken, şehrin sokaklarında henüz bir kıpırdanma başlamamıştı. Sabahın ilk ışıkları, eski taş binaların cephelerinde dans ediyor, gece boyunca biriken çiğ taneleri pırıl pırıl parlıyordu. Uzaktan bir horozun ötüşü duyuldu; ardından bir başka, sonra bir başkası daha. Şehir yavaş yavaş uyanmaya başlıyordu.",
                "Dar sokakların arasından süzülen rüzgâr, açık pencerelerden içeri dalıyor, perdeleri hafifçe oynatıyordu. Bir fırıncı, taze ekmeğin mis gibi kokusunu sokaklara yayarken, bir çocuk koşarak okulun yolunu tutuyordu. Hayat, her sabah olduğu gibi, alışılmış düzenine kavuşuyordu.",
                "Kahvaltı sofrasında aileler bir araya geliyor, günün planları konuşuluyordu. Çay bardaklarının buharı yükseliyor, taze peynirin ve zeytinin kokusu yemek odasını dolduruyordu."
            ],
            [
                "Kütüphanenin tozlu raflarında yüzlerce yıllık kitaplar, sessizce bekleşiyordu. Her biri farklı bir dünyanın kapısını aralıyor, farklı bir hikayenin anahtarını sunuyordu. Eski ciltlerin deri kaplamaları zamanla yıpranmış, sayfaları sararmıştı ama taşıdıkları bilgi hâlâ taptaze duruyordu.",
                "Masanın üzerinde açık duran kitap, okurunu bambaşka bir zamana götürüyordu. Satırlar arasında kaybolmak, gerçek dünyadan uzaklaşmak ve hayal gücünün sınırlarını zorlamak ne güzel bir şeydi.",
                "Pencereden süzülen ışık, açık kitabın sayfalarına düşüyor, kelimeleri aydınlatıyordu. Dışarıda hayat koşuşturma içinde devam ederken, burada zaman durmuş gibiydi."
            ],
            [
                "Denizin maviliği, gökyüzüyle birleştiği noktada kayboluyordu. Dalgalar, sahile vurup geri çekilirken ritmik bir melodi oluşturuyorlardı. Kumların üzerinde yürümek, ayak izlerinin suda kaybolmasını izlemek, insana zamanın akışını hatırlatıyordu.",
                "Balıkçı tekneleri limandan ayrılırken, martılar onlara eşlik ediyordu. Denizin tuzlu kokusu rüzgârla birlikte kıyıya taşınıyor, insanların ciğerlerini dolduruyordu.",
                "Akşam olduğunda güneş, denizin içine batıyor gibi görünürdü. Gökyüzü turuncu, pembe ve mor tonlarına bürünür, bulutlar ateşten tablolar çizerdi."
            ],
            [
                "Dağların eteklerinde küçük bir köy uzanıyordu. Taş evlerin bacalarından yükselen duman, soğuk havada beyaz bulutlar oluşturuyordu. Köyün meydanındaki çınar ağacı yüzyıllardır oradaydı.",
                "Köylüler sabahın erken saatlerinde tarlalara çıkar, toprağı eker ve sularlardı. Doğayla uyum içinde yaşamak, onların en büyük bilgeliğiydi.",
                "Akşamüstü köy kahvesinde toplanan yaşlılar, çay içerken eski günleri anlatırlardı. Her hikaye bir ders, her anı bir hazine idi."
            ],
            [
                "Yıldızlı bir gece, evrenin sonsuzluğunu hatırlatan en güzel manzaraydı. Teleskopla bakıldığında her yıldızın kendi hikayesi olduğu görülürdü.",
                "Bilim insanları yüzyıllardır gökyüzünü inceliyorlardı. Her yeni keşif, bilinmeyenin sınırlarını biraz daha genişletiyordu.",
                "Gece yarısı, sessizlik çöktüğünde, uzayın derinliklerinden gelen kozmik radyasyonlar yeryüzüne ulaşıyordu. Bu görünmez dalgalar, evrenin başlangıcından kalma mesajlar taşıyordu."
            ],
            [
                "Müzik, insan ruhunun en derin köşelerine ulaşabilen evrensel bir dildi. Bir kemanın tiz sesi, bir piyanonun derin notaları veya bir neyin hüzünlü ezgisi; her biri farklı duyguları uyandırıyordu.",
                "Konser salonunda ışıklar karardığında, orkestra şefinin batonunu kaldırmasıyla birlikte büyü başlıyordu. Yüzlerce farklı enstrümandan çıkan sesler tek bir uyum içinde birleşiyordu.",
                "Sokak müzisyeni, gitar çalarak geçimini sağlıyordu. Ama onun için müzik sadece bir geçim aracı değil, yaşamın ta kendisiydi."
            ],
            [
                "Sonbahar yaprakları, rüzgârın elinde dans ederek yere düşüyordu. Sarı, turuncu ve kızıl tonlarındaki bu doğal tablo, her yıl tekrarlanan ama asla sıradanlaşmayan bir gösteriydi.",
                "Bir bank üzerinde oturan yaşlı adam, elindeki gazetenin üzerinden gözlüklerinin ardına bakıyordu. Çevresindeki değişimi izliyor, zamanın akıp gidişini sessizce kabul ediyordu.",
                "Çocuklar yaprak yığınlarının arasında oynuyor, kahkahalarla gülüyorlardı. Onlar için sonbahar, rengarenk bir oyun alanıydı."
            ],
            [
                "Bilimin ilerlemesi, insanlığın en büyük başarılarından biriydi. Ateşin keşfinden uzay yolculuğuna kadar uzanan bu yolculuk, merakın ve azmin ürünüydü.",
                "Laboratuvarda çalışan araştırmacı, mikroskobun altındaki hücreleri inceliyordu. Her yeni gözlem, hayatın karmaşıklığını bir kez daha ortaya koyuyordu.",
                "Teknolojinin hızla gelişmesi, toplumları derinden etkiliyor ve dönüştürüyordu. İletişim araçları mesafeleri kısaltmış, bilgiye erişim demokratikleşmişti."
            ],
            [
                "Felsefe, insanın varoluşunu, bilgiyi ve ahlakı sorgulayan kadim bir disiplindi. Sokrates'in sözleri felsefi düşüncenin temelini oluşturuyordu.",
                "Doğu ve Batı felsefesi, farklı yaklaşımlarla aynı soruları cevaplamaya çalışıyordu. Bir tarafta analitik düşünce, diğer tarafta bütüncül bakış vardı.",
                "Modern dünyada felsefenin rolü tartışılsa da, etik sorunlar giderek daha karmaşık hale geliyordu. Yapay zekâ ve biyoteknoloji yeni felsefi soruları beraberinde getiriyordu."
            ],
            [
                "Sanatçının atölyesi, yaratıcılığın sığınağıydı. Boya lekeleri, fırçalar ve tuvaller arasında yeni dünyalar doğuyordu. Her fırça darbesi bir duygunun ifadesiydi.",
                "Resim yapmak, sadece teknik bir beceri değil, aynı zamanda bir meditasyondu. Sanatçı tuvale bakarken, aslında kendi iç dünyasına bakıyordu.",
                "Sergi açılışında insanlar tabloların önünde duraksıyor, her birinde farklı şeyler görüyorlardı. İşte sanatın gerçek gücü buradaydı."
            ]
        ];

        for (int i = 1; i <= totalPages; i++)
        {
            var paragraphSet = turkishParagraphs[(i - 1) % turkishParagraphs.Length];
            var content = string.Join("\n\n", paragraphSet);
            pages.Add(BookPage.Create(bookId, i, content));
        }

        return pages;
    }
}
