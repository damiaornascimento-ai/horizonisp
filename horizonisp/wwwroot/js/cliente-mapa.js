window.horizonMapa = (function () {
    const defaultCenter = [-23.5505, -46.6333];
    const defaultZoom = 12;
    const detailZoom = 17;

    function formatCoord(value) {
        return Number(value).toFixed(6);
    }

    function criarMapa(elementId, center, zoom) {
        const map = L.map(elementId, { scrollWheelZoom: true }).setView(center, zoom);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);
        return map;
    }

    function iniciarCliente(config) {
        const latInput = document.getElementById('inputLatitude');
        const lngInput = document.getElementById('inputLongitude');
        const statusEl = document.getElementById('mapaStatus');
        const coordEl = document.getElementById('mapaCoordenadas');
        const btnGps = document.getElementById('btnGps');
        const btnManual = document.getElementById('btnManual');

        if (!latInput || !lngInput) {
            return;
        }

        let lat = config.latitude;
        let lng = config.longitude;
        const hasPosition = lat !== null && lng !== null;
        const center = hasPosition ? [lat, lng] : defaultCenter;
        const zoom = hasPosition ? detailZoom : defaultZoom;

        const map = criarMapa('mapaInstalacao', center, zoom);
        let marker = null;
        let manualMode = false;

        function atualizarCoordenadas(newLat, newLng) {
            lat = newLat;
            lng = newLng;
            latInput.value = formatCoord(newLat);
            lngInput.value = formatCoord(newLng);
            coordEl.textContent = `Lat: ${formatCoord(newLat)} · Lng: ${formatCoord(newLng)}`;
        }

        function colocarMarcador(newLat, newLng, moverMapa) {
            if (marker) {
                marker.setLatLng([newLat, newLng]);
            } else {
                marker = L.marker([newLat, newLng], { draggable: true }).addTo(map);
                marker.on('dragend', () => {
                    const pos = marker.getLatLng();
                    atualizarCoordenadas(pos.lat, pos.lng);
                    statusEl.textContent = 'Posição ajustada. Salve para confirmar.';
                });
            }

            atualizarCoordenadas(newLat, newLng);

            if (moverMapa) {
                map.setView([newLat, newLng], detailZoom);
            }
        }

        if (hasPosition) {
            colocarMarcador(lat, lng, false);
            statusEl.textContent = 'Arraste o marcador ou clique no mapa para ajustar.';
        }

        map.on('click', (e) => {
            if (!manualMode && !marker) {
                statusEl.textContent = 'Ative "Marcar manualmente no mapa" ou use o GPS.';
                return;
            }

            colocarMarcador(e.latlng.lat, e.latlng.lng, false);
            statusEl.textContent = 'Posição definida no mapa. Salve para confirmar.';
        });

        btnManual?.addEventListener('click', () => {
            manualMode = true;
            statusEl.textContent = 'Clique no mapa no ponto da instalação ou arraste o marcador.';
            btnManual.classList.remove('btn-outline-primary');
            btnManual.classList.add('btn-primary');
        });

        btnGps?.addEventListener('click', () => {
            if (!navigator.geolocation) {
                statusEl.textContent = 'Geolocalização não disponível neste dispositivo.';
                return;
            }

            statusEl.textContent = 'Obtendo localização do dispositivo...';
            btnGps.disabled = true;

            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    manualMode = true;
                    colocarMarcador(pos.coords.latitude, pos.coords.longitude, true);
                    statusEl.textContent = 'Localização do dispositivo capturada. Ajuste se necessário e salve.';
                    btnGps.disabled = false;
                },
                (err) => {
                    const mensagens = {
                        1: 'Permissão de localização negada.',
                        2: 'Posição indisponível.',
                        3: 'Tempo esgotado ao obter GPS.'
                    };
                    statusEl.textContent = mensagens[err.code] || 'Não foi possível obter a localização.';
                    btnGps.disabled = false;
                },
                { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
            );
        });

        if (config.endereco && !hasPosition) {
            fetch(`https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(config.endereco)}`, {
                headers: { 'Accept-Language': 'pt-BR' }
            })
                .then((r) => r.json())
                .then((results) => {
                    if (results?.length && !marker) {
                        const item = results[0];
                        map.setView([parseFloat(item.lat), parseFloat(item.lon)], 15);
                        statusEl.textContent = 'Mapa centralizado no endereço do cliente. Use GPS ou marque manualmente.';
                    }
                })
                .catch(() => { /* silencioso */ });
        }

        setTimeout(() => map.invalidateSize(), 200);
    }

    function iniciarGeral(config) {
        const map = criarMapa('mapaClientes', defaultCenter, 5);
        const bounds = [];

        (config.clientes || []).forEach((cliente) => {
            const marker = L.marker([cliente.latitude, cliente.longitude]).addTo(map);
            const popup = `
                <strong>${cliente.nome}</strong><br/>
                ${cliente.endereco}<br/>
                ${cliente.cidade}<br/>
                <a href="${config.urlLocalizacao.replace('__id__', cliente.id)}">Abrir instalação</a>
            `;
            marker.bindPopup(popup);
            bounds.push([cliente.latitude, cliente.longitude]);
        });

        if (bounds.length === 1) {
            map.setView(bounds[0], detailZoom);
        } else if (bounds.length > 1) {
            map.fitBounds(bounds, { padding: [40, 40] });
        }

        setTimeout(() => map.invalidateSize(), 200);
    }

    return { iniciarCliente, iniciarGeral };
})();
