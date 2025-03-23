$(document).ready(function () {
    // Initialize components
    const stageSelect = $("#stageSelect");
    const stageOccurrence = $("#stageOccurrence");
    const stagePreview = $("#stage-preview");
    const noStagesMessage = $("#no-stages-message");
    const stagesList = $(".stages-list");
    const addStageBtn = $("#add-stage-btn");
    const selectedStagesInputs = $("#selected-stages-inputs");
    const modal = $("#addExistingStageModal");
    const templateForm = $("form");

    // Use Bootstrap's modal methods for better control
    $("#openStageModalBtn").on('click', function () {
        updateOccurrenceInput();
        modal.modal('show');
    });

    // Function to get the next available occurrence number
    function getNextAvailableOccurrence() {
        if (selectedStages.length === 0) return 1;

        // Get all existing occurrence numbers
        const existingOccurrences = selectedStages.map(s => s.occurrence);

        // Find the first unused number starting from 1
        let nextOccurrence = 1;
        while (existingOccurrences.includes(nextOccurrence)) {
            nextOccurrence++;
        }

        return nextOccurrence;
    }

    // Function to update the occurrence input with the next available number
    function updateOccurrenceInput() {
        stageOccurrence.val(getNextAvailableOccurrence());
    }

    // Function to get formatted outcome type name
    function getOutcomeTypeName(outcomeType) {
        // If outcomeType is a number (0, 1, 2), convert to string
        if (typeof outcomeType === 'number') {
            return outcomeTypeNames[outcomeType] || "Unknown";
        }

        // If it's already a string, return it directly
        return outcomeType;
    }

    // Event handler for stage selection
    stageSelect.on('change', function () {
        const selectedStageId = $(this).val();

        if (selectedStageId) {
            // Find the selected stage from availableStages
            const selectedStage = availableStages.find(s => s.id == selectedStageId);

            if (selectedStage) {
                // Update preview
                $("#preview-name").text(selectedStage.stageName);

                // Format outcome type as a readable string
                const outcomeTypeName = getOutcomeTypeName(selectedStage.outcomeType);
                $("#preview-outcome-type").text(outcomeTypeName);

                // Format parameters as a comma-separated list of names
                const parameterNames = selectedStage.parameters.map(p => p.name || p.parameterName).join(", ");
                $("#preview-parameters").text(parameterNames || "None");

                // Show preview and enable add button
                stagePreview.show();
                addStageBtn.prop('disabled', false);
            } else {
                stagePreview.hide();
                addStageBtn.prop('disabled', true);
            }
        } else {
            stagePreview.hide();
            addStageBtn.prop('disabled', true);
        }
    });

    // Event handler for adding a stage
    addStageBtn.on('click', function () {
        const selectedStageId = stageSelect.val();
        const occurrence = parseInt(stageOccurrence.val());

        if (!selectedStageId || isNaN(occurrence) || occurrence < 1) {
            alert("Please select a stage and provide a valid occurrence number.");
            return;
        }

        // Find the selected stage details
        const selectedStage = availableStages.find(s => s.id == selectedStageId);

        if (!selectedStage) {
            alert("Selected stage not found.");
            return;
        }

        // Check if the occurrence is already used
        const existingOccurrence = selectedStages.find(s => s.occurrence == occurrence);
        if (existingOccurrence) {
            alert("This occurrence number is already used. Please select a different number.");
            return;
        }

        // Get parameter names for display
        const parameterNames = selectedStage.parameters.map(p => p.name || p.parameterName).join(", ");

        // Get formatted outcome type
        const outcomeTypeName = getOutcomeTypeName(selectedStage.outcomeType);

        // Create a deep copy of the stage
        const stageCopy = JSON.parse(JSON.stringify(selectedStage));

        // Set the occurrence property
        stageCopy.occurance = occurrence;

        // Add to selected stages
        const stageData = {
            stageId: selectedStageId,
            stageName: selectedStage.stageName,
            occurrence: occurrence,
            outcomeType: selectedStage.outcomeType,
            outcomeTypeName: outcomeTypeName,
            parameters: parameterNames,
            stageObject: stageCopy  // Store the modified stage object
        };

        selectedStages.push(stageData);

        // Update UI
        updateStagesList();

        // Reset modal
        stageSelect.val("");
        stagePreview.hide();
        addStageBtn.prop('disabled', true);

        // Close modal
        modal.modal('hide');
    });

    // Function to update the stages list UI
    function updateStagesList() {
        // Clear existing stages
        stagesList.empty();
        selectedStagesInputs.empty();

        // Sort stages by occurrence
        const sortedStages = [...selectedStages].sort((a, b) => a.occurrence - b.occurrence);

        // Add stages to the list
        sortedStages.forEach((stage, index) => {
            const listItem = $(`
                <li class="list-group-item">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <div class="input-group" style="width: auto;">
                                <span class="input-group-text">Order</span>
                                <input type="number" class="form-control occurrence-input" 
                                    style="width: 80px;" min="1" value="${stage.occurrence}" 
                                    data-stage-id="${stage.stageId}" data-original-occurrence="${stage.occurrence}" 
                                    data-index="${index}">
                            </div>
                            <strong class="ms-2">${stage.stageName}</strong>
                        </div>
                        <div>
                            <button type="button" class="btn btn-outline-danger btn-sm remove-stage" 
                                data-stage-id="${stage.stageId}" data-occurrence="${stage.occurrence}" 
                                data-index="${index}">
                                <i class="bi bi-trash"></i> Remove
                            </button>
                        </div>
                    </div>
                    <div class="mt-2 small text-muted">
                        <span class="me-3"><strong>Outcome Type:</strong> ${stage.outcomeTypeName}</span>
                        <span><strong>Parameters:</strong> ${stage.parameters || "None"}</span>
                    </div>
                </li>
            `);

            stagesList.append(listItem);
        });

        // Show/hide no stages message
        if (selectedStages.length > 0) {
            noStagesMessage.hide();
            stagesList.show();
        } else {
            noStagesMessage.show();
            stagesList.hide();
        }
    }

    // Event delegation for remove buttons
    $(document).on('click', '.remove-stage', function () {
        const stageId = $(this).data('stage-id');
        const occurrence = $(this).data('occurrence');

        // Remove the specific stage with matching stage ID and occurrence
        selectedStages = selectedStages.filter(s => !(s.stageId == stageId && s.occurrence == occurrence));
        updateStagesList();
    });

    // Event delegation for occurrence input changes
    $(document).on('change', '.occurrence-input', function () {
        const stageId = $(this).data('stage-id');
        const originalOccurrence = $(this).data('original-occurrence');
        const newOccurrence = parseInt($(this).val());

        // Validate input
        if (isNaN(newOccurrence) || newOccurrence < 1) {
            alert("Please enter a valid occurrence number (must be 1 or greater).");
            $(this).val(originalOccurrence);
            return;
        }

        // Check if the new occurrence number is already in use
        const existingStage = selectedStages.find(s =>
            s.occurrence == newOccurrence &&
            !(s.stageId == stageId && s.occurrence == originalOccurrence)
        );

        if (existingStage) {
            alert("This occurrence number is already in use. Please choose a different number.");
            $(this).val(originalOccurrence);
            return;
        }

        // Update the occurrence in the selectedStages array
        const stageToUpdate = selectedStages.find(s =>
            s.stageId == stageId && s.occurrence == originalOccurrence
        );

        if (stageToUpdate) {
            stageToUpdate.occurrence = newOccurrence;
            stageToUpdate.stageObject.occurance = newOccurrence;

            // Update data attributes for future reference
            $(this).data('original-occurrence', newOccurrence);

            // Update the remove button's data attribute
            $(this).closest('li').find(`.remove-stage[data-stage-id="${stageId}"]`)
                .data('occurrence', newOccurrence)
                .attr('data-occurrence', newOccurrence);

            // Re-render the list to maintain sort order
            updateStagesList();
        }
    });

    // Handle form submission
    templateForm.on('submit', function (e) {
        e.preventDefault();

        // Check if we have at least one stage
        if (selectedStages.length === 0) {
            alert("Please add at least one hiring stage to the template.");
            return false;
        }

        // First, clear any existing hidden inputs for stages
        selectedStagesInputs.empty();

        // Create hidden inputs for the selected stages
        // Update to match the new TemplateStageVM structure
        selectedStages.forEach((stage, index) => {
            selectedStagesInputs.append(`
                <input type="hidden" name="TemplateStages[${index}].StageId" value="${stage.stageId}">
                <input type="hidden" name="TemplateStages[${index}].Occurance" value="${stage.occurrence}">
            `);
        });

        // Continue with form submission
        this.submit();
    });
});