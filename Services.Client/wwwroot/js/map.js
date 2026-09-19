if (!window.appMapState) {
    window.appMapState = {
        map: null,
        marker: null
    };

// Estado persistente de la conexión (icono / etiqueta)
function ensureConnStatusEl() {
    let el = document.getElementById('map-conn-status');
    const container = document.getElementById('map');
    if (!container) return null;
    if (!el) {
        el = document.createElement('div');
        el.id = 'map-conn-status';
        el.style.position = 'absolute';
        el.style.bottom = '10px';
        el.style.left = '10px';
        el.style.zIndex = '1000';
        el.style.display = 'flex';
        el.style.alignItems = 'center';
        el.style.gap = '8px';
        el.style.background = 'rgba(255,255,255,0.95)';
        el.style.padding = '6px 10px';
        el.style.borderRadius = '8px';
        el.style.boxShadow = '0 2px 6px rgba(0,0,0,0.15)';
        el.style.fontSize = '13px';
        container.style.position = 'relative';

        const dot = document.createElement('span');
        dot.id = 'map-conn-dot';
        dot.style.width = '10px';
        dot.style.height = '10px';
        dot.style.borderRadius = '50%';
        dot.style.display = 'inline-block';
        dot.style.background = '#999';
        el.appendChild(dot);

        const text = document.createElement('span');
        text.id = 'map-conn-text';
        text.textContent = '';
        el.appendChild(text);

        container.appendChild(el);
    }
    return el;
}

function setConnectionStatus(state, message) {
    const el = ensureConnStatusEl();
    if (!el) return;
    const dot = document.getElementById('map-conn-dot');
    const text = document.getElementById('map-conn-text');
    // Actualizar texto
    text.textContent = message || '';
    // Reset clases en el punto
    dot.classList.remove('connecting', 'connected', 'reconnecting', 'disconnected');
    // Añadir clase por estado para estilizar y animar
    switch (state) {
        case 'connecting':
            dot.classList.add('connecting');
            break;
        case 'connected':
            dot.classList.add('connected');
            break;
        case 'reconnecting':
            dot.classList.add('reconnecting');
            break;
        case 'disconnected':
            dot.classList.add('disconnected');
            break;
        default:
            break;
    }
    // Asegurar que el badge esté visible y con transición
    el.classList.add('map-conn-status--visible');
}
}

// Función utilitaria para mostrar un estado textual sencillo encima del mapa
window.showMapStatus = function (text) {
    let statusEl = document.getElementById('map-status');
    if (!statusEl) {
        const container = document.getElementById('map');
        if (!container) return;
        statusEl = document.createElement('div');
        statusEl.id = 'map-status';
        statusEl.className = 'map-status';
        container.appendChild(statusEl);
    }
    statusEl.textContent = text;
    // Mostrar con transición
    statusEl.classList.add('map-status--visible');
    clearTimeout(statusEl._hideTimeout);
    statusEl._hideTimeout = setTimeout(() => {
        statusEl.classList.remove('map-status--visible');
        // dejar tiempo para la transición antes de vaciar el texto
        setTimeout(() => { statusEl.textContent = ''; }, 250);
    }, 5000);
};

function limpiarMapaExistente() {
    if (window.appMapState.map !== null) {
        window.appMapState.map.remove();
        window.appMapState.map = null;
        window.appMapState.marker = null;
    }
}

// Función para el CLIENTE
window.iniciarEscuchaCliente = function (serviceId, hubUrl) {
    limpiarMapaExistente();

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { transport: signalR.HttpTransportType.WebSockets, withCredentials: true })
        .withAutomaticReconnect()
        .build();

    console.log("iniciarEscuchaCliente: intentando conectar al Hub:", hubUrl, " serviceId:", serviceId);
    setConnectionStatus('connecting', 'Conectando al hub...');

    // Eventos de estado de conexión
    connection.onreconnecting(error => {
        console.warn('SignalR Cliente reconectando:', error);
        setConnectionStatus('reconnecting', 'Reconectando...');
        window.showMapStatus('Reconectando al servidor...');
    });
    connection.onreconnected(connectionId => {
        console.log('SignalR Cliente reconectado:', connectionId);
        setConnectionStatus('connected', 'Conectado');
        window.showMapStatus('Reconectado');
    });
    connection.onclose(error => {
        console.error('SignalR Cliente cerrado:', error);
        setConnectionStatus('disconnected', 'Desconectado');
        window.showMapStatus('Conexión perdida');
    });

    connection.start().then(() => {
        console.log("SignalR Cliente conectado a:", hubUrl);
        setConnectionStatus('connected', 'Conectado');
        connection.invoke("JoinServiceGroup", serviceId).catch(e => console.error("JoinServiceGroup error (cliente):", e));

        connection.on("ReceiveLocation", (lat, lon) => {
            console.log("ReceiveLocation (cliente):", lat, lon);
            const container = document.getElementById('map');
            if (!container) return;

            if (!window.appMapState.map) {
                window.appMapState.map = L.map('map').setView([lat, lon], 16);
                L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '&copy; OpenStreetMap'
                }).addTo(window.appMapState.map);

                window.appMapState.marker = L.marker([lat, lon]).addTo(window.appMapState.map)
                    .bindPopup("¡El prestador está aquí!")
                    .openPopup();
            } else {
                window.appMapState.marker.setLatLng([lat, lon]);
                window.appMapState.map.setView([lat, lon], window.appMapState.map.getZoom());
            }
        });
    }).catch(err => {
        console.error("Error SignalR Cliente:", err);
        setConnectionStatus('disconnected', 'Error al conectar');
        window.showMapStatus('Error al conectar al servidor');
    });
};

// Función para el PRESTADOR
window.iniciarRastreoGPS = function (serviceId, hubUrl) {
    limpiarMapaExistente();

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { transport: signalR.HttpTransportType.WebSockets, withCredentials: true })
        .withAutomaticReconnect()
        .build();

    console.log("iniciarRastreoGPS: intentando conectar al Hub:", hubUrl, " serviceId:", serviceId);
    setConnectionStatus('connecting', 'Conectando al hub...');

    connection.onreconnecting(error => {
        console.warn('SignalR Prestador reconectando:', error);
        setConnectionStatus('reconnecting', 'Reconectando...');
        window.showMapStatus('Reconectando al servidor...');
    });
    connection.onreconnected(connectionId => {
        console.log('SignalR Prestador reconectado:', connectionId);
        setConnectionStatus('connected', 'Conectado');
        window.showMapStatus('Reconectado');
    });
    connection.onclose(error => {
        console.error('SignalR Prestador cerrado:', error);
        setConnectionStatus('disconnected', 'Desconectado');
        window.showMapStatus('Conexión perdida');
    });

    connection.start().then(() => {
        console.log("SignalR Prestador conectado a:", hubUrl);
        setConnectionStatus('connected', 'Conectado');
        connection.invoke("JoinServiceGroup", serviceId).catch(e => console.error("JoinServiceGroup error (prestador):", e));

        if (navigator.geolocation) {
            navigator.geolocation.watchPosition(
                position => {
                    const lat = position.coords.latitude;
                    const lon = position.coords.longitude;
                    console.log("GPS posición detectada:", lat, lon);

                    const container = document.getElementById('map');
                    if (!container) return;

                    if (!window.appMapState.map) {
                        window.appMapState.map = L.map('map').setView([lat, lon], 16);
                        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                            maxZoom: 19,
                            attribution: '&copy; OpenStreetMap'
                        }).addTo(window.appMapState.map);

                        window.appMapState.marker = L.marker([lat, lon]).addTo(window.appMapState.map)
                            .bindPopup("¡Estás transmitiendo aquí!")
                            .openPopup();
                    } else {
                        window.appMapState.marker.setLatLng([lat, lon]);
                        window.appMapState.map.setView([lat, lon], window.appMapState.map.getZoom());
                    }

                    connection.invoke("SendLocation", serviceId, lat, lon)
                        .then(() => console.log("SendLocation enviado:", lat, lon))
                        .catch(err => console.error("Error al enviar ubicación:", err));
                },
                error => console.error("Error GPS:", error),
                { enableHighAccuracy: true, maximumAge: 0, timeout: 5000 }
            );
        }
    }).catch(err => console.error("Error SignalR Prestador:", err));
};