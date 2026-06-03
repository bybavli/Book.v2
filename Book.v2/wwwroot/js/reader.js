
import { api } from './api.js';
import {
    debounce,           // Çok sık tetiklenen fonksiyonları yavaşlatmak (geciktirmek) için kullanılır.
    showToast,          // Ekranda küçük bildirim mesajları (toast) göstermek için kullanılır.
    getQueryParam,      // URL'den parametre okumak için kullanılır (örneğin bookId).
    escapeHtml,         // HTML enjeksiyon (XSS) saldırılarını engellemek için metinleri güvenli hale getirir.
    calcProgressPercent // Okuma ilerleme yüzdesini hesaplamak için kullanılır.
} from './utils.js';



let bookId = null;            // Okunmakta olan kitabın eşsiz kimliği (ID)
let bookDetail = null;        // Kitabın başlık, yazar gibi detay bilgilerini tutan obje
let pages = [];               // Kitabın sayfalarının bulunduğu dizi
let pageFlip = null;          // St.PageFlip kütüphanesinin (sayfa çevirme animasyonları) örneği
let currentPageIndex = 0;     // Kullanıcının şu an bulunduğu sayfanın indeksi
let totalRenderedPages = 0;   // Kapaklar dahil ekrana çizdirilen toplam sayfa sayısı



document.addEventListener('DOMContentLoaded', async () => {

    bookId = getQueryParam('bookId');

    if (!bookId) {
        showError('Kitap bulunamadı. Lütfen ana sayfaya dönün.');
        return;
    }

    initHeaderScroll();

    initKeyboard();

    await loadBookAndRender();
});



async function loadBookAndRender() {

    showLoading();

    try {


        const [detail, pagesData] = await Promise.all([
            api.getBookDetail(bookId),
            api.getBookPages(bookId),
        ]);

        bookDetail = detail;

        pages = (Array.isArray(pagesData) ? pagesData : []).sort((a, b) => {
            return (a.pageNumber ?? a.PageNumber ?? 0) - (b.pageNumber ?? b.PageNumber ?? 0);
        });

        if (pages.length === 0) {
            showError('Bu kitap için sayfa bulunamadı.');
            return;
        }

        const title = bookDetail?.title ?? bookDetail?.Title ?? 'Kitap';
        document.title = `${title} — Book`;

        renderReaderShell();

        let startPage = 0;
        try {
            const progress = await api.getProgress(bookId);
            startPage = progress?.currentPage ?? progress?.CurrentPage ?? 0;
        } catch (_) { 

        }




        createPageFlip(startPage);

        if (startPage > 0) {
            showToast('Kaldığınız yerden devam ediliyor', 'info');
        }

    } catch (err) {
        console.error('Kitap yükleme hatası:', err);
        showError('Kitap yüklenirken hata oluştu. Sunucu bağlantınızı kontrol edin.');
    }
}



function renderReaderShell() {
    const container = document.getElementById('reader-container');
    if (!container) return;

    const title = bookDetail?.title ?? bookDetail?.Title ?? 'Kitap';


    container.innerHTML = `
        <div class="reader-header">
            <button class="reader-back-btn" onclick="window.location.href='index.html?v=999'">
                <svg viewBox="0 0 24 24"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
                <span>Geri</span>
            </button>
            <div class="reader-book-title">${escapeHtml(title)}</div>
            <div class="reader-progress-badge" id="reader-progress-badge">%0</div>
        </div>

        <div class="reader-book-wrapper">
            <!-- Sol taraftaki ok ipucu -->
            <div class="page-turn-hint page-turn-hint--left">
                <svg viewBox="0 0 24 24"><polyline points="15 18 9 12 15 6"/></svg>
            </div>
            <!-- Kitap sayfalarının çizileceği ana bölüm -->
            <div id="book-container"></div>
            <!-- Sağ taraftaki ok ipucu -->
            <div class="page-turn-hint page-turn-hint--right">
                <svg viewBox="0 0 24 24"><polyline points="9 18 15 12 9 6"/></svg>
            </div>
        </div>

        <div class="reader-controls">
            <!-- Önceki Sayfa Butonu -->
            <button class="reader-nav-btn" id="prev-btn" title="Önceki sayfa">
                <svg viewBox="0 0 24 24"><polyline points="15 18 9 12 15 6"/></svg>
            </button>

            <!-- Ortadaki İlerleme Çubuğu ve Sayfa Sayacı -->
            <div class="reader-page-info">
                <div class="reader-page-counter" id="page-counter">Sayfa <strong>0</strong> / 0</div>
                <input type="range" class="reader-slider" id="page-slider" min="0" max="0" value="0">
            </div>

            <!-- Sonraki Sayfa Butonu -->
            <button class="reader-nav-btn" id="next-btn" title="Sonraki sayfa">
                <svg viewBox="0 0 24 24"><polyline points="9 18 15 12 9 6"/></svg>
            </button>

            <!-- Tam Ekran Butonu -->
            <button class="reader-fullscreen-btn" id="fullscreen-btn" title="Tam ekran">
                <svg viewBox="0 0 24 24" id="fullscreen-icon">
                    <polyline points="15 3 21 3 21 9"/><polyline points="9 21 3 21 3 15"/>
                    <line x1="21" y1="3" x2="14" y2="10"/><line x1="3" y1="21" x2="10" y2="14"/>
                </svg>
                <span id="fullscreen-label">Tam Ekran</span>
            </button>
        </div>
    `;

    document.getElementById('prev-btn')?.addEventListener('click', () => pageFlip?.flipPrev());
    document.getElementById('next-btn')?.addEventListener('click', () => pageFlip?.flipNext());
    document.getElementById('page-slider')?.addEventListener('input', onSliderChange);
    document.getElementById('fullscreen-btn')?.addEventListener('click', toggleFullscreen);
}



function createPageFlip(startPage = 0) {
    const container = document.getElementById('book-container');
    if (!container) return;

    if (pageFlip) {
        try { pageFlip.destroy(); } catch (_) {}
        pageFlip = null;
    }

    container.innerHTML = '';

    const pageElements = buildPages();
    totalRenderedPages = pageElements.length;

    const safeStartPage = Math.min(startPage, Math.max(0, totalRenderedPages - 1));

    const dim = calcDimensions();

    pageFlip = new St.PageFlip(container, {
        width: dim.w,                 // Sayfa genişliği
        height: dim.h,                // Sayfa yüksekliği
        size: 'stretch',              // Boyutlandırma türü
        minWidth: 280,                // Minimum sayfa genişliği
        maxWidth: 1200,               // Maksimum sayfa genişliği (Tam ekran için artırıldı)
        minHeight: 400,
        maxHeight: 1600,              // Maksimum sayfa yüksekliği (Tam ekran için artırıldı)
        showCover: true,              // Sert kapak efektini aç
        drawShadow: true,             // Sayfa kıvrılırken gölge çiz
        flippingTime: 700,            // Çevrilme animasyonu süresi (ms)
        useMouseEvents: true,         // Fareyle sürükleyip çevirme
        usePortrait: true,            // Dikey ekranlarda (mobilde) tek sayfa gösterme modu
        startZIndex: 0,
        autoSize: true,               // Otomatik boyutlandırma
        maxShadowOpacity: 0.3,
        mobileScrollSupport: true,
        clickEventForward: false,
        swipeDistance: 30,
        startPage: safeStartPage      // BUG FİX: Kitap doğrudan istenilen sayfadan başlatılır (animasyonsuz geçiş sağlar).
    });

    pageFlip.loadFromHTML(pageElements);

    pageFlip.on('flip', (e) => {
        currentPageIndex = e.data; // Yeni sayfanın indeksini al
        syncUI();                  // Arayüzdeki (UI) sayfa sayılarını ve ilerleme çubuğunu güncelle
        debouncedSave();           // Okunan sayfayı veritabanına kaydet (Sunucuyu yormamak için gecikmeli kaydeder)
    });

    pageFlip.on('changeState', () => syncUI());

    const slider = document.getElementById('page-slider');
    if (slider) {
        slider.max = totalRenderedPages - 1;
        slider.value = safeStartPage;
    }

    syncUI();
}



function buildPages() {
    const elems = [];
    const title = bookDetail?.title ?? bookDetail?.Title ?? 'Kitap';
    const author = bookDetail?.author ?? bookDetail?.Author ?? '';

    const cover = document.createElement('div');
    cover.className = 'page page--cover'; // Kapak için özel CSS sınıfı
    cover.innerHTML = `
        <div class="cover-content">
            <div class="cover-icon">
                <svg viewBox="0 0 24 24">
                    <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
                    <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>
                </svg>
            </div>
            <div class="cover-divider"></div>
            <div class="cover-title">${escapeHtml(title)}</div>
            <div class="cover-author">${escapeHtml(author)}</div>
        </div>
    `;
    elems.push(cover);

    pages.forEach((page, idx) => {

        const content = page.content ?? page.Content ?? page.text ?? page.Text ?? '';
        const num = page.pageNumber ?? page.PageNumber ?? (idx + 1);

        let html = content;


        if (!content.includes('<p>') && !content.includes('<div>')) {
            html = content.split(/\n\n+/).filter(p => p.trim()).map(p => `<p>${p.trim()}</p>`).join('');
            if (!html) html = `<p>${content}</p>`;
        }

        const el = document.createElement('div');

        el.className = `page${idx === 0 ? ' page--first' : ''}`;
        el.dataset.pageNumber = num;
        
        el.innerHTML = `
            <div class="page-inner">
                <div class="page-content">${html}</div>
                <div class="page-number">${num}</div>
            </div>
        `;
        elems.push(el);
    });

    const back = document.createElement('div');
    back.className = 'page page--cover';
    back.innerHTML = `
        <div class="cover-content">
            <div class="cover-icon">
                <svg viewBox="0 0 24 24">
                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                    <polyline points="22 4 12 14.01 9 11.01"/>
                </svg>
            </div>
            <div class="cover-divider"></div>
            <div class="cover-title" style="font-size:1.3rem;">Son</div>
            <div class="cover-author">Okuma tamamlandı</div>
        </div>
    `;
    elems.push(back);

    return elems; // Oluşturulan tüm HTML sayfalarını dizi olarak döndürüyoruz.
}



function calcDimensions() {
    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const isFs = document.body.classList.contains('is-fullscreen');

    let w, h;

    if (vw <= 480) {
        w = Math.min(vw - 32, 340);
    } 

    else if (vw <= 768) {
        w = Math.min(vw - 64, 420);
    } 

    else {

        w = isFs ? Math.min((vw / 2) - 100, 800) : 460;
    }

    h = Math.round(w * 1.4);



    const paddingY = isFs ? 120 : 220;
    const maxH = vh - paddingY;
    
    if (h > maxH && maxH > 300) {
        h = maxH;
        w = Math.round(h / 1.4);
    }

    return { w, h };
}



function syncUI() {
    if (!pageFlip) return;

    const idx = pageFlip.getCurrentPageIndex();
    currentPageIndex = idx;

    const counter = document.getElementById('page-counter');
    if (counter) {
        counter.innerHTML = `Sayfa <strong>${Math.max(0, idx)}</strong> / ${totalRenderedPages - 1}`;
    }

    const slider = document.getElementById('page-slider');
    if (slider) slider.value = idx;

    const badge = document.getElementById('reader-progress-badge');
    if (badge) {
        badge.textContent = `%${calcProgressPercent(Math.max(0, idx - 1), pages.length)}`;
    }

    const prev = document.getElementById('prev-btn');
    const next = document.getElementById('next-btn');
    if (prev) prev.disabled = idx <= 0;
    if (next) next.disabled = idx >= totalRenderedPages - 1;
}

function onSliderChange(e) {
    const target = parseInt(e.target.value, 10);

    if (pageFlip && !isNaN(target)) {
        pageFlip.turnToPage(target);
    }
}



function initKeyboard() {
    document.addEventListener('keydown', (e) => {
        if (!pageFlip) return;

        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

        switch (e.key) {
            case 'ArrowLeft': // Sol Ok: Önceki sayfa
                e.preventDefault();
                pageFlip.flipPrev();
                break;
            case 'ArrowRight': // Sağ Ok: Sonraki sayfa
                e.preventDefault();
                pageFlip.flipNext();
                break;
            case 'Home': // Home tuşu: İlk sayfaya (Kapağa) dön
                e.preventDefault();
                pageFlip.turnToPage(0);
                break;
            case 'End': // End tuşu: Son sayfaya (Arka Kapağa) git
                e.preventDefault();
                pageFlip.turnToPage(totalRenderedPages - 1);
                break;
            case 'Escape': // ESC tuşu: Tam ekrandan çık
                if (document.fullscreenElement) {
                    document.exitFullscreen?.();
                }
                break;
        }
    });
}



const debouncedSave = debounce(async () => {
    if (!bookId || !pageFlip) return;

    const contentPage = Math.max(0, pageFlip.getCurrentPageIndex() - 1);
    try {

        await api.updateProgress(bookId, contentPage, pages.length);
    } catch (_) { 
         
    }
}, 2000); // 2000ms = 2 Saniye (En son çevirmeden 2 saniye sonra veritabanına kaydeder)



function initHeaderScroll() {
    const header = document.querySelector('.header');
    if (!header) return;
    let ticking = false;
    window.addEventListener('scroll', () => {
        if (!ticking) {
            requestAnimationFrame(() => {
                header.classList.toggle('scrolled', window.scrollY > 10);
                ticking = false;
            });
            ticking = true;
        }
    });
}

function showLoading() {
    const c = document.getElementById('reader-container');
    if (c) {
        c.innerHTML = `
            <div class="reader-loading">
                <div class="reader-loading-book">
                    <svg viewBox="0 0 24 24">
                        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
                        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>
                    </svg>
                </div>
                <div class="reader-loading-text">Kitap yükleniyor…</div>
                <div class="spinner"></div>
            </div>
        `;
    }
}

function showError(msg) {
    const c = document.getElementById('reader-container');
    if (c) {
        c.innerHTML = `
            <div class="reader-loading">
                <div class="reader-loading-book">
                    <svg viewBox="0 0 24 24">
                        <circle cx="12" cy="12" r="10"/>
                        <line x1="15" y1="9" x2="9" y2="15"/>
                        <line x1="9" y1="9" x2="15" y2="15"/>
                    </svg>
                </div>
                <div class="reader-loading-text">${msg}</div>
                <a href="index.html?v=999" class="btn btn-primary" style="margin-top:16px;">Ana Sayfaya Dön</a>
            </div>
        `;
    }
}



function toggleFullscreen() {
    if (!document.fullscreenElement) {

        document.documentElement.requestFullscreen().catch(err => {
            console.error(`Tam ekrana geçilemedi: ${err.message}`);
        });
    } else {

        if (document.exitFullscreen) {
            document.exitFullscreen();
        }
    }
}

window._toggleFullscreen = toggleFullscreen;

document.addEventListener('fullscreenchange', () => {
    const isFs = !!document.fullscreenElement;

    document.body.classList.toggle('is-fullscreen', isFs);

    const icon = document.getElementById('fullscreen-icon');
    if (icon) {
        icon.innerHTML = isFs
            ? '<polyline points="4 14 10 14 10 20"/><polyline points="20 10 14 10 14 4"/><line x1="14" y1="10" x2="21" y2="3"/><line x1="3" y1="21" x2="10" y2="14"/>'
            : '<polyline points="15 3 21 3 21 9"/><polyline points="9 21 3 21 3 15"/><line x1="21" y1="3" x2="14" y2="10"/><line x1="3" y1="21" x2="10" y2="14"/>';
    }

    const label = document.getElementById('fullscreen-label');
    if (label) label.textContent = isFs ? 'Küçült' : 'Tam Ekran';



    setTimeout(() => {
        if (pageFlip) {
            const currentIdx = pageFlip.getCurrentPageIndex();
            createPageFlip(currentIdx);
        }
    }, 150);
});



let _resizeTimer;

window.addEventListener('resize', () => {
    clearTimeout(_resizeTimer);
    _resizeTimer = setTimeout(() => {
        if (!pageFlip) return;





        pageFlip.update();
    }, 200);
});
