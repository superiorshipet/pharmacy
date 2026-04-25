// Dawaee Chatbot
(function() {
    let chatMessages = [];
    let isOpen = false;

    function createChatbotUI() {
        // Create toggle button
        const btn = document.createElement('button');
        btn.id = 'chatbot-toggle';
        btn.innerHTML = '💊';
        btn.title = 'دواءي - المساعد الطبي';
        btn.style.cssText = `
            position:fixed; bottom:24px; left:24px; z-index:9999;
            width:56px; height:56px; border-radius:50%; border:none;
            background:#2563eb; color:#fff; font-size:24px;
            cursor:pointer; box-shadow:0 4px 16px rgba(37,99,235,0.4);
            transition:transform 0.2s;
        `;
        btn.onmouseenter = () => btn.style.transform = 'scale(1.1)';
        btn.onmouseleave = () => btn.style.transform = 'scale(1)';

        // Create chat window
        const win = document.createElement('div');
        win.id = 'chatbot-window';
        win.style.cssText = `
            position:fixed; bottom:90px; left:24px; z-index:9998;
            width:340px; height:480px; border-radius:16px;
            background:#fff; box-shadow:0 8px 32px rgba(0,0,0,0.18);
            display:none; flex-direction:column; overflow:hidden;
            font-family:inherit;
        `;
        win.innerHTML = `
            <div style="background:#2563eb;color:#fff;padding:14px 16px;display:flex;align-items:center;justify-content:space-between;">
                <span style="font-weight:700;font-size:15px;">💊 دواءي - مساعدك الطبي</span>
                <button onclick="document.getElementById('chatbot-window').style.display='none';window._chatOpen=false;"
                    style="background:none;border:none;color:#fff;font-size:20px;cursor:pointer;line-height:1;">×</button>
            </div>
            <div id="chatbot-messages" style="flex:1;overflow-y:auto;padding:12px;display:flex;flex-direction:column;gap:8px;background:#f8fafc;">
                <div class="bot-msg" style="background:#e0e7ff;border-radius:12px 12px 12px 4px;padding:10px 14px;max-width:85%;font-size:14px;line-height:1.5;">
                    أهلاً! أنا دواءي 👋 كيف أقدر أساعدك اليوم؟
                </div>
            </div>
            <div style="padding:10px;border-top:1px solid #e2e8f0;display:flex;gap:8px;background:#fff;">
                <input id="chatbot-input" type="text" placeholder="اكتب رسالتك..." dir="rtl"
                    style="flex:1;border:1px solid #cbd5e1;border-radius:8px;padding:8px 12px;font-size:14px;outline:none;font-family:inherit;"
                    onkeydown="if(event.key==='Enter')window._sendChat()"/>
                <button onclick="window._sendChat()"
                    style="background:#2563eb;color:#fff;border:none;border-radius:8px;padding:8px 14px;cursor:pointer;font-size:18px;">
                    ➤
                </button>
            </div>
        `;

        document.body.appendChild(btn);
        document.body.appendChild(win);

        btn.onclick = () => {
            window._chatOpen = !window._chatOpen;
            win.style.display = window._chatOpen ? 'flex' : 'none';
            if (window._chatOpen) {
                document.getElementById('chatbot-input')?.focus();
            }
        };
    }

    function appendMessage(text, isUser) {
        const container = document.getElementById('chatbot-messages');
        if (!container) return;
        const div = document.createElement('div');
        div.style.cssText = isUser
            ? 'background:#2563eb;color:#fff;border-radius:12px 12px 4px 12px;padding:10px 14px;max-width:85%;font-size:14px;align-self:flex-end;line-height:1.5;'
            : 'background:#e0e7ff;border-radius:12px 12px 12px 4px;padding:10px 14px;max-width:85%;font-size:14px;line-height:1.5;white-space:pre-wrap;';
        div.textContent = text;
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
    }

    function appendTyping() {
        const container = document.getElementById('chatbot-messages');
        if (!container) return;
        const div = document.createElement('div');
        div.id = 'chatbot-typing';
        div.style.cssText = 'background:#e0e7ff;border-radius:12px;padding:10px 14px;font-size:20px;';
        div.textContent = '...';
        container.appendChild(div);
        container.scrollTop = container.scrollHeight;
    }

    function removeTyping() {
        document.getElementById('chatbot-typing')?.remove();
    }

    window._chatOpen = false;
    window._chatMessages = [];

    window._sendChat = async function() {
        const input = document.getElementById('chatbot-input');
        if (!input) return;
        const text = input.value.trim();
        if (!text) return;
        input.value = '';

        appendMessage(text, true);
        window._chatMessages.push({ role: 'user', message: text });

        appendTyping();

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                removeTyping();
                appendMessage('يرجى تسجيل الدخول أولاً للاستخدام الدردشة.', false);
                return;
            }

            const apiBase = (window.BACKEND_URL || '') + '/api';
            const res = await fetch(`${apiBase}/chatbot/chat`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ messages: window._chatMessages })
            });

            removeTyping();

            if (!res.ok) {
                appendMessage('حدث خطأ في الاتصال. حاول مرة ثانية.', false);
                return;
            }

            const data = await res.json();
            const reply = data.reply || 'لم أفهم. حاول مرة ثانية.';
            appendMessage(reply, false);
            window._chatMessages.push({ role: 'assistant', message: reply });

            // Keep only last 10 messages to avoid large payloads
            if (window._chatMessages.length > 10) {
                window._chatMessages = window._chatMessages.slice(-10);
            }
        } catch (err) {
            removeTyping();
            appendMessage('حدث خطأ. تحقق من الاتصال.', false);
            console.error('Chatbot error:', err);
        }
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', createChatbotUI);
    } else {
        createChatbotUI();
    }
})();
