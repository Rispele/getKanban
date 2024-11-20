document.addEventListener("DOMContentLoaded", () => {
    const draggables = document.querySelectorAll(".draggable");
    const dropzones = document.querySelectorAll(".dropzone");

    draggables.forEach(draggable => {
        draggable.addEventListener("dragstart", handleDragStart);
        draggable.addEventListener("dragend", handleDragEnd);
    });

    dropzones.forEach(dropzone => {
        dropzone.addEventListener("dragover", handleDragOver);
        dropzone.addEventListener("drop", handleDrop);
    });

    function handleDragStart(event) {
        event.target.classList.add("dragging");
    }

    function handleDragEnd(event) {
        event.target.classList.remove("dragging");
        const closestDropzone = getClosestDropzone(event.target);
        if (closestDropzone) {
            closestDropzone.appendChild(event.target);
        }
    }

    function handleDragOver(event) {
        event.preventDefault();
        const draggingElement = document.querySelector(".dragging");
        if (draggingElement) {
            const dropzone = event.currentTarget;
            dropzone.appendChild(draggingElement);
        }
    }

    function handleDrop(event) {
        event.preventDefault();
        const draggingElement = document.querySelector(".dragging");
        if (draggingElement) {
            event.currentTarget.appendChild(draggingElement);
        }
    }

    function getClosestDropzone(draggable) {
        let closestDropzone = null;
        let closestDistance = Infinity;

        dropzones.forEach(dropzone => {
            const rect = dropzone.getBoundingClientRect();
            const dropzoneCenterX = rect.left + rect.width / 2;
            const dropzoneCenterY = rect.top + rect.height / 2;

            const draggableRect = draggable.getBoundingClientRect();
            const draggableCenterX = draggableRect.left + draggableRect.width / 2;
            const draggableCenterY = draggableRect.top + draggableRect.height / 2;

            const distance = Math.sqrt(
                Math.pow(dropzoneCenterX - draggableCenterX, 2) +
                Math.pow(dropzoneCenterY - draggableCenterY, 2)
            );

            if (distance < closestDistance) {
                closestDistance = distance;
                closestDropzone = dropzone;
            }
        });

        return closestDropzone;
    }
});


