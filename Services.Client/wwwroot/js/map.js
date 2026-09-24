// Objeto global para mantener el estado del mapa y evitar conflictos
window.appMapState = {
    map: null,
    marker: null,
    clientConnection: null
};

// Limpia el mapa previo si existiera
function limpiarMapaExistente() {
    if (window.appMapState.map) {
        window.appMapState.map.remove();
        window.appMapState.map = null;
        window.appMapState.marker = null;
    }
}

// Función auxiliar para mostrar estados
window.showMapStatus = function (mensaje) {
    console.log("Estado del Mapa:", mensaje);
};

// Inicialización básica del mapa (Leaflet)
window.testInitMap = function (lat, lon) {
    const container = document.getElementById('map');
    if (!container) {
        console.error("No se encontró el elemento #map en el DOM.");
        return;
    }

    limpiarMapaExistente();

    // Crear instancia del mapa centrada en coordenadas iniciales (San Miguel de Tucumán por defecto)
    window.appMapState.map = L.map('map').setView([lat, lon], 15);

    // Añadir capa de OpenStreetMap
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(window.appMapState.map);

    // Forzar redimensionamiento para evitar pantallas grises
    setTimeout(() => {
        if (window.appMapState.map) {
            window.appMapState.map.invalidateSize();
        }
    }, 300);
};

// Actualizar o crear marcador (Usado por el cliente al recibir ubicaciones)
window.updateMapMarker = function (lat, lon) {
    if (!window.appMapState.map) {
        window.testInitMap(lat, lon);
    }

    const latLng = [lat, lon];
    if (!window.appMapState.marker) {
        window.appMapState.marker = L.marker(latLng)
            .addTo(window.appMapState.map)
            .bindPopup("¡Ubicación del prestador en tiempo real!")
            .openPopup();
    } else {
        window.appMapState.marker.setLatLng(latLng);
    }

    window.appMapState.map.setView(latLng, window.appMapState.map.getZoom());
    window.appMapState.map.invalidateSize();
};

// ==========================================
// ROL 1: PRESTADOR (Envía su ubicación GPS)
// ==========================================
window.iniciarRastreoGPS = function (serviceId, hubUrl) {
    limpiarMapaExistente();

    // Mapa inicial centrado en San Miguel de Tucumán hasta obtener GPS real
    window.testInitMap(-26.8083, -65.2176);

    if (!navigator.geolocation) {
        alert("La geolocalización no es compatible con este navegador.");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { transport: signalR.HttpTransportType.WebSockets })
        .withAutomaticReconnect()
        .build();

    connection.start().then(() => {
        console.log("SignalR Prestador conectado.");
        connection.invoke("JoinServiceGroup", serviceId).catch(err => console.error(err));

        navigator.geolocation.watchPosition(
            position => {
                const lat = position.coords.latitude;
                const lon = position.coords.longitude;

                if (window.appMapState.map) {
                    const latLng = [lat, lon];
                    if (!window.appMapState.marker) {
                        window.appMapState.marker = L.marker(latLng)
                            .addTo(window.appMapState.map)
                            .bindPopup("¡Estás transmitiendo aquí!")
                            .openPopup();
                    } else {
                        window.appMapState.marker.setLatLng(latLng);
                    }
                    window.appMapState.map.setView(latLng, 16);
                }

                connection.invoke("SendLocation", serviceId, lat, lon)
                    .catch(err => console.error("Error al enviar ubicación via SignalR:", err));
            },
            error => {
                console.warn("Aviso de GPS en rastreo: " + error.message);
            },
            { enableHighAccuracy: true, maximumAge: 0, timeout: 5000 }
        );
    }).catch(err => {
        console.error("Error al conectar SignalR en el prestador:", err);
    });
};

// ==========================================
// ROL 2: CLIENTE (Escucha la ubicación del prestador)
// ==========================================
window.iniciarEscuchaCliente = function (serviceId, hubUrl) {
    limpiarMapaExistente();

    // Inicializar mapa en San Miguel de Tucumán por defecto hasta recibir la primera señal
    window.testInitMap(-26.8083, -65.2176);

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, { transport: signalR.HttpTransportType.WebSockets })
        .withAutomaticReconnect()
        .build();

    window.appMapState.clientConnection = connection;

    // Escuchar el evento proveniente del Hub de SignalR
    connection.on("ReceiveLocation", (lat, lon) => {
        console.log(`Ubicación recibida del prestador -> Lat: ${lat}, Lon: ${lon}`);
        window.updateMapMarker(lat, lon);
    });

    connection.start().then(() => {
        console.log("SignalR Cliente conectado correctamente.");
        connection.invoke("JoinServiceGroup", serviceId).catch(err => console.error(err));
    }).catch(err => {
        console.error("Error al conectar SignalR en el cliente:", err);
    });
};

// ==========================================
// FUNCIÓN CORREGIDA: Obtiene GPS o usa Tucumán de respaldo
// ==========================================
window.obtenerCoordenadasActuales = function () {
    return new Promise((resolve) => {
        if (!navigator.geolocation) {
            console.warn("Geolocalización no soportada. Usando San Miguel de Tucumán por defecto.");
            resolve({ latitude: -26.8083, longitude: -65.2176 });
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => resolve({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            }),
            error => {
                console.warn("Aviso de GPS: " + error.message + ". Usando San Miguel de Tucumán como respaldo.");
                // Ubicación por defecto de respaldo (San Miguel de Tucumán)
                resolve({ latitude: -26.8083, longitude: -65.2176 });
            },
            { enableHighAccuracy: false, timeout: 7000, maximumAge: 0 }
        );
    });
};