$(document).ready(function () {
    // Variable to track the number of departments
    let departmentCount = 1;
    let parameterCount = 1;

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
                <select name="Departments[${departmentCount}].Id" class="form-select department-select" required>
                    ${options}
                </select>
                <input type="hidden" name="Departments[${departmentCount}].Name" class="department-name">
                <input type="number" name="Departments[${departmentCount}].NeededEmployees" class="form-control employee-count" style="max-width: 100px;" min="1" value="1" required>
                <button class="btn btn-outline-danger remove-row" type="button">✕</button>
            </div>
        </div>
        `;
        departmentsContainer.append(newRow);
        departmentCount++;
    });

    // Add parameter row
    $(document).on('click', '.add-parameter', function () {
        const parametersContainer = $(this).closest('.form-group').find('.parameters-list');

        let options = '<option value="">Select Parameter</option>';
        parametersList.forEach(parameter => {
            options += `<option value="${parameter.value}">${parameter.text}</option>`;
        });

        const newRow = `
        <div class="parameter-row mb-2">
            <div class="input-group">
                <select name="ParameterIds[${parameterCount}]" class="form-select parameter-select" required>
                    ${options}
                </select>
                <button class="btn btn-outline-danger remove-row" type="button">✕</button>
            </div>
        </div>
        `;
        parametersContainer.append(newRow);
        parameterCount++;
    });

    // Remove row (any type)
    $(document).on('click', '.remove-row', function () {
        $(this).closest('.department-row, .parameter-row').remove();
        reindexInputs();
    });

    // Update department name hidden field when department is selected
    $(document).on('change', '.department-select', function () {
        const selectedOption = $(this).find('option:selected');
        const departmentName = selectedOption.text();
        $(this).siblings('.department-name').val(departmentName);
    });

    // Reindex input names when rows are removed
    function reindexInputs() {
        // Reindex departments
        $('.department-row').each(function (index) {
            $(this).find('.department-select').attr('name', `Departments[${index}].Id`);
            $(this).find('.department-name').attr('name', `Departments[${index}].Name`);
            $(this).find('.employee-count').attr('name', `Departments[${index}].NeededEmployees`);
        });

        // Reindex parameters
        $('.parameter-row').each(function (index) {
            $(this).find('.parameter-select').attr('name', `ParameterIds[${index}]`);
        });

        // Reset counters
        departmentCount = $('.department-row').length;
        parameterCount = $('.parameter-row').length;
    }

    // Initialize department names for initial rows
    $('.department-select').each(function () {
        if ($(this).val()) {
            const selectedOption = $(this).find('option:selected');
            const departmentName = selectedOption.text();
            $(this).siblings('.department-name').val(departmentName);
        }
    });
});