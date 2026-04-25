// API Configuration
// In production (Railway), the frontend should call the deployed backend URL.
// Set BACKEND_URL to your Railway backend URL, or leave empty to use same origin.
const API_URL = (window.BACKEND_URL || '') + '/api';

console.log('📡 API URL:', API_URL);

async function apiRequest(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers
    };
    
    try {
        const response = await fetch(`${API_URL}${endpoint}`, {
            ...options,
            headers
        });
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

const api = {
    auth: {
        login: (data) => apiRequest('/auth/login', { method: 'POST', body: JSON.stringify(data) }),
        register: (data) => apiRequest('/auth/register', { method: 'POST', body: JSON.stringify(data) })
    },
    medications: {
        getAll: (search = '') => apiRequest(`/Medications${search ? `?search=${search}` : ''}`)
    },
    dashboard: {
        getSchedules: () => apiRequest('/dashboard/schedules'),
        addSchedule: (data) => apiRequest('/dashboard/schedules', { method: 'POST', body: JSON.stringify(data) }),
        toggleTaken: (data) => apiRequest('/dashboard/schedules/toggle', { method: 'PUT', body: JSON.stringify(data) }),
        deleteSchedule: (id) => apiRequest(`/dashboard/schedules/${id}`, { method: 'DELETE' }),
        getWeeklyReport: () => apiRequest('/dashboard/weekly-report')
    },
    admin: {
        getPatients: () => apiRequest('/admin/patients'),
        deletePatient: (id) => apiRequest(`/admin/patients/${id}`, { method: 'DELETE' }),
        getMedications: () => apiRequest('/admin/medications'),
        createMedication: (data) => apiRequest('/admin/medications', { method: 'POST', body: JSON.stringify(data) }),
        deleteMedication: (id) => apiRequest(`/admin/medications/${id}`, { method: 'DELETE' })
    },
    chatbot: {
        chat: (messages) => apiRequest('/chatbot/chat', { method: 'POST', body: JSON.stringify({ messages }) }),
        getHistory: () => apiRequest('/chatbot/history')
    }
};
