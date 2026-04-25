let currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
let currentPage = localStorage.getItem('currentPage') || 'home';
let allMedications = [];

function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type === 'success' ? 'bg-green-500' : 'bg-red-500'} text-white`;
    toast.innerHTML = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

function navigateTo(page) {
    currentPage = page;
    localStorage.setItem('currentPage', page);
    renderApp();
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    currentUser = null;
    showToast('تم تسجيل الخروج');
    navigateTo('home');
}

function Navbar() {
    const token = localStorage.getItem('token');
    return `
        <nav class="bg-white dark:bg-gray-800 shadow-lg sticky top-0 z-50">
            <div class="max-w-7xl mx-auto px-4">
                <div class="flex justify-between items-center h-16">
                    <div class="flex items-center gap-2 cursor-pointer" onclick="navigateTo('home')">
                        <i class="fa-solid fa-heartbeat text-emerald-600 text-3xl"></i>
                        <span class="text-xl font-bold">${t('appName')}</span>
                    </div>
                    <div class="flex items-center gap-4">
                        <button onclick="toggleTheme()" class="text-gray-600 dark:text-white">
                            <i class="fa-solid ${currentTheme === 'light' ? 'fa-moon' : 'fa-sun'} text-xl"></i>
                        </button>
                        <button onclick="toggleLang()" class="text-gray-600 dark:text-white">
                            <i class="fa-solid fa-globe"></i> ${currentLang === 'ar' ? 'EN' : 'عربي'}
                        </button>
                        ${token && currentUser ? `
                            <button onclick="navigateTo('home')">${t('home')}</button>
                            <button onclick="navigateTo('dashboard')">${t('dashboard')}</button>
                            ${currentUser.role === 'Admin' ? `<button onclick="navigateTo('admin')" class="text-purple-600">${t('adminPanel')}</button>` : ''}
                            <button onclick="logout()" class="text-red-600">${t('logout')}</button>
                        ` : `
                            <button onclick="navigateTo('login')" class="text-emerald-600">${t('login')}</button>
                            <button onclick="navigateTo('register')" class="bg-emerald-600 text-white px-4 py-2 rounded-lg">${t('register')}</button>
                        `}
                    </div>
                </div>
            </div>
        </nav>
    `;
}

async function renderHome() {
    return `
        <div class="max-w-7xl mx-auto px-4 py-8">
            <div class="bg-gradient-to-r from-emerald-600 to-teal-700 rounded-3xl p-12 text-white mb-12">
                <h1 class="text-5xl font-bold mb-4">${t('heroTitle')}</h1>
                <p class="text-lg mb-8">${t('heroDesc')}</p>
                <div class="relative max-w-xl">
                    <input type="text" id="searchInput" placeholder="${t('searchPlaceholder')}" 
                        class="w-full px-6 py-4 rounded-full text-gray-800">
                    <i class="fa-solid fa-search absolute left-5 top-5 text-gray-400"></i>
                </div>
            </div>
            <h2 class="text-2xl font-bold mb-6">${t('medDirectory')}</h2>
            <div id="medicationsList" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <div class="text-center py-12">جاري التحميل...</div>
            </div>
        </div>
    `;
}

function renderLogin() {
    return `
        <div class="min-h-screen flex items-center justify-center p-4">
            <div class="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-xl w-full max-w-md">
                <div class="text-center mb-6">
                    <i class="fa-solid fa-heartbeat text-emerald-600 text-6xl mb-4"></i>
                    <h2 class="text-2xl font-bold">${t('loginTitle')}</h2>
                </div>
                <form onsubmit="handleLogin(event)">
                    <div class="mb-4">
                        <label class="block mb-2">${t('email')}</label>
                        <input type="email" id="loginEmail" required class="w-full px-4 py-2 border rounded-lg dark:bg-gray-700">
                    </div>
                    <div class="mb-6">
                        <label class="block mb-2">${t('password')}</label>
                        <input type="password" id="loginPassword" required class="w-full px-4 py-2 border rounded-lg dark:bg-gray-700">
                    </div>
                    <button type="submit" class="w-full bg-emerald-600 text-white py-3 rounded-lg font-bold">${t('login')}</button>
                </form>
                <div class="mt-6 text-center">
                    ${t('noAccount')} <button onclick="navigateTo('register')" class="text-emerald-600">${t('register')}</button>
                </div>
                <div class="mt-4 text-center text-xs text-gray-400">
                    <p>Admin: admin@dawaee.com / Admin123!</p>
                    <p>Patient: patient@test.com / Patient123!</p>
                </div>
            </div>
        </div>
    `;
}

function renderRegister() {
    return `
        <div class="min-h-screen flex items-center justify-center p-4">
            <div class="bg-white dark:bg-gray-800 p-8 rounded-2xl shadow-xl w-full max-w-lg">
                <h2 class="text-2xl font-bold text-center mb-6">${t('registerTitle')}</h2>
                <form onsubmit="handleRegister(event)">
                    <div class="grid grid-cols-2 gap-4 mb-4">
                        <div><label>${t('firstName')}</label><input type="text" id="regFirstName" required class="w-full px-4 py-2 border rounded-lg"></div>
                        <div><label>${t('lastName')}</label><input type="text" id="regLastName" required class="w-full px-4 py-2 border rounded-lg"></div>
                    </div>
                    <div class="mb-4"><label>${t('email')}</label><input type="email" id="regEmail" required class="w-full px-4 py-2 border rounded-lg"></div>
                    <div class="mb-4"><label>${t('password')}</label><input type="password" id="regPassword" required class="w-full px-4 py-2 border rounded-lg"></div>
                    <div class="mb-6"><label>${t('diseases')}</label><textarea id="regDiseases" rows="2" class="w-full px-4 py-2 border rounded-lg"></textarea></div>
                    <button type="submit" class="w-full bg-emerald-600 text-white py-3 rounded-lg font-bold">${t('register')}</button>
                </form>
            </div>
        </div>
    `;
}

async function loadMedications() {
    try {
        const meds = await api.medications.getAll();
        const container = document.getElementById('medicationsList');
        if (container) {
            container.innerHTML = meds.map(med => `
                <div class="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow">
                    <h3 class="text-xl font-bold text-emerald-700 mb-2">${med.nameAr}</h3>
                    <p class="text-sm text-gray-500 mb-3">${med.activeIngredientAr}</p>
                    <p class="text-gray-700 mb-4">${med.descriptionAr}</p>
                    <div class="p-3 rounded-xl ${med.dangerLevel === 'high' ? 'bg-red-50' : med.dangerLevel === 'medium' ? 'bg-orange-50' : 'bg-green-50'}">
                        <strong>${t('interactionWarning')}</strong>
                        <p class="text-sm">${med.warningsAr}</p>
                    </div>
                </div>
            `).join('');
        }
    } catch (error) {
        console.error(error);
    }
}

async function handleLogin(e) {
    e.preventDefault();
    try {
        const result = await api.auth.login({
            email: document.getElementById('loginEmail').value,
            password: document.getElementById('loginPassword').value
        });
        localStorage.setItem('token', result.token);
        localStorage.setItem('currentUser', JSON.stringify(result));
        currentUser = result;
        showToast('تم تسجيل الدخول بنجاح');
        navigateTo('home');
    } catch (error) {
        showToast('خطأ في البريد أو كلمة المرور', 'error');
    }
}

async function handleRegister(e) {
    e.preventDefault();
    try {
        const result = await api.auth.register({
            firstName: document.getElementById('regFirstName').value,
            lastName: document.getElementById('regLastName').value,
            email: document.getElementById('regEmail').value,
            password: document.getElementById('regPassword').value,
            chronicDiseases: document.getElementById('regDiseases').value
        });
        localStorage.setItem('token', result.token);
        localStorage.setItem('currentUser', JSON.stringify(result));
        currentUser = result;
        showToast('تم إنشاء الحساب بنجاح');
        navigateTo('home');
    } catch (error) {
        showToast('حدث خطأ', 'error');
    }
}

async function renderApp() {
    const token = localStorage.getItem('token');
    let mainContent = '';
    
    if (!token && (currentPage === 'dashboard' || currentPage === 'admin')) {
        mainContent = renderLogin();
        currentPage = 'login';
    } else if (currentPage === 'login') {
        mainContent = renderLogin();
    } else if (currentPage === 'register') {
        mainContent = renderRegister();
    } else {
        mainContent = await renderHome();
    }
    
    document.getElementById('root').innerHTML = Navbar() + `<main>${mainContent}</main>`;
    
    if (currentPage === 'home') {
        await loadMedications();
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', async (e) => {
                const meds = await api.medications.getAll(e.target.value);
                const container = document.getElementById('medicationsList');
                if (container) {
                    container.innerHTML = meds.map(med => `
                        <div class="bg-white dark:bg-gray-800 rounded-2xl p-6 shadow">
                            <h3 class="text-xl font-bold text-emerald-700 mb-2">${med.nameAr}</h3>
                            <p class="text-gray-700 mb-4">${med.descriptionAr}</p>
                        </div>
                    `).join('');
                }
            });
        }
    }
}

// Start app
renderApp();
