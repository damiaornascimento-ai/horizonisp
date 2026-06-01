(function () {
    const STORAGE_KEY = 'horizon-theme';
    const DEFAULT_THEME = 'azul';

    const THEME_COLORS = {
        azul: '#0d6efd',
        verde: '#198754',
        roxo: '#6f42c1',
        laranja: '#e8590c',
        indigo: '#4338ca'
    };

    function applyTheme(theme) {
        if (!THEME_COLORS[theme]) {
            theme = DEFAULT_THEME;
        }

        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);

        document.querySelectorAll('.theme-option').forEach(function (btn) {
            var isActive = btn.dataset.theme === theme;
            btn.classList.toggle('active', isActive);
            btn.setAttribute('aria-pressed', isActive ? 'true' : 'false');
        });

        var dot = document.querySelector('.theme-current-dot');
        if (dot) {
            dot.style.backgroundColor = THEME_COLORS[theme];
        }
    }

    function initThemePicker() {
        var saved = localStorage.getItem(STORAGE_KEY) || DEFAULT_THEME;
        applyTheme(saved);

        document.querySelectorAll('.theme-option').forEach(function (btn) {
            btn.addEventListener('click', function () {
                applyTheme(btn.dataset.theme);
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initThemePicker);
    } else {
        initThemePicker();
    }
})();
