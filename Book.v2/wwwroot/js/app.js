

import { api } from './api.js?v=14';
import {
    showToast,
    createSkeleton,
    escapeHtml,
    calcProgressPercent,
    truncate,
    BOOK_ICON_SVG,
    CHEVRON_RIGHT_SVG,
} from './utils.js?v=14';


window.handleImgError = function(img, title) {
    replaceBrokenImg(img, title);
};
window.handleImgLoad = function(img, title) {

    if (img.naturalWidth < 50 || img.naturalHeight < 50) {
        replaceBrokenImg(img, title);
    }
};
function replaceBrokenImg(img, title) {
    const initial = (title || '?')[0].toUpperCase();
    const colors = [
        ['#124330','#1a6b4a'], ['#6b2d5b','#9b4d8b'], ['#2d4a6b','#4a7aab'],
        ['#6b4a2d','#ab7a4a'], ['#2d6b5b','#4aab9b'], ['#4a2d6b','#7a4aab'],
        ['#5b2d2d','#8b4a4a'], ['#2d5b3d','#4a8b5d']
    ];
    const pair = colors[initial.charCodeAt(0) % colors.length];
    const placeholder = document.createElement('div');
    placeholder.className = 'cover-fallback';
    placeholder.style.cssText = `
        width:100%;height:100%;display:flex;align-items:center;justify-content:center;
        background:linear-gradient(145deg,${pair[0]},${pair[1]});
        color:rgba(255,255,255,0.9);font-size:2.2rem;font-weight:700;font-family:var(--font-ui);
        letter-spacing:-0.02em;text-shadow:0 2px 8px rgba(0,0,0,0.2);
    `;
    placeholder.textContent = initial;
    img.parentNode.replaceChild(placeholder, img);
}
function coverImg(url, title, lazy) {
    const t = escapeHtml(title).replace(/'/g, "\\'");
    return `<img src="${escapeHtml(url)}" alt="${escapeHtml(title)}"${lazy ? ' loading="lazy"' : ''} onerror="handleImgError(this,'${t}')" onload="handleImgLoad(this,'${t}')">`;
}


let allBooks = [];
let readingList = [];
let readingListBookIds = new Set();
let similarBooks = [];
let selectedSimilarBookId = null;
let currentSearchQuery = '';


const continueReadingSection = document.getElementById('continue-reading');
const continueReadingGrid = document.getElementById('continue-reading-grid');
const discoverGrid = document.getElementById('discover-grid');
const recommendationsGrid = document.getElementById('recommendations-grid');
const searchInput = document.getElementById('search-input');
const discoverSection = document.getElementById('discover');
const recommendationsSection = document.getElementById('recommendations');


document.addEventListener('DOMContentLoaded', init);

async function init() {

    const userId = localStorage.getItem('kitapoku_user_id');
    if (!userId) {

        window.location.replace('/auth.html?v=999');
        return;
    }

    setupHeaderScroll();
    setupNavLinks();
    setupSearch();
    setupLogout();
    await loadAllData();
}

function setupLogout() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {

            localStorage.removeItem('kitapoku_user_id');
            localStorage.removeItem('kitapoku_username');
            localStorage.removeItem('kitapoku_token'); // Eğer kullanılıyorsa

            window.location.href = '/auth.html?v=999';
        });
    }
}



function setupHeaderScroll() {
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



function setupNavLinks() {
    document.querySelectorAll('.nav-link[data-section]').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('data-section');
            const section = document.getElementById(sectionId);
            if (section) {
                const headerH = parseInt(getComputedStyle(document.documentElement).getPropertyValue('--header-height')) || 72;
                const top = section.getBoundingClientRect().top + window.scrollY - headerH - 16;
                window.scrollTo({ top, behavior: 'smooth' });
            }

            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            link.classList.add('active');
        });
    });
}



function setupSearch() {
    if (!searchInput) return;

    let searchTimer;
    searchInput.addEventListener('input', () => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => {
            currentSearchQuery = searchInput.value.trim().toLowerCase();
            renderBooks();
        }, 250);
    });
}



async function loadAllData() {

    if (continueReadingGrid) {
        continueReadingGrid.innerHTML = createSkeleton('reading', 4);
    }
    if (discoverGrid) {
        discoverGrid.innerHTML = createSkeleton('card', 8);
    }
    if (recommendationsGrid) {
        recommendationsGrid.innerHTML = createSkeleton('reading', 4);
    }

    const [booksResult, readingListResult] = await Promise.allSettled([
        loadBooks(),
        loadReadingList(),
    ]);

    if (booksResult.status === 'rejected') {
        console.error('Failed to load books:', booksResult.reason);
    }
    if (readingListResult.status === 'rejected') {
        console.error('Failed to load reading list:', readingListResult.reason);
    }
}

async function loadBooks() {
    try {
        const response = await api.getBooks(1, 100);

        if (Array.isArray(response)) {
            allBooks = response;
        } else if (response && response.items) {
            allBooks = response.items;
        } else if (response && response.data) {
            allBooks = response.data;
        } else {
            allBooks = response ? [].concat(response) : [];
        }
        renderBooks();
    } catch (error) {
        console.error('Books load error:', error);
        if (discoverGrid) {
            discoverGrid.innerHTML = renderEmptyState(
                'Kitaplar yüklenemedi',
                'Sunucu ile bağlantı kurulamadı. Lütfen daha sonra tekrar deneyin.'
            );
        }
    }
}

async function loadReadingList() {
    try {
        const response = await api.getReadingList();
        readingList = Array.isArray(response) ? response : [];
        readingListBookIds = new Set(readingList.map(item =>
            item.bookId || item.BookId || item.book?.id || item.Book?.Id
        ).filter(Boolean));
        renderReadingList();
        renderSimilarBooksSelector();

        renderBooks();
    } catch (error) {
        console.error('Reading list load error:', error);
        if (continueReadingGrid) {
            continueReadingSection.style.display = 'none';
        }
    }
}



function renderReadingList() {
    const completedBooksSection = document.getElementById('completed-books');
    const completedBooksGrid = document.getElementById('completed-books-grid');

    if (!continueReadingGrid || !continueReadingSection) return;

    const incompleteBooks = readingList.filter(item => {
        const percent = Math.round(item.progress?.progressPercentage || item.Progress?.ProgressPercentage || 0);
        return percent < 100;
    });

    const completedBooks = readingList.filter(item => {
        const percent = Math.round(item.progress?.progressPercentage || item.Progress?.ProgressPercentage || 0);
        return percent >= 100;
    });

    // Okumaya Devam Et Kısmı
    if (incompleteBooks.length === 0) {
        continueReadingSection.style.display = 'none';
    } else {
        continueReadingSection.style.display = 'block';
        continueReadingGrid.innerHTML = incompleteBooks.map(item => {
            const book = item.book || item.Book || item;
            const bookId = book.id || book.Id || item.bookId || item.BookId;
            const title = book.title || book.Title || 'Bilinmeyen Kitap';
            const author = book.author || book.Author || '';
            const coverUrl = book.coverImageUrl || book.CoverImageUrl || book.coverUrl || book.CoverUrl || '';
            const percent = Math.round(item.progress?.progressPercentage || item.Progress?.ProgressPercentage || 0);
            return { bookId, title, author, coverUrl, percent, originalIndex: readingList.indexOf(item) };
        })
        .sort((a, b) => a.originalIndex - b.originalIndex)
        .map(data => createReadingCardHtml(data, "Okumaya Devam Et")).join('');
    }

    // Bitirilen Kitaplar Kısmı
    if (completedBooksSection && completedBooksGrid) {
        if (completedBooks.length === 0) {
            completedBooksSection.style.display = 'none';
        } else {
            completedBooksSection.style.display = 'block';
            completedBooksGrid.innerHTML = completedBooks.map(item => {
                const book = item.book || item.Book || item;
                const bookId = book.id || book.Id || item.bookId || item.BookId;
                const title = book.title || book.Title || 'Bilinmeyen Kitap';
                const author = book.author || book.Author || '';
                const coverUrl = book.coverImageUrl || book.CoverImageUrl || book.coverUrl || book.CoverUrl || '';
                const percent = Math.round(item.progress?.progressPercentage || item.Progress?.ProgressPercentage || 0);
                return { bookId, title, author, coverUrl, percent, originalIndex: readingList.indexOf(item) };
            })
            .sort((a, b) => a.originalIndex - b.originalIndex)
            .map(data => createReadingCardHtml(data, "Tekrar Oku")).join('');
        }
    }
}

function createReadingCardHtml(data, actionText) {
    const coverHtml = data.coverUrl
        ? coverImg(data.coverUrl, data.title, true)
        : `<div class="reading-card-cover-placeholder">
               <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>
           </div>`;

    return `
        <div class="reading-card" data-book-id="${data.bookId}" onclick="window.location.href='reader.html?v=13&bookId=${data.bookId}'">
            <div class="reading-card-cover">
                ${coverHtml}
                <div class="reading-card-overlay"><span>${actionText}</span></div>
            </div>
            <div class="reading-card-body">
                <div class="reading-card-title">${escapeHtml(data.title)}</div>
                <div class="reading-card-author">${escapeHtml(data.author)}</div>
                <div class="reading-card-progress">
                    <div class="reading-card-progress-text">%${data.percent} tamamlandı</div>
                    <div class="progress-bar">
                        <div class="progress-bar-fill" style="width: ${data.percent}%"></div>
                    </div>
                </div>
            </div>
        </div>
    `;
}



function renderBooks() {
    if (!discoverGrid) return;

    let filtered = allBooks;

    if (currentSearchQuery) {
        filtered = allBooks.filter(book => {
            const title = (book.title || book.Title || '').toLowerCase();
            const author = (book.author || book.Author || '').toLowerCase();
            const genre = (book.genre || book.Genre || '').toLowerCase();
            return title.includes(currentSearchQuery) || author.includes(currentSearchQuery) || genre.includes(currentSearchQuery);
        });
    }

    if (filtered.length === 0 && currentSearchQuery) {
        discoverGrid.innerHTML = renderEmptyState(
            'Sonuç bulunamadı',
            `"${escapeHtml(currentSearchQuery)}" için eşleşen kitap bulunamadı.`
        );
        return;
    }

    if (filtered.length === 0) {
        discoverGrid.innerHTML = renderEmptyState(
            'Henüz kitap yok',
            'Kütüphane henüz boş. Yakında kitaplar eklenecek!'
        );
        return;
    }

    const cards = filtered.map(book => renderBookCard(book)).join('');
    discoverGrid.innerHTML = cards;

    discoverGrid.querySelectorAll('.book-card-action').forEach(btn => {
        btn.addEventListener('click', handleBookmarkToggle);
    });
}

function renderBookCard(book) {
    const bookId = book.id || book.Id;
    const title = book.title || book.Title || 'Bilinmeyen Kitap';
    const author = book.author || book.Author || 'Bilinmeyen Yazar';
    const genre = book.genre || book.Genre || book.category || book.Category || '';
    const coverUrl = book.coverImageUrl || book.CoverImageUrl || book.coverUrl || book.CoverUrl || '';
    const isInList = readingListBookIds.has(bookId);

    const coverHtml = coverUrl
        ? coverImg(coverUrl, title, true)
        : `<div class="book-card-cover-placeholder">
               <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>
           </div>`;

    const genreBadge = genre
        ? `<span class="badge badge-genre">${escapeHtml(genre)}</span>`
        : '';

    return `
        <div class="book-card" data-book-id="${bookId}">
            <div class="book-card-cover" onclick="window.location.href='reader.html?v=4&bookId=${bookId}'">
                ${coverHtml}
                <button class="book-card-action ${isInList ? 'active' : ''}"
                        data-book-id="${bookId}"
                        title="${isInList ? 'Listeden çıkar' : 'Listeye ekle'}"
                        onclick="event.stopPropagation()">
                    <svg viewBox="0 0 24 24">
                        <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z"/>
                    </svg>
                </button>
            </div>
            <div class="book-card-info" onclick="window.location.href='reader.html?v=4&bookId=${bookId}'">
                <div class="book-card-title" title="${escapeHtml(title)}">${escapeHtml(title)}</div>
                <div class="book-card-author">${escapeHtml(author)}</div>
                <div class="book-card-meta">${genreBadge}</div>
            </div>
        </div>
    `;
}



function renderSimilarBooksSelector() {
    if (!recommendationsSection || !recommendationsGrid) return;

    if (readingList.length === 0) {
        recommendationsSection.style.display = 'none';
        return;
    }

    recommendationsSection.style.display = 'block';

    const sectionTitle = recommendationsSection.querySelector('.section-title');
    if (sectionTitle) {
        sectionTitle.textContent = 'Benzer Kitaplar';
    }
    const sectionSubtitle = recommendationsSection.querySelector('.section-subtitle');
    if (sectionSubtitle) {
        sectionSubtitle.textContent = 'Okuduğunuz bir kitabı seçin, benzerlerini keşfedin';
    }

    let selectorContainer = recommendationsSection.querySelector('.similar-selector');
    if (!selectorContainer) {
        selectorContainer = document.createElement('div');
        selectorContainer.className = 'similar-selector';

        recommendationsGrid.parentNode.insertBefore(selectorContainer, recommendationsGrid);
    }

    const chips = readingList.map(item => {
        const book = item.book || item.Book || item;
        const bookId = book.id || book.Id || item.bookId || item.BookId;
        const title = book.title || book.Title || 'Kitap';
        const coverUrl = book.coverImageUrl || book.CoverImageUrl || book.coverUrl || book.CoverUrl || '';
        const isSelected = selectedSimilarBookId === bookId;

        const coverHtml = coverUrl
            ? coverImg(coverUrl, title, false)
            : `<div class="chip-cover-placeholder">
                   <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>
               </div>`;

        return `
            <button class="similar-chip ${isSelected ? 'similar-chip--active' : ''}"
                    data-book-id="${bookId}" title="${escapeHtml(title)}">
                <div class="similar-chip-cover">${coverHtml}</div>
                <span class="similar-chip-title">${escapeHtml(truncate(title, 20))}</span>
            </button>
        `;
    }).join('');

    selectorContainer.innerHTML = chips;

    selectorContainer.querySelectorAll('.similar-chip').forEach(chip => {
        chip.addEventListener('click', async (e) => {
            const chipBookId = chip.getAttribute('data-book-id');
            if (!chipBookId) return;

            selectedSimilarBookId = chipBookId;
            selectorContainer.querySelectorAll('.similar-chip').forEach(c => c.classList.remove('similar-chip--active'));
            chip.classList.add('similar-chip--active');

            await loadSimilarBooks(chipBookId);
        });
    });

    if (!selectedSimilarBookId && readingList.length > 0) {
        const firstBook = readingList[0];
        const firstBookId = firstBook.bookId || firstBook.BookId || firstBook.book?.id || firstBook.Book?.Id;
        if (firstBookId) {
            selectedSimilarBookId = firstBookId;
            const firstChip = selectorContainer.querySelector('.similar-chip');
            if (firstChip) firstChip.classList.add('similar-chip--active');
            loadSimilarBooks(firstBookId);
        }
    }
}

async function loadSimilarBooks(bookId) {
    if (!recommendationsGrid) return;

    recommendationsGrid.innerHTML = createSkeleton('card', 6);

    try {
        const response = await api.getSimilarBooks(bookId, 12);
        similarBooks = Array.isArray(response) ? response : [];
        renderSimilarBooks();
    } catch (error) {
        console.error('Similar books load error:', error);
        recommendationsGrid.innerHTML = renderEmptyState(
            'Benzer kitap bulunamadı',
            'Bu kitap için benzer öneriler oluşturulamadı.'
        );
    }
}

function renderSimilarBooks() {
    if (!recommendationsGrid) return;

    if (similarBooks.length === 0) {
        recommendationsGrid.innerHTML = renderEmptyState(
            'Benzer kitap bulunamadı',
            'Bu kitap için henüz bir öneri yok.'
        );
        return;
    }

    const cards = similarBooks.map(book => {
        const bookId = book.bookId || book.BookId || book.id || book.Id;
        const title = book.title || book.Title || 'Bilinmeyen Kitap';
        const author = book.author || book.Author || '';
        const coverUrl = book.coverImageUrl || book.CoverImageUrl || book.coverUrl || book.CoverUrl || '';
        const genre = book.genre || book.Genre || '';
        const reason = book.reason || book.Reason || '';
        const score = book.relevanceScore || book.RelevanceScore || 0;
        const matchPercent = Math.round(score * 100);

        const coverHtml = coverUrl
            ? coverImg(coverUrl, title, true)
            : `<div class="book-card-cover-placeholder">
                   <svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>
               </div>`;

        const genreBadge = genre
            ? `<span class="badge badge-genre">${escapeHtml(genre)}</span>`
            : '';

        return `
            <div class="book-card book-card--similar" data-book-id="${bookId}"
                 onclick="window.location.href='reader.html?v=4&bookId=${bookId}'">
                <div class="book-card-cover">
                    ${coverHtml}
                    <div class="similarity-badge">%${matchPercent}</div>
                </div>
                <div class="book-card-info">
                    <div class="book-card-title" title="${escapeHtml(title)}">${escapeHtml(title)}</div>
                    <div class="book-card-author">${escapeHtml(author)}</div>
                    <div class="book-card-meta">${genreBadge}</div>
                    ${reason ? `<div class="book-card-reason">${escapeHtml(reason)}</div>` : ''}
                </div>
            </div>
        `;
    }).join('');

    recommendationsGrid.innerHTML = cards;
}



async function handleBookmarkToggle(e) {
    e.preventDefault();
    e.stopPropagation();

    const btn = e.currentTarget;
    const bookId = btn.getAttribute('data-book-id');
    if (!bookId) return;

    const isActive = btn.classList.contains('active');

    btn.classList.toggle('active');

    try {
        if (isActive) {
            await api.removeFromReadingList(bookId);
            readingListBookIds.delete(bookId);
            readingList = readingList.filter(item => {
                const id = item.bookId || item.BookId || item.book?.id || item.Book?.Id;
                return id !== bookId;
            });
            showToast('Okuma listenizden çıkarıldı', 'info');
        } else {
            await api.addToReadingList(bookId);
            readingListBookIds.add(bookId);

            await loadReadingList();
            showToast('Okuma listenize eklendi', 'success');
        }
        renderReadingList();
        renderSimilarBooksSelector();
    } catch (error) {

        btn.classList.toggle('active');
        console.error('Bookmark toggle error:', error);
        showToast('İşlem başarısız oldu', 'error');
    }
}



function renderEmptyState(title, text) {
    return `
        <div class="empty-state" style="grid-column: 1 / -1;">
            <svg class="empty-state-icon" viewBox="0 0 24 24">
                <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
                <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>
            </svg>
            <div class="empty-state-title">${title}</div>
            <div class="empty-state-text">${text}</div>
        </div>
    `;
}
