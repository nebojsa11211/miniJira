// User Selection Component JavaScript Functions
window.focusElement = function(element) {
    if (element) {
        element.focus();
        return true;
    }
    return false;
};

// Add keyboard navigation support
window.initializeUserSelection = function() {
    document.addEventListener('keydown', function(e) {
        // Global keyboard shortcuts for user selection
        if (e.key === '/' && !e.ctrlKey && !e.altKey && !e.shiftKey) {
            const searchInput = document.querySelector('.search-input');
            if (searchInput && document.activeElement !== searchInput) {
                e.preventDefault();
                searchInput.focus();
            }
        }
    });
};
