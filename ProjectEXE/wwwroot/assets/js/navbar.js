// This file contains the JavaScript functionality for the navigation bar, including dropdown menus and mobile responsiveness.

document.addEventListener('DOMContentLoaded', function() {
    const navbarToggle = document.querySelector('.navbar-toggle');
    const navbarMenu = document.querySelector('.navbar-menu');

    // Toggle the navigation menu on mobile
    navbarToggle.addEventListener('click', function() {
        navbarMenu.classList.toggle('is-active');
    });

    // Close the menu when a link is clicked
    const navbarLinks = document.querySelectorAll('.navbar-menu a');
    navbarLinks.forEach(link => {
        link.addEventListener('click', function() {
            navbarMenu.classList.remove('is-active');
        });
    });

    // Dropdown functionality
    const dropdowns = document.querySelectorAll('.dropdown');
    dropdowns.forEach(dropdown => {
        dropdown.addEventListener('click', function() {
            this.classList.toggle('is-active');
        });
    });
});