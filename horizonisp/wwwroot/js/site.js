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

    function initSidebar() {
        const sidebar = document.querySelector('.mk-sidebar');
        const overlay = document.querySelector('.mk-sidebar-overlay');
        const toggle = document.querySelector('[data-mk-sidebar-toggle]');

        if (!sidebar || !toggle) {
            return;
        }

        const openSidebar = () => {
            sidebar.classList.add('open');
            overlay?.classList.add('show');
            document.body.style.overflow = 'hidden';
        };

        const closeSidebar = () => {
            sidebar.classList.remove('open');
            overlay?.classList.remove('show');
            document.body.style.overflow = '';
        };

        toggle.addEventListener('click', () => {
            if (sidebar.classList.contains('open')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });

        overlay?.addEventListener('click', closeSidebar);

        sidebar.querySelectorAll('.mk-nav-link').forEach((link) => {
            link.addEventListener('click', () => {
                if (window.innerWidth < 992) {
                    closeSidebar();
                }
            });
        });
    }

    function init() {
        initThemePicker();
        initSidebar();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
