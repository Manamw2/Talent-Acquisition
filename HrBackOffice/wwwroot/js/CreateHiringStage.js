$(document).ready(function () {
    // Keep all the outcome type logic for future use but don't call updateOutcomeOptions()
    function updateOutcomeOptions() {
        const outcomeType = '0'; // Hard-coded to PassFail for now
        const container = $('.outcome-options-container');
        container.empty();

        // Show/hide score range inputs
        if (outcomeType === '1') { // Set (assuming Set is enum value 1)
            $('.score-range').show();
        } else {
            $('.score-range').hide();
        }

        // Add outcome options based on type
        if (outcomeType === '0') { // PassFail
            container.append(`
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label class="form-label">Accepted Outcome</label>
                                    <input type="text" name="OutcomeNames" class="form-control" value="Accepted" required>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label class="form-label">Rejected Outcome</label>
                                    <input type="text" name="OutcomeNames" class="form-control" value="Rejected" required>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label class="form-label">Accepted Notification</label>
                                    <textarea name="OutcomeNotifications" class="form-control" rows="3" placeholder="Notification message for accepted applicants" required></textarea>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label class="form-label">Rejected Notification</label>
                                    <textarea name="OutcomeNotifications" class="form-control" rows="3" placeholder="Notification message for rejected applicants" required></textarea>
                                </div>
                            </div>
                        </div>
                    `);
        } else if (outcomeType === '1') { // Set
            container.append(`
                        <div class="col-md-12">
                            <div class="form-group">
                                <label class="form-label">Score Outcomes</label>
                                <div class="score-outcomes-list">
                                    <div class="score-outcome-row mb-2">
                                        <div class="input-group">
                                            <input type="text" name="OutcomeNames" class="form-control" placeholder="Outcome name" required>
                                            <input type="number" name="OutcomeValues" class="form-control" placeholder="Threshold value" required>
                                            <textarea name="OutcomeNotifications" class="form-control" placeholder="Notification message" required></textarea>
                                            <button class="btn btn-outline-danger remove-row" type="button">✕</button>
                                        </div>
                                    </div>
                                </div>
                                <button class="btn btn-link p-0 add-score-outcome" type="button">+ Add Score Outcome</button>
                            </div>
                        </div>
                    `);
        } else if (outcomeType === '2') { // Evaluation
            container.append(`
                        <div class="col-md-12">
                            <div class="form-group">
                                <label class="form-label">Evaluation Outcomes</label>
                                <div class="evaluation-outcomes-list">
                                    <div class="evaluation-outcome-row mb-2">
                                        <div class="input-group">
                                            <input type="text" name="OutcomeNames" class="form-control" placeholder="Outcome name" required>
                                            <textarea name="OutcomeNotifications" class="form-control" placeholder="Notification message" required></textarea>
                                            <button class="btn btn-outline-danger remove-row" type="button">✕</button>
                                        </div>
                                    </div>
                                </div>
                                <button class="btn btn-link p-0 add-evaluation-outcome" type="button">+ Add Evaluation Outcome</button>
                            </div>
                        </div>
                    `);
        }
    }

    // Don't call updateOutcomeOptions() as we're handling it manually in the HTML

    // Add department row
    $(document).on('click', '.add-department', function () {
        const departmentsContainer = $(this).closest('.form-group').find('.departments-list');

        let options = '<option value="">Select Department</option>';
        departmentsList.forEach(department => {
            options += `<option value="${department.value}">${department.text}</option>`;
        });

        const newRow = `
        <div class="department-row mb-2">
            <div class="input-group">
                <select name="DepartmentIds" class="form-select department-select" required>
                    ${options}
                </select>
                <input type="number" name="EmployeeCounts" class="form-control employee-count" style="max-width: 100px;" min="1" value="1" required>
                <button class="btn btn-outline-danger remove-row" type="button">✕</button>
            </div>
        </div>
    `;
        departmentsContainer.append(newRow);
    });
    // Add paramter row
    $(document).on('click', '.add-parameter', function () {
        const parametersContainer = $(this).closest('.form-group').find('.parameters-list');

        let options = '<option value="">Select Parameter</option>';
        parametersList.forEach(parameter => {
            options += `<option value="${parameter.value}">${parameter.text}</option>`;
        });

        const newRow = `
        <div class="parameter-row mb-2">
            <div class="input-group">
                <select name="ParameterIds" class="form-select parameter-select" required>
                    ${options}
                </select>
                <button class="btn btn-outline-danger remove-row" type="button">✕</button>
            </div>
        </div>
    `;
        parametersContainer.append(newRow);
    });

    // Add evaluation outcome row
    $(document).on('click', '.add-evaluation-outcome', function () {
        const evaluationOutcomesList = $(this).closest('.form-group').find('.evaluation-outcomes-list');
        const newRow = `
                    <div class="evaluation-outcome-row mb-2">
                        <div class="input-group">
                            <input type="text" name="OutcomeNames" class="form-control" placeholder="Outcome name" required>
                            <textarea name="OutcomeNotifications" class="form-control" placeholder="Notification message" required></textarea>
                            <button class="btn btn-outline-danger remove-row" type="button">✕</button>
                        </div>
                    </div>
                `;
        evaluationOutcomesList.append(newRow);
    });

    // Remove row (any type)
    $(document).on('click', '.remove-row', function () {
        $(this).closest('.department-row, .parameter-row, .score-outcome-row, .evaluation-outcome-row').remove();
    });
});