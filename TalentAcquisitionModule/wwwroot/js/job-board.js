// Wait for the document to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Get all job cards and attach click handlers
    const jobCards = document.querySelectorAll('.job-card');
    const jobTypeFilter = document.getElementById('job-type-filter');

    // Function to update job details
    function updateJobDetails(jobCard) {
        // Get data from the clicked job card
        const jobId = jobCard.dataset.jobId;

        // Make an AJAX call to get job details
        fetch(`/Job/GetJobDetails/${jobId}`)
            .then(response => response.json())
            .then(job => {
                // Update the main content area
                document.getElementById('job-title').textContent = job.title;
                document.getElementById('job-location').textContent = `📍 ${job.departmentName}`;
                document.getElementById('job-description').innerHTML = `
                    <h5>Description:</h5>
                    <p>${job.description}</p>
                    <div class="mt-3">
                        <span class="badge bg-info">${job.applicationCount} Applications</span>
                        <span class="badge bg-secondary ms-2">${job.jobType}</span>
                    </div>
                `;

                // Update the apply button
                const applyButton = document.getElementById('applyButton');
                applyButton.dataset.jobId = jobId;
            })
            .catch(error => console.error('Error fetching job details:', error));
    }

    // Function to filter jobs
    function filterJobs(jobType) {
        jobCards.forEach(card => {
            if (jobType === 'all' || card.dataset.jobType === jobType) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        });

        // If there are visible jobs after filtering, select the first one
        const visibleJobs = Array.from(jobCards).filter(card =>
            card.style.display !== 'none');

        if (visibleJobs.length > 0) {
            updateJobDetails(visibleJobs[0]);
        } else {
            // Show "No jobs found" message in the main content area
            document.querySelector('.job-detail-card').innerHTML = `
                <div class="alert alert-info">
                    No jobs found matching the selected criteria.
                </div>
            `;
        }
    }

    // Add click event listeners to job cards
    jobCards.forEach(card => {
        card.addEventListener('click', function (e) {
            e.preventDefault();

            // Remove active class from all cards and add to clicked one
            jobCards.forEach(c => c.classList.remove('active'));
            this.classList.add('active');

            updateJobDetails(this);
        });
    });

    // Add change event listener to job type filter
    jobTypeFilter.addEventListener('change', function () {
        filterJobs(this.value);
    });


window.addEventListener("scroll", function () {
    let navbar = document.getElementById("navbar");
    if (window.scrollY > 50) {
        navbar.classList.add("bg-dark");
    } else {
    }
}); 