// Update your sidebar.js with these changes
document.addEventListener('DOMContentLoaded', function () {
    const hamburgerBtn = document.querySelector('.navbar-toggler');
    const hamburgerMenu = document.querySelector('.hamburger-menu');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('.main-content');

    // Create backdrop element
    const backdrop = document.createElement('div');
    backdrop.className = 'sidebar-backdrop';
    document.body.appendChild(backdrop);

    // Toggle sidebar
    hamburgerBtn.addEventListener('click', function () {
        sidebar.classList.toggle('active');
        backdrop.classList.toggle('active');
        hamburgerMenu.classList.toggle('shifted'); // Add this line to toggle hamburger position
    });

    // Close sidebar when clicking outside
    backdrop.addEventListener('click', function () {
        sidebar.classList.remove('active');
        backdrop.classList.remove('active');
        hamburgerMenu.classList.remove('shifted'); // Add this line
    });

    // Close sidebar when clicking a nav link (mobile only)
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                sidebar.classList.remove('active');
                backdrop.classList.remove('active');
                hamburgerMenu.classList.remove('shifted'); // Add this line
            }
        });
    });

    // Handle window resize
    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            sidebar.classList.remove('active');
            backdrop.classList.remove('active');
            hamburgerMenu.classList.remove('shifted'); // Add this line
        }
    });
});