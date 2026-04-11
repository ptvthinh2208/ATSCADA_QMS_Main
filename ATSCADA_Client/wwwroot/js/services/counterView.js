const urlParams = new URLSearchParams(window.location.search);
const counterId = urlParams.get('counterId') || 1;

let connection;
let isUpdating = false;
let lastNumber = null;

const statusEl = document.getElementById('status');
const counterNameEl = document.getElementById('counterName');
const numberEl = document.getElementById('currentNumber');



async function updateDisplay() {
    if (isUpdating) return;
    isUpdating = true;

    try {
        const res = await fetch(`/qms/api/Counter/get-counter-by-id/${counterId}`);
        if (!res.ok) throw new Error("API lỗi");

        const data = await res.json();
        const fullName = `${data.name}`;
        const newNumber = data.code + String(data.currentNumber).padStart(3, '0');

        // Cập nhật tên quầy
        if (counterNameEl.textContent !== fullName) {
            counterNameEl.textContent = fullName;
        }

        

        numberEl.textContent = newNumber;
        lastNumber = newNumber;

    } catch (err) {
        console.error(err);
        numberEl.textContent = '---';
    } finally {
        isUpdating = false;
    }
}

// SignalR
async function startSignalR() {
    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/qms/queueHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", () => updateDisplay());

        connection.onreconnecting(() => {
            statusEl.className = 'status disconnected';
            statusEl.textContent = 'Mất kết nối...';
        });

        connection.onreconnected(() => {
            statusEl.className = 'status connected';
            statusEl.textContent = 'Đã kết nối';
            updateDisplay();
        });

        await connection.start();
        statusEl.className = 'status connected';
        statusEl.textContent = 'Đã kết nối';
        updateDisplay();

    } catch (err) {
        statusEl.className = 'status disconnected';
        statusEl.textContent = 'Lỗi kết nối';
        setTimeout(startSignalR, 5000);
    }
}

// Khởi động
window.addEventListener('load', () => {
    startSignalR();
    setInterval(() => {
        if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
            updateDisplay();
        }
    }, 8000);
});