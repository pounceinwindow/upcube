(function () {
    const chat = document.querySelector('[data-inquiry-chat]');
    if (!chat || typeof signalR === 'undefined') return;

    const inquiryId = Number.parseInt(chat.dataset.inquiryId, 10);
    const currentUserId = chat.dataset.currentUserId || '';
    const messages = chat.querySelector('[data-messages]');
    const form = chat.querySelector('[data-message-form]');
    const input = chat.querySelector('[data-message-input]');
    const submit = chat.querySelector('[data-message-submit]');
    const error = chat.querySelector('[data-chat-error]');
    const status = chat.querySelector('[data-chat-status]');

    if (!Number.isInteger(inquiryId) || !messages || !form || !input || !submit) return;

    const maxLength = Number.parseInt(input.getAttribute('maxlength') || '4000', 10);
    let isSending = false;
    let isJoined = false;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/inquiries')
        .withAutomaticReconnect()
        .build();

    function setError(message) {
        if (!error) return;

        if (!message) {
            error.classList.add('d-none');
            error.textContent = '';
            return;
        }

        error.textContent = message;
        error.classList.remove('d-none');
    }

    function setStatus(message) {
        if (status) status.textContent = message || '';
    }

    function updateSubmitState() {
        submit.disabled = isSending || input.value.trim().length === 0;
    }

    function hasMessage(messageId) {
        if (!messageId) return false;

        return Array.from(messages.querySelectorAll('[data-message-id]'))
            .some(function (item) {
                return item.dataset.messageId === messageId;
            });
    }

    function formatDate(value) {
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return value || '';

        return date.toLocaleString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    function appendMessage(payload) {
        if (!payload || Number(payload.inquiryId) !== inquiryId) return;

        const messageId = payload.messageId == null ? '' : String(payload.messageId);
        if (hasMessage(messageId)) return;

        const isOwn = payload.senderId === currentUserId;
        const row = document.createElement('div');
        row.className = 'uc-chat-message-row ' + (isOwn ? 'is-own' : 'is-other');
        if (messageId) row.dataset.messageId = messageId;

        const bubble = document.createElement('div');
        bubble.className = 'uc-chat-message';

        const meta = document.createElement('div');
        meta.className = 'small text-variant-1 mb-1';
        meta.textContent = (isOwn ? 'Вы' : payload.senderName || 'Собеседник') + ' · ' + formatDate(payload.sentAt);

        const text = document.createElement('div');
        text.textContent = payload.text || '';

        bubble.appendChild(meta);
        bubble.appendChild(text);
        row.appendChild(bubble);
        messages.appendChild(row);
        messages.scrollTop = messages.scrollHeight;
    }

    async function joinInquiry() {
        await connection.invoke('JoinInquiry', inquiryId);
        isJoined = true;
        setStatus('');
    }

    connection.on('ReceiveMessage', appendMessage);

    connection.onreconnecting(function () {
        isJoined = false;
        setStatus('Переподключение к чату...');
    });

    connection.onreconnected(async function () {
        try {
            await joinInquiry();
        } catch (err) {
            setError(err.message || 'Не удалось переподключиться к чату.');
        }
    });

    connection.onclose(function () {
        isJoined = false;
        setStatus('Realtime-соединение закрыто. Обычная отправка всё ещё доступна.');
    });

    connection.start()
        .then(joinInquiry)
        .catch(function (err) {
            setStatus('Realtime-чат недоступен. Сообщение будет отправлено обычной формой.');
            if (window.console) window.console.warn(err);
        });

    input.addEventListener('input', function () {
        setError('');
        updateSubmitState();
    });

    form.addEventListener('submit', async function (event) {
        const text = input.value.trim();

        if (!text) {
            event.preventDefault();
            setError('Введите сообщение перед отправкой.');
            updateSubmitState();
            return;
        }

        if (text.length > maxLength) {
            event.preventDefault();
            setError('Сообщение слишком длинное. Максимум ' + maxLength + ' символов.');
            return;
        }

        if (connection.state !== signalR.HubConnectionState.Connected || !isJoined) {
            return;
        }

        event.preventDefault();
        isSending = true;
        setError('');
        updateSubmitState();

        try {
            await connection.invoke('SendMessage', inquiryId, text);
            input.value = '';
        } catch (err) {
            setError(err.message || 'Не удалось отправить сообщение.');
        } finally {
            isSending = false;
            updateSubmitState();
        }
    });

    updateSubmitState();
    messages.scrollTop = messages.scrollHeight;
})();
