

document.addEventListener('DOMContentLoaded', () => {


    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');

    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }
});

window.toggleForm = function(target) {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    
    const titleLeft = document.querySelector('.split-text .part.left');
    const titleRight = document.querySelector('.split-text .part.right');

    if (target === 'register') {

        loginForm.classList.remove('active');
        loginForm.style.opacity = '0';
        loginForm.style.transform = 'scale(0.95)';
        
        setTimeout(() => {
            loginForm.classList.add('hidden');
            registerForm.classList.remove('hidden');

            void registerForm.offsetWidth;
            
            registerForm.classList.add('active');
            registerForm.style.opacity = '1';
            registerForm.style.transform = 'scale(1)';

            titleLeft.textContent = "Kayıt";
            titleRight.textContent = "Ol";
        }, 400);

    } else {

        registerForm.classList.remove('active');
        registerForm.style.opacity = '0';
        registerForm.style.transform = 'scale(0.95)';
        
        setTimeout(() => {
            registerForm.classList.add('hidden');
            loginForm.classList.remove('hidden');

            void loginForm.offsetWidth;
            
            loginForm.classList.add('active');
            loginForm.style.opacity = '1';
            loginForm.style.transform = 'scale(1)';

            titleLeft.textContent = "Giriş";
            titleRight.textContent = "Yap";
        }, 400);
    }
};

async function handleLogin(e) {
    e.preventDefault();
    const btn = document.getElementById('loginBtn');
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    setLoading(btn, true, "Giriş Yap");

    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (response.ok) {
            showToast('Giriş başarılı! Yönlendiriliyorsunuz...', 'success');

            localStorage.setItem('kitapoku_user_id', data.id);
            localStorage.setItem('kitapoku_username', data.username);
            
            setTimeout(() => {
                window.location.href = '/index.html?v=999';
            }, 1500);
        } else {
            showToast(data.message || 'Giriş başarısız oldu.', 'error');
            setLoading(btn, false, "Giriş Yap");
        }
    } catch (err) {
        console.error(err);
        showToast('Sunucuya bağlanılamadı.', 'error');
        setLoading(btn, false, "Giriş Yap");
    }
}

async function handleRegister(e) {
    e.preventDefault();
    const btn = document.getElementById('registerBtn');
    const username = document.getElementById('regUsername').value;
    const email = document.getElementById('regEmail').value;
    const password = document.getElementById('regPassword').value;

    setLoading(btn, true, "Kayıt Ol");

    try {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password })
        });

        const data = await response.json();

        if (response.ok) {
            showToast('Kayıt başarılı! Yönlendiriliyorsunuz...', 'success');

            localStorage.setItem('kitapoku_user_id', data.id);
            localStorage.setItem('kitapoku_username', data.username);
            
            setTimeout(() => {
                window.location.href = '/index.html?v=999';
            }, 1500);
        } else {
            showToast(data.message || 'Kayıt başarısız oldu.', 'error');
            setLoading(btn, false, "Kayıt Ol");
        }
    } catch (err) {
        console.error(err);
        showToast('Sunucuya bağlanılamadı.', 'error');
        setLoading(btn, false, "Kayıt Ol");
    }
}

function setLoading(button, isLoading, originalText) {
    if (!button) return;
    const textSpan = button.querySelector('.btn-text');
    if (isLoading) {
        button.disabled = true;
        if (textSpan) textSpan.textContent = "Lütfen Bekleyin...";
    } else {
        button.disabled = false;
        if (textSpan) textSpan.textContent = originalText;
    }
}

function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;

    container.appendChild(toast);

    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300); // animasyon süresini bekle
    }, 3000);
}
