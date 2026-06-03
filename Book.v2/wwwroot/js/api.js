

const API_BASE = '/api';
function getUserId() {
    return localStorage.getItem('kitapoku_user_id') || '11111111-1111-1111-1111-111111111111';
}


async function request(endpoint, options = {}) {
    const url = `${API_BASE}${endpoint}`;
    const config = {
        headers: {
            'Content-Type': 'application/json',
            ...options.headers,
        },
        ...options,
    };

    try {
        const response = await fetch(url, config);

        if (!response.ok) {
            const errorBody = await response.text().catch(() => '');
            throw new Error(
                `API Error ${response.status}: ${response.statusText}${errorBody ? ` — ${errorBody}` : ''}`
            );
        }

        if (response.status === 204) {
            return null;
        }

        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        }

        return await response.text();
    } catch (error) {
        if (error.name === 'TypeError' && error.message.includes('Failed to fetch')) {
            console.error(`Network error for ${url}:`, error);
            throw new Error('Sunucuya bağlanılamadı. Lütfen internet bağlantınızı kontrol edin.');
        }
        throw error;
    }
}

export const api = {
    

    
    async getBooks(page = 1, pageSize = 12) {
        return request(`/books?page=${page}&pageSize=${pageSize}`);
    },

    
    async getBookDetail(bookId) {
        return request(`/books/${bookId}`);
    },

    
    async getBookPage(bookId, pageNumber) {
        return request(`/books/${bookId}/pages/${pageNumber}`);
    },

    
    async getBookPages(bookId) {
        return request(`/books/${bookId}/pages`);
    },

    
    async getSimilarBooks(bookId, count = 10) {
        return request(`/books/${bookId}/similar?count=${count}`);
    },


    

    
    async getReadingList(userId = getUserId()) {
        return request(`/users/${userId}/reading-list`);
    },

    
    async addToReadingList(bookId, userId = getUserId()) {
        return request(`/users/${userId}/reading-list`, {
            method: 'POST',
            body: JSON.stringify({ bookId }),
        });
    },

    
    async removeFromReadingList(bookId, userId = getUserId()) {
        return request(`/users/${userId}/reading-list/${bookId}`, {
            method: 'DELETE',
        });
    },


    

    
    async getProgress(bookId, userId = getUserId()) {
        return request(`/users/${userId}/progress/${bookId}`);
    },

    
    async updateProgress(bookId, currentPage, totalPages, userId = getUserId()) {
        return request(`/users/${userId}/progress/${bookId}`, {
            method: 'PUT',
            body: JSON.stringify({
                currentPage,
                totalPages,
            }),
        });
    },


    

    
    async getRecommendations(count = 10, userId = getUserId()) {
        return request(`/users/${userId}/recommendations?count=${count}`);
    },
};
