document.addEventListener('DOMContentLoaded', () => {
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
});
