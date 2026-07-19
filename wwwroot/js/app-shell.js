// Sidebar collapse (desktop) / drawer (mobile) toggle — čist JS, ne zavisi od Blazor state-a.
(function () {
    const STORAGE_KEY = 'odetta.sidebar';
    const MOBILE_BREAKPOINT = 900;

    function getShell() {
        return document.getElementById('appShell');
    }

    function toggle() {
        const shell = getShell();
        if (!shell) return;

        if (window.innerWidth < MOBILE_BREAKPOINT) {
            shell.classList.toggle('mobile-open');
        } else {
            const collapsed = shell.classList.toggle('collapsed');
            localStorage.setItem(STORAGE_KEY, collapsed ? 'collapsed' : 'expanded');
        }
    }

    function closeMobile() {
        const shell = getShell();
        if (shell) shell.classList.remove('mobile-open');
    }

    function init() {
        const shell = getShell();
        if (!shell) return;

        if (window.innerWidth >= MOBILE_BREAKPOINT && localStorage.getItem(STORAGE_KEY) === 'collapsed') {
            shell.classList.add('collapsed');
        }
    }

    window.odettaSidebar = { toggle, closeMobile, init };

    document.addEventListener('DOMContentLoaded', init);
})();
