
$(document).ready(function() {
    console.log('Start');
    $('#templateSelect').change(function() {
        var templateId = $(this).val();
        var detailsContainer = $('#templateDetailsContainer');

        // Debug logging
        console.log('Selected Template ID:', templateId);

        if (templateId) {
            $.ajax({
                url: `/api/HiringTemplateApi/${templateId}`,
                type: 'GET',
                dataType: 'json', // Explicitly specify JSON
                contentType: 'application/json', // Ensure correct content type

                // Add more detailed error handling
                beforeSend: function (xhr) {
                    console.log('Sending AJAX request to:', `/api/HiringTemplateApi/${templateId}`);
                    // Optional: Add authentication headers if needed
                    // xhr.setRequestHeader("Authorization", "Bearer " + yourToken);
                },

                success: function (response) {
                    console.log('AJAX Success - Full Response:', response);

                    // More robust response handling
                    $('#templateName').text(response.name || 'No Name');
                    $('#templateDescription').text(response.description || 'No Description');
                    detailsContainer.show();
                },

                error: function (xhr, status, error) {
                    console.error('AJAX Error Details:');
                    console.error('Status:', status);
                    console.error('Error:', error);
                    console.error('Response Text:', xhr.responseText);

                    detailsContainer.hide();
                    alert('Failed to load template details. Check console for more information.');
                }
            });
        } else {
            detailsContainer.hide();
        }
    });
});
