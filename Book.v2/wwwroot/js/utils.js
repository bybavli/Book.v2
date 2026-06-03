


export function debounce(fn, delay = 2000) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
}


export function formatDate(dateStr) {
    if (!dateStr) return '';
    const months = [
        'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
        'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
    ];
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return '';
    const day = date.getDate();
    const month = months[date.getMonth()];
    const year = date.getFullYear();
    return `${day} ${month} ${year}`;
}


export function truncate(text, maxLength = 100) {
    if (!text) return '';
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trimEnd() + '…';
}


export function showToast(message, type = 'success', duration = 3500) {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const icons = {
        success: `<svg class="toast-icon" viewBox="0 0 24 24"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>`,
        error: `<svg class="toast-icon" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>`,
        info: `<svg class="toast-icon" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>`
    };

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `${icons[type] || icons.info}<span>${message}</span>`;

    container.appendChild(toast);

    const dismissTimer = setTimeout(() => dismissToast(toast), duration);

    toast.addEventListener('click', () => {
        clearTimeout(dismissTimer);
        dismissToast(toast);
    });
}


function dismissToast(toast) {
    if (!toast || toast.classList.contains('dismissing')) return;
    toast.classList.add('dismissing');
    toast.addEventListener('animationend', () => {
        toast.remove();

        const container = document.querySelector('.toast-container');
        if (container && container.children.length === 0) {
            container.remove();
        }
    }, { once: true });
}


export function createSkeleton(type = 'card', count = 4) {
    let html = '';

    if (type === 'card') {
        for (let i = 0; i < count; i++) {
            html += `
                <div class="book-card skeleton-card" style="animation-delay: ${i * 80}ms">
                    <div class="skeleton-cover skeleton"></div>
                    <div class="book-card-info">
                        <div class="skeleton-text skeleton" style="margin-bottom: 8px;"></div>
                        <div class="skeleton-text skeleton skeleton-text--short" style="margin-bottom: 10px;"></div>
                        <div class="skeleton-text skeleton skeleton-text--xs"></div>
                    </div>
                </div>
            `;
        }
    } else if (type === 'reading') {
        for (let i = 0; i < count; i++) {
            html += `<div class="skeleton skeleton-reading" style="animation-delay: ${i * 100}ms"></div>`;
        }
    } else if (type === 'text') {
        for (let i = 0; i < count; i++) {
            html += `<div class="skeleton skeleton-text" style="width: ${60 + Math.random() * 30}%; margin-bottom: 8px; animation-delay: ${i * 60}ms"></div>`;
        }
    }

    return html;
}


export function generateId() {
    return 'xxxx-xxxx-xxxx'.replace(/x/g, () =>
        Math.floor(Math.random() * 16).toString(16)
    );
}


export function escapeHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}


export function getQueryParam(name) {
    const params = new URLSearchParams(window.location.search);
    return params.get(name);
}


export function calcProgressPercent(currentPage, totalPages) {
    if (!totalPages || totalPages <= 0) return 0;
    return Math.min(100, Math.round((currentPage / totalPages) * 100));
}


export const BOOK_ICON_SVG = `<svg viewBox="0 0 24 24"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>`;


export const CHEVRON_RIGHT_SVG = `<svg viewBox="0 0 24 24"><polyline points="9 18 15 12 9 6"/></svg>`;
