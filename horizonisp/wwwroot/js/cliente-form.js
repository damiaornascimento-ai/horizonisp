(function () {
    function normalizarCep(valor) {
        return (valor || '').replace(/\D/g, '').slice(0, 8);
    }

    function formatarCep(valor) {
        const numeros = normalizarCep(valor);
        if (numeros.length <= 5) {
            return numeros;
        }

        return `${numeros.slice(0, 5)}-${numeros.slice(5)}`;
    }

    function montarEndereco(dados) {
        const partes = [];
        if (dados.logradouro) {
            partes.push(dados.logradouro);
        }

        if (dados.bairro) {
            partes.push(dados.bairro);
        }

        return partes.join(', ');
    }

    async function buscarCep(cep, statusEl) {
        const resposta = await fetch(`https://viacep.com.br/ws/${cep}/json/`, {
            headers: { Accept: 'application/json' }
        });

        if (!resposta.ok) {
            throw new Error('Falha na consulta do CEP.');
        }

        const dados = await resposta.json();
        if (dados.erro) {
            if (statusEl) {
                statusEl.textContent = 'CEP não encontrado.';
                statusEl.className = 'form-text text-danger cep-busca-status';
            }

            return null;
        }

        if (statusEl) {
            statusEl.textContent = 'Endereço preenchido automaticamente.';
            statusEl.className = 'form-text text-success cep-busca-status';
        }

        return dados;
    }

    function initFormularioCliente(form) {
        const cepInput = form.querySelector('input[name$=".Cep"]');
        if (!cepInput || cepInput.dataset.cepLookupInit === 'true') {
            return;
        }

        cepInput.dataset.cepLookupInit = 'true';

        const enderecoInput = form.querySelector('input[name$=".Endereco"]');
        const cidadeInput = form.querySelector('input[name$=".Cidade"]');
        const estadoInput = form.querySelector('input[name$=".Estado"]');
        const statusEl = form.querySelector('.cep-busca-status');

        let ultimoCepBuscado = normalizarCep(cepInput.value);
        let buscando = false;

        cepInput.addEventListener('input', function () {
            cepInput.value = formatarCep(cepInput.value);

            const cep = normalizarCep(cepInput.value);
            if (cep.length < 8) {
                ultimoCepBuscado = '';
            }

            if (statusEl) {
                statusEl.textContent = '';
                statusEl.className = 'form-text cep-busca-status';
            }

            if (cep.length === 8) {
                consultar();
            }
        });

        async function consultar() {
            const cep = normalizarCep(cepInput.value);
            if (cep.length !== 8 || buscando || cep === ultimoCepBuscado) {
                return;
            }

            buscando = true;
            ultimoCepBuscado = cep;

            if (statusEl) {
                statusEl.textContent = 'Buscando CEP...';
                statusEl.className = 'form-text text-muted cep-busca-status';
            }

            try {
                const dados = await buscarCep(cep, statusEl);
                if (!dados) {
                    return;
                }

                if (enderecoInput) {
                    enderecoInput.value = montarEndereco(dados);
                }

                if (cidadeInput) {
                    cidadeInput.value = dados.localidade || '';
                }

                if (estadoInput) {
                    estadoInput.value = dados.uf || '';
                }
            } catch {
                ultimoCepBuscado = '';
                if (statusEl) {
                    statusEl.textContent = 'Não foi possível consultar o CEP. Tente novamente.';
                    statusEl.className = 'form-text text-danger cep-busca-status';
                }
            } finally {
                buscando = false;
            }
        }

        cepInput.addEventListener('blur', consultar);

        cepInput.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                consultar();
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('form').forEach(initFormularioCliente);
    });
})();
