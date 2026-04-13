

// ====================== AUDIO HELPERS ======================
window.playIntroAudio = (url) => new Promise((resolve, reject) => {
    const a = new Audio(url);
    a.play().then(() => a.onended = resolve).catch(reject);
});

window.playBase64Audio = (url) => new Promise((resolve, reject) => {
    const a = new Audio(url);
    a.play().then(() => a.onended = resolve).catch(reject);
});

window.playBase64AudioWithIntro = async (base64Url) => {
    try {
        //await window.playIntroAudio(introUrl);
        await window.playBase64Audio(base64Url);
    } catch (e) {
        console.error('Audio error:', e);
    }
};

// ====================== MAIN CLASS ======================
class CounterDisplay {
    constructor() {
        this.settings = {};
        this.counters = [];
        this.isLoading = false;
        this.hubConnection = null;
        this.speechQueue = [];           // [{ id, text }]
        this.isSpeaking = false;

        this.init();
    }

    async init() {
        await this.loadSignalRAndStart();
    }

    async loadSignalRAndStart() {
        if (typeof signalR !== 'undefined') {
            this.startApp();
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js';
        script.onload = () => this.startApp();
        script.onerror = () => console.error("Lỗi tải SignalR");
        document.head.appendChild(script);
    }

    async startApp() {
        await this.fetchAll();
        this.startClock();
        this.startPolling();
        this.setupResponsive();
        this.startSignalR();
    }

    // ====================== API ======================
    async fetchSettings() {
        try {
            const r = await fetch('/qms/api/setting');
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            const data = await r.json();

            this.settings = {
                footerTextCountersMainView: data.footerTextCountersMainView || "Chào mừng quý khách...",
                footerTextColor: data.footerTextColor || "#ffffff",
                footerTextFontSize: data.footerTextFontSize || 16,
                maxVisibleCounters: data.maxVisibleCounters || 12,
                isActiveSpeechCall: data.isActiveSpeechCall !== false,
                speechIntroUrl: data.speechIntroUrl || "assets/audio/NHAC.mp3",
                // Cấu hình TTS từ API
                ttsProtocol: data.ttsProtocol || "http",
                ttsIpAddress: data.ttsIpAddress || "10.190.186.210",
                ttsPort: data.ttsPort || "5000"
            };
        } catch (e) {
            console.warn('Settings fallback', e);
            this.settings = {
                footerTextCountersMainView: "Chào mừng quý khách...",
                footerTextColor: "#ffffff",
                footerTextFontSize: 16,
                maxVisibleCounters: 12,
                isActiveSpeechCall: true,
                speechIntroUrl: "assets/audio/NHAC.mp3",
                ttsProtocol: "http",
                ttsIpAddress: "10.190.186.210",
                ttsPort: "5000"
            };
        }
    }

    async fetchCounters() {
        try {
            const r = await fetch('/qms/api/Counter/get-all-counter', {
                headers: {
                    'Cache-Control': 'no-cache, no-store, must-revalidate',
                    'Pragma': 'no-cache',
                    'Expires': '0'
                }
            });
            if (!r.ok) throw new Error(`HTTP ${r.status}`);
            const data = await r.json();

            if (data && Array.isArray(data.items)) {
                this.counters = data.items.filter(c => c.isActive !== false);
            } else {
                throw new Error("items không tồn tại");
            }
        } catch (e) {
            console.error('Lỗi lấy counters:', e);
            this.counters = [
                { id: 1, name: "Quầy 1", code: "1", currentNumber: 1 },
                { id: 2, name: "Quầy 2", code: "2", currentNumber: 0 }
            ];
        }
    }

    async fetchAll() {
        if (this.isLoading) return;
        this.isLoading = true;
        try {
            await Promise.all([this.fetchSettings(), this.fetchCounters()]);
            this.render();
        } finally {
            this.isLoading = false;
        }
    }

    // ====================== SIGNALR ======================
    startSignalR() {
        if (typeof signalR === 'undefined') {
            console.error("SignalR chưa tải!");
            return;
        }

        const hubUrl = '/qms/queueHub';
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.hubConnection.on('ReceiveMessage', async () => {
			try {
				await this.fetchCounters();  // Gọi API lấy số mới
				this.renderCounters();       // Render NGAY
			} catch (err) {
				console.error('Lỗi cập nhật số:', err);
			}

			// BƯỚC 2: PHÁT ÂM THANH SAU (không block UI)
			this.fetchAndPlayPendingSpeech();
		});

        this.hubConnection.start()
            .then(() => console.log('SignalR kết nối OK'))
            .catch(err => console.error('SignalR lỗi:', err));
    }
	// Gọi riêng khi cần render nhanh
	async updateCountersUI() {
		try {
			await this.fetchCounters();
			this.renderCounters();
		} catch (err) {
			console.error('Lỗi render:', err);
		}
	}

    // ====================== TTS: TEXT → BASE64 ======================
    async convertTextToBase64Audio(text) {
        const apiUrl = `${this.settings.ttsProtocol}://${this.settings.ttsIpAddress}:${this.settings.ttsPort}/api/v1/Tts/tts`;

        try {
            const response = await fetch(apiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ text })
            });

            if (!response.ok) {
                throw new Error(`TTS API lỗi: ${response.status}`);
            }

            const data = await response.json();
            if (data && data.base64_Audio) {
                return `data:audio/wav;base64,${data.base64_Audio}`;
            } else {
                console.warn("Không có base64_Audio", data);
                return "";
            }
        } catch (err) {
            console.error("Lỗi gọi TTS API:", err);
            return "";
        }
    }

    // ====================== LẤY & PHÁT ÂM THANH TỪ DB ======================
    async fetchAndPlayPendingSpeech() {
        if (!this.settings.isActiveSpeechCall || this.isSpeaking) return;

        try {
            const res = await fetch('/qms/api/QueueSpeech/get-all');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);

            const speeches = await res.json();
            if (!Array.isArray(speeches) || speeches.length === 0) {
                return;
            }

            speeches.forEach(s => {
                if (s.textToSpeech && !this.speechQueue.find(q => q.id === s.id)) {
                    this.speechQueue.push({ id: s.id, text: s.textToSpeech });
                }
            });

            if (!this.isSpeaking) await this.playNextSpeech();

        } catch (err) {
            console.error("Lỗi lấy QueueSpeech:", err);
        }
    }

    async playNextSpeech() {
        if (this.speechQueue.length === 0) {
            this.isSpeaking = false;
            return;
        }
        this.isSpeaking = true;

        const item = this.speechQueue.shift();

        try {
            // 1. GỌI TTS
            const audioUrl = await this.convertTextToBase64Audio(item.text);
            if (!audioUrl) throw new Error("TTS trả về rỗng");

            // 2. PHÁT ÂM THANH
            await window.playBase64AudioWithIntro(
                //this.settings.speechIntroUrl,
                audioUrl
            );

            // 3. CẬP NHẬT DB – GỬI TOÀN BỘ MODEL
            const updatePayload = {
                id: item.id,
                text: item.text,
                isCompleted: true
                // Thêm các field khác nếu cần (createdDate, counterId, v.v.)
            };

            const res = await fetch(`/qms/api/QueueSpeech/update-queuespeech/${item.id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updatePayload) // GỬI ĐÚNG MODEL
            });


            if (!res.ok) {
                const err = await res.text();
                throw new Error(`Cập nhật DB lỗi: ${res.status} - ${err}`);
            }


        } catch (err) {
            console.error("Lỗi phát âm thanh hoặc cập nhật:", err);
        }

        await this.playNextSpeech();
    }

    // ====================== RENDER ======================
    render() {
        this.renderFooter();
        this.renderCounters();
    }

    renderFooter() {
        const m = document.getElementById('footerMarquee');
        if (m) {
            m.textContent = this.settings.footerTextCountersMainView || "Đang tải...";
            m.style.color = this.settings.footerTextColor || "#fff";
            m.style.fontSize = `${this.settings.footerTextFontSize || 16}px`;
        }
    }

    renderCounters() {
        const grid = document.getElementById('counter-grid');
        if (!grid) return;
        grid.innerHTML = '';

        const max = this.settings.maxVisibleCounters || 12;
        const counters = Array.isArray(this.counters) ? this.counters : [];

        counters.slice(0, max).forEach(c => {
            const box = document.createElement('div');
            box.className = 'counter-box';
            const num = c.currentNumber > 0 ? String(c.currentNumber).padStart(3, '0') : '000';
            box.innerHTML = `
                <div class="counter-name">${c.name}</div>
                <div class="current-number">${c.code}${num}</div>
            `;
            grid.appendChild(box);
        });
    }

    // ====================== CLOCK + POLLING ======================
    startClock() {
        const update = () => {
            const n = new Date();
            const t = `${String(n.getHours()).padStart(2, '0')}:${String(n.getMinutes()).padStart(2, '0')}:${String(n.getSeconds()).padStart(2, '0')}`;
            const el = document.getElementById('clock');
            if (el) el.textContent = t;
        };
        setInterval(update, 1000);
        update();
    }

    startPolling() {
        setInterval(() => this.fetchAll(), 5000);
        setInterval(() => location.reload(), 300000);
    }

    setupResponsive() {
        const adj = () => {
            const w = window.innerWidth;
            document.body.style.fontSize = w > 1920 ? '18px' : w < 768 ? '14px' : '16px';
        };
        window.addEventListener('resize', adj);
        adj();
    }
}

// ====================== KHỞI CHẠY ======================
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => new CounterDisplay());
} else {
    new CounterDisplay();
}