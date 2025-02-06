@section Scripts {
    <script>
        function applyForJob(jobId) {
            // Send an AJAX request to the server
            fetch(`/Jobs/ApplyForJob?jobId=${jobId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Anti-forgery token
                }
            })
                .then(response => {
                    if (response.ok) {
                        // Show a success popup
                        alert('You applied successfully!');
                        // Optionally, update the UI (e.g., disable the "Apply Now" button)
                        document.querySelector(`.apply-btn`).disabled = true;
                    } else {
                        alert('Failed to apply. Please try again.');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('An error occurred. Please try again.');
                });
        }
    </script>
}