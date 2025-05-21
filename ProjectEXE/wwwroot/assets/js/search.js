// This file contains the JavaScript code to manage the search functionality for the second-hand clothing e-commerce website.

document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('search-input');
    const searchResultsContainer = document.getElementById('search-results');
    const products = []; // This should be populated with actual product data

    searchInput.addEventListener('input', function() {
        const query = searchInput.value.toLowerCase();
        searchResultsContainer.innerHTML = '';

        if (query) {
            const filteredProducts = products.filter(product => 
                product.name.toLowerCase().includes(query) || 
                product.category.toLowerCase().includes(query)
            );

            filteredProducts.forEach(product => {
                const resultItem = document.createElement('div');
                resultItem.classList.add('search-result-item');
                resultItem.innerHTML = `<a href="pages/product-details.html?id=${product.id}">${product.name}</a>`;
                searchResultsContainer.appendChild(resultItem);
            });
        }
    });
});