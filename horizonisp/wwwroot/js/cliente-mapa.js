window.horizonMapa = (function () {
    const defaultCenter = [-23.5505, -46.6333];
    const defaultZoom = 12;
    const detailZoom = 17;
    const gpsPrecisaoIdeal = 15;
    const gpsPrecisaoAceitavel = 40;
    const gpsTempoMaxMs = 30000;
    const gpsTempoMinMs = 4000;

    function formatCoord(value) {
        return Number(value).toFixed(6);
    }

    function formatPrecisao(metros) {
        if (!Number.isFinite(metros)) {
            return '—';
        }
        return metros < 1000 ? `${Math.round(metros)} m` : `${(metros / 1000).toFixed(1)} km`;
    }

    function criarMapa(elementId, center, zoom) {
        const map = L.map(elementId, { scrollWheelZoom: true }).setView(center, zoom);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);

        const container = document.getElementById(elementId);
        if (container && typeof ResizeObserver !== 'undefined') {
            const observer = new ResizeObserver(() => map.invalidateSize());
            observer.observe(container);
        }

        window.addEventListener('resize', () => map.invalidateSize());

        return map;
    }

    function coordenadaValida(lat, lng) {
        return Number.isFinite(lat) && Number.isFinite(lng)
            && lat >= -90 && lat <= 90
            && lng >= -180 && lng <= 180;
    }

    function capturarGpsRefinado(onProgress, onSuccess, onError) {
        const opcoes = { enableHighAccuracy: true, timeout: 25000, maximumAge: 0 };
        let melhor = null;
        let watchId = null;
        let finalizado = false;
        const inicio = Date.now();

        function encerrar() {
            if (watchId !== null) {
                navigator.geolocation.clearWatch(watchId);
                watchId = null;
            }
        }

        function concluir() {
            if (finalizado) {
                return;
            }
            finalizado = true;
            encerrar();
            if (melhor) {
                onSuccess(melhor);
            } else {
                onError({ code: 3 });
            }
        }

        function avaliarLeitura(pos) {
            const leitura = {
                latitude: pos.coords.latitude,
                longitude: pos.coords.longitude,
                accuracy: pos.coords.accuracy,
                altitude: pos.coords.altitude,
                timestamp: pos.timestamp
            };

            if (!melhor || leitura.accuracy < melhor.accuracy) {
                melhor = leitura;
                onProgress(melhor);
            }

            const tempoDecorrido = Date.now() - inicio;
            const precisaoBoa = melhor.accuracy <= gpsPrecisaoIdeal;
            const precisaoRazoavel = melhor.accuracy <= gpsPrecisaoAceitavel;
            const tempoSuficiente = tempoDecorrido >= gpsTempoMinMs;

            if (precisaoBoa && tempoSuficiente) {
                concluir();
                return;
            }

            if (tempoDecorrido >= gpsTempoMaxMs) {
                concluir();
                return;
            }

            if (tempoSuficiente && precisaoRazoavel && tempoDecorrido >= 10000) {
                concluir();
            }
        }

        watchId = navigator.geolocation.watchPosition(
            avaliarLeitura,
            (err) => {
                if (finalizado) {
                    return;
                }
                finalizado = true;
                encerrar();
                if (melhor) {
                    onSuccess(melhor);
                } else {
                    onError(err);
                }
            },
            opcoes
        );

        setTimeout(() => {
            if (!finalizado) {
                concluir();
            }
        }, gpsTempoMaxMs + 500);
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
        const hasPosition = coordenadaValida(Number(lat), Number(lng));
        if (hasPosition) {
            lat = Number(lat);
            lng = Number(lng);
        }
        const center = hasPosition ? [lat, lng] : defaultCenter;
        const zoom = hasPosition ? detailZoom : defaultZoom;

        const map = criarMapa('mapaInstalacao', center, zoom);
        let marker = null;
        let circuloPrecisao = null;
        let manualMode = false;
        let gpsAtivo = false;

        function removerCirculoPrecisao() {
            if (circuloPrecisao) {
                map.removeLayer(circuloPrecisao);
                circuloPrecisao = null;
            }
        }

        function atualizarCirculoPrecisao(newLat, newLng, metros) {
            removerCirculoPrecisao();
            if (!Number.isFinite(metros) || metros <= 0) {
                return;
            }

            circuloPrecisao = L.circle([newLat, newLng], {
                radius: metros,
                color: '#0069b4',
                fillColor: '#00a8e8',
                fillOpacity: 0.12,
                weight: 1
            }).addTo(map);
        }

        function atualizarCoordenadas(newLat, newLng, precisaoMetros) {
            lat = newLat;
            lng = newLng;
            latInput.value = formatCoord(newLat);
            lngInput.value = formatCoord(newLng);

            const precisaoTexto = Number.isFinite(precisaoMetros)
                ? ` · Precisão: ±${formatPrecisao(precisaoMetros)}`
                : '';
            coordEl.textContent = `Lat: ${formatCoord(newLat)} · Lng: ${formatCoord(newLng)}${precisaoTexto}`;
        }

        function colocarMarcador(newLat, newLng, moverMapa, precisaoMetros) {
            if (marker) {
                marker.setLatLng([newLat, newLng]);
            } else {
                marker = L.marker([newLat, newLng], { draggable: true }).addTo(map);
                marker.on('dragend', () => {
                    const pos = marker.getLatLng();
                    removerCirculoPrecisao();
                    atualizarCoordenadas(pos.lat, pos.lng);
                    statusEl.textContent = 'Posição ajustada manualmente. Salve para confirmar.';
                });
            }

            atualizarCoordenadas(newLat, newLng, precisaoMetros);

            if (Number.isFinite(precisaoMetros)) {
                atualizarCirculoPrecisao(newLat, newLng, precisaoMetros);
            } else {
                removerCirculoPrecisao();
            }

            if (moverMapa) {
                const zoomAlvo = Number.isFinite(precisaoMetros) && precisaoMetros > 80 ? 16 : detailZoom;
                map.setView([newLat, newLng], zoomAlvo);
            }
        }

        function mensagemPrecisao(metros) {
            if (metros <= gpsPrecisaoIdeal) {
                return `GPS com boa precisão (±${formatPrecisao(metros)}). Ajuste o marcador se necessário e salve.`;
            }
            if (metros <= gpsPrecisaoAceitavel) {
                return `GPS capturado (±${formatPrecisao(metros)}). Recomendado arrastar o marcador para o ponto exato da instalação.`;
            }
            return `GPS com baixa precisão (±${formatPrecisao(metros)}). Arraste o marcador para a posição correta antes de salvar.`;
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

            manualMode = true;
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

            if (gpsAtivo) {
                return;
            }

            manualMode = true;
            gpsAtivo = true;
            btnGps.disabled = true;
            statusEl.textContent = 'Buscando sinal GPS... aguarde ao ar livre, com o celular parado.';

            capturarGpsRefinado(
                (leitura) => {
                    colocarMarcador(leitura.latitude, leitura.longitude, true, leitura.accuracy);
                    statusEl.textContent = `Refinando GPS... precisão atual ±${formatPrecisao(leitura.accuracy)}.`;
                },
                (leitura) => {
                    gpsAtivo = false;
                    btnGps.disabled = false;
                    colocarMarcador(leitura.latitude, leitura.longitude, true, leitura.accuracy);
                    statusEl.textContent = mensagemPrecisao(leitura.accuracy);
                },
                (err) => {
                    gpsAtivo = false;
                    btnGps.disabled = false;
                    const mensagens = {
                        1: 'Permissão de localização negada. Libere o acesso ao GPS nas configurações do navegador.',
                        2: 'Sinal GPS indisponível. Tente ao ar livre ou marque manualmente.',
                        3: 'Tempo esgotado ao obter GPS. Tente novamente ou marque manualmente.'
                    };
                    statusEl.textContent = mensagens[err.code] || 'Não foi possível obter a localização.';
                }
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

        document.getElementById('formLocalizacao')?.addEventListener('submit', (e) => {
            if (!latInput.value?.trim() || !lngInput.value?.trim()) {
                e.preventDefault();
                statusEl.textContent = 'Marque a posição no mapa antes de salvar.';
                return;
            }

            const latNum = Number(latInput.value);
            const lngNum = Number(lngInput.value);
            if (!Number.isFinite(latNum) || !Number.isFinite(lngNum)) {
                e.preventDefault();
                statusEl.textContent = 'Coordenadas inválidas. Marque novamente no mapa.';
            }
        });

        setTimeout(() => map.invalidateSize(), 200);
    }

    function iniciarGeral(config) {
        const map = criarMapa('mapaClientes', defaultCenter, 5);
        const bounds = [];

        (config.clientes || []).forEach((cliente) => {
            const lat = Number(cliente.latitude ?? cliente.Latitude);
            const lng = Number(cliente.longitude ?? cliente.Longitude);
            const id = cliente.id ?? cliente.Id;

            if (!coordenadaValida(lat, lng)) {
                return;
            }

            const marker = L.marker([lat, lng]).addTo(map);
            const popup = `
                <strong>${cliente.nome ?? cliente.Nome ?? 'Cliente'}</strong><br/>
                ${cliente.endereco ?? cliente.Endereco ?? ''}<br/>
                ${cliente.cidade ?? cliente.Cidade ?? ''}<br/>
                <a href="${config.urlLocalizacao.replace('__id__', id)}">Abrir instalação</a>
            `;
            marker.bindPopup(popup);
            bounds.push([lat, lng]);
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
