// Translations for Dawaee Platform
const TRANSLATIONS = {
    ar: {
        appName: 'دوائي',
        home: 'الرئيسية',
        dashboard: 'لوحة التحكم',
        logout: 'خروج',
        login: 'دخول',
        register: 'حساب جديد',
        adminPanel: 'لوحة الأدمن',
        email: 'البريد الإلكتروني',
        password: 'كلمة المرور',
        noAccount: 'ليس لديك حساب؟',
        firstName: 'الاسم الأول',
        lastName: 'اسم العائلة',
        diseases: 'الأمراض المزمنة',
        loginTitle: 'تسجيل الدخول',
        registerTitle: 'إنشاء حساب جديد',
        heroTitle: 'دليلك الشامل للأدوية والتفاعلات الدوائية',
        heroDesc: 'ابحث عن أدويتك، تعرف على المواد الفعالة، واكتشف التحذيرات الهامة',
        searchPlaceholder: 'ابحث باسم الدواء أو المادة الفعالة...',
        medDirectory: 'دليل الأدوية',
        noResults: 'لا توجد أدوية مطابقة لبحثك',
        interactionWarning: '⚠️ تحذير تفاعلات دوائية',
        welcome: 'أهلاً بك',
        patient: 'مريض',
        dailySchedule: 'جدول الأدوية اليومي',
        today: 'اليوم',
        taken: 'تم التناول',
        notTaken: 'لم يتم التناول',
        weeklyReport: 'التقرير الأسبوعي',
        adherence: 'نسبة الالتزام',
        medsTaken: 'الأدوية المتناولة',
        from: 'من',
        addToSchedule: 'إضافة إلى جدولي',
        selectMedication: 'اختر الدواء',
        selectTime: 'اختر الوقت',
        add: 'إضافة',
        cancel: 'إلغاء',
        patients: 'المرضى',
        medications: 'الأدوية',
        actions: 'إجراءات',
        addMedication: 'إضافة دواء جديد',
        nameAr: 'الاسم بالعربية',
        nameEn: 'الاسم بالإنجليزية',
        activeIngredient: 'المادة الفعالة',
        description: 'الوصف',
        warnings: 'التحذيرات',
        dangerLevel: 'مستوى الخطورة',
        save: 'حفظ',
        delete: 'حذف',
        edit: 'تعديل',
        botTitle: 'المساعد الطبي الذكي',
        botPlaceholder: 'اكتب استفسارك الطبي هنا...',
        chatWelcome: 'مرحباً! أنا دواءي، مساعدك الطبي الذكي. كيف أساعدك اليوم؟'
    },
    en: {
        appName: 'Dawaee',
        home: 'Home',
        dashboard: 'Dashboard',
        logout: 'Logout',
        login: 'Login',
        register: 'Sign Up',
        adminPanel: 'Admin Panel',
        email: 'Email',
        password: 'Password',
        noAccount: "Don't have an account?",
        firstName: 'First Name',
        lastName: 'Last Name',
        diseases: 'Chronic Diseases',
        loginTitle: 'Welcome Back',
        registerTitle: 'Create Account',
        heroTitle: 'Your Complete Guide to Medications',
        heroDesc: 'Search medications, learn about active ingredients, and discover important warnings',
        searchPlaceholder: 'Search by medication or active ingredient...',
        medDirectory: 'Medications Directory',
        noResults: 'No medications match your search',
        interactionWarning: '⚠️ Drug Interaction Warning',
        welcome: 'Welcome',
        patient: 'Patient',
        dailySchedule: 'Daily Schedule',
        today: 'Today',
        taken: 'Taken',
        notTaken: 'Not Taken',
        weeklyReport: 'Weekly Report',
        adherence: 'Adherence Rate',
        medsTaken: 'Meds Taken',
        from: 'of',
        addToSchedule: 'Add to Schedule',
        selectMedication: 'Select Medication',
        selectTime: 'Select Time',
        add: 'Add',
        cancel: 'Cancel',
        patients: 'Patients',
        medications: 'Medications',
        actions: 'Actions',
        addMedication: 'Add Medication',
        nameAr: 'Arabic Name',
        nameEn: 'English Name',
        activeIngredient: 'Active Ingredient',
        description: 'Description',
        warnings: 'Warnings',
        dangerLevel: 'Danger Level',
        save: 'Save',
        delete: 'Delete',
        edit: 'Edit',
        botTitle: 'AI Medical Assistant',
        botPlaceholder: 'Type your medical question...',
        chatWelcome: 'Hello! I am Dawaee, your AI medical assistant. How can I help you?'
    }
};

let currentLang = localStorage.getItem('lang') || 'ar';
let currentTheme = localStorage.getItem('theme') || 'light';

function t(key) {
    return TRANSLATIONS[currentLang][key] || key;
}

function toggleLang() {
    currentLang = currentLang === 'ar' ? 'en' : 'ar';
    localStorage.setItem('lang', currentLang);
    location.reload();
}

function toggleTheme() {
    currentTheme = currentTheme === 'light' ? 'dark' : 'light';
    localStorage.setItem('theme', currentTheme);
    if (currentTheme === 'dark') {
        document.documentElement.classList.add('dark');
        document.body.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
        document.body.classList.remove('dark');
    }
    location.reload();
}
