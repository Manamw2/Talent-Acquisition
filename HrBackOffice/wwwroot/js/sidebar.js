document.addEventListener('DOMContentLoaded', function () {
    // Get elements
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarBackdrop = document.getElementById('sidebar-backdrop');
    const hamburgerMenu = document.getElementById('hamburger-menu');
    const mainContent = document.getElementById('main-content');

    // Toggle sidebar function
    function toggleSidebar() {
        sidebar.classList.toggle('active');
        sidebarBackdrop.classList.toggle('active');

        if (hamburgerMenu) {
            hamburgerMenu.classList.toggle('shifted');
        }

        // Log states to debug
        console.log('Sidebar toggle clicked');
        console.log('Sidebar active:', sidebar.classList.contains('active'));
        console.log('Backdrop active:', sidebarBackdrop.classList.contains('active'));
    }

    // Toggle sidebar when hamburger is clicked
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            toggleSidebar();
        });
    }

    // Close sidebar when clicking on backdrop
    if (sidebarBackdrop) {
        sidebarBackdrop.addEventListener('click', function () {
            toggleSidebar();
        });
    }

    // Close sidebar when clicking on a nav link (for mobile)
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992 && sidebar.classList.contains('active')) {
                toggleSidebar();
            }
        });
    });

    // Set active class to current page nav link
    const currentLocation = window.location.pathname;

    navLinks.forEach(link => {
        const linkPath = link.getAttribute('href');

        // Check if the path contains controller/action pattern
        if (linkPath && linkPath !== '/' &&
            ((currentLocation === linkPath) ||
                (linkPath.includes('/') && currentLocation.includes(linkPath)))) {
            link.classList.add('active');
        }
    });

    // Check window size on load
    function checkWindowSize() {
        if (window.innerWidth >= 992) {
            sidebar.classList.remove('active');
            sidebarBackdrop.classList.remove('active');
            if (hamburgerMenu) hamburgerMenu.classList.remove('shifted');
            mainContent.style.marginLeft = '250px';
        } else {
            mainContent.style.marginLeft = '0';
        }
    }

    // Initial check
    checkWindowSize();

    // Listen for window resize
    window.addEventListener('resize', checkWindowSize);

    // Fix for iOS and Android touch events
    document.body.addEventListener('touchstart', function (e) {
        if (sidebar.classList.contains('active') &&
            !sidebar.contains(e.target) &&
            !hamburgerMenu.contains(e.target)) {
            e.preventDefault();
            toggleSidebar();
        }
    }, { passive: false });
});