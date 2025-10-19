// Blazor Server Drag-and-Drop Implementation
// This implementation works around Blazor's HTML5 drag-drop limitations by handling
// drag events entirely in JavaScript and calling back to Blazor for state updates.

window.DragDropInterop = {
    // Current state
    draggedTaskId: null,
    draggedElement: null,
    dotNetHelper: null,

    // Initialize drag-drop system
    initialize: function(dotNetHelper) {
        console.log('DragDrop system initialized');
        this.dotNetHelper = dotNetHelper;
        this.attachEventListeners();
    },

    // Attach event listeners to all task cards and drop zones
    attachEventListeners: function() {
        // Find all task cards
        const taskCards = document.querySelectorAll('.jira-task-card');
        taskCards.forEach(card => {
            this.makeCardDraggable(card);
        });

        // Find all drop zones
        const dropZones = document.querySelectorAll('.kanban-column-content');
        dropZones.forEach(zone => {
            this.makeDropZone(zone);
        });

        console.log(`Attached listeners to ${taskCards.length} cards and ${dropZones.length} drop zones`);
    },

    // Make a task card draggable
    makeCardDraggable: function(card) {
        const self = this;

        card.setAttribute('draggable', 'true');

        // Drag start - this must be synchronous to work with HTML5 drag-drop
        card.addEventListener('dragstart', function(e) {
            const taskId = this.id.replace('task-card-', '');
            self.draggedTaskId = taskId;
            self.draggedElement = this;

            // Set the drag data - MUST be synchronous
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', taskId);

            // Visual feedback
            this.style.opacity = '0.5';
            this.classList.add('dragging');

            console.log('Drag started:', taskId);
        });

        // Drag end
        card.addEventListener('dragend', function(e) {
            this.style.opacity = '1';
            this.classList.remove('dragging');

            // Remove all drag-over classes
            document.querySelectorAll('.kanban-column-content').forEach(zone => {
                zone.classList.remove('drag-over');
            });

            self.draggedTaskId = null;
            self.draggedElement = null;

            console.log('Drag ended');
        });
    },

    // Make a column content area a drop zone
    makeDropZone: function(zone) {
        const self = this;

        // Drag over - required to allow dropping
        zone.addEventListener('dragover', function(e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
        });

        // Drag enter - visual feedback
        zone.addEventListener('dragenter', function(e) {
            e.preventDefault();
            if (self.draggedElement && !this.contains(self.draggedElement.parentElement)) {
                this.classList.add('drag-over');
            }
        });

        // Drag leave - remove visual feedback
        zone.addEventListener('dragleave', function(e) {
            // Only remove highlight if we're leaving the drop zone (not entering a child)
            if (e.target === this) {
                this.classList.remove('drag-over');
            }
        });

        // Drop - handle the drop
        zone.addEventListener('drop', function(e) {
            e.preventDefault();
            this.classList.remove('drag-over');

            const taskId = e.dataTransfer.getData('text/plain');
            if (!taskId) {
                console.error('No task ID in drop event');
                return;
            }

            // Get the column ID from the data-column-id attribute
            const columnId = this.getAttribute('data-column-id');

            if (!columnId) {
                console.error('Could not determine target column ID from drop zone');
                return;
            }

            console.log(`Dropped task ${taskId} into column ${columnId}`);

            // Call back to Blazor to handle the column update
            if (self.dotNetHelper) {
                self.dotNetHelper.invokeMethodAsync('OnTaskDropped', taskId, columnId)
                    .then(success => {
                        if (success) {
                            console.log('Task column updated successfully');
                        } else {
                            console.error('Failed to update task column');
                        }
                    })
                    .catch(error => {
                        console.error('Error updating task column:', error);
                    });
            }
        });
    },

    // Refresh listeners after Blazor re-renders
    refresh: function() {
        console.log('Refreshing drag-drop listeners');
        this.attachEventListeners();
    },

    // Cleanup
    dispose: function() {
        this.draggedTaskId = null;
        this.draggedElement = null;
        this.dotNetHelper = null;
        console.log('DragDrop disposed');
    }
};
