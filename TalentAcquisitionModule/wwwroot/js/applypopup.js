//@section Scripts {
//    <script>
//        function applyForJob(jobId) {
//            // Send an AJAX request to the server
//            fetch(`/Jobs/ApplyForJob?jobId=${jobId}`, {
//                method: 'POST',
//                headers: {
//                    'Content-Type': 'application/json',
//                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Anti-forgery token
//                }
//            })
//                .then(response => {
//                    if (response.ok) {
//                        // Show a success popup
//                        alert('You applied successfully!');
//                        // Optionally, update the UI (e.g., disable the "Apply Now" button)
//                        document.querySelector(`.apply-btn`).disabled = true;
//                    } else {

//                        const loginLink = '<a class="btn btn-outline-light text-uppercase" asp-area="Identity" asp-page="/Account/Login">Login</a>';
//                        const modalContent = `
//            <div class="modal-content">
//                <h3>Oops...</h3>
//                <p>Applicant not found. Please log in.</p>
//                ${loginLink}
//                <button onclick="closeModal()" class="btn">OK</button>
//            </div>
//        `;

//                        // Create and show the modal
//                        const modal = document.createElement('div');
//                        modal.className = 'custom-modal';
//                        modal.innerHTML = modalContent;
//                        document.body.appendChild(modal);
//                    }
//                })

//// Add this function to handle modal closing
//function closeModal() {
//            document.querySelector('.custom-modal').remove();
//}
//                })
//                .catch(error => {
//                    console.error('Error:', error);
//                    alert('An error occurred. Please try again.');
//                });
//        }
//    </script>
//}

@section Scripts {
    <script>
{/*        function closeModal() {*/}
{/*            document.querySelector('.custom-modal').remove();*/}
{/*        }*/}

{/*        function applyForJob(jobId) {*/}
{/*            // Send an AJAX request to the server*/}
{/*            fetch(`/Jobs/ApplyForJob?jobId=${jobId}`, {*/}
{/*                method: 'POST',*/}
{/*                headers: {*/}
{/*                    'Content-Type': 'application/json',*/}
{/*                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()*/}
{/*                }*/}
{/*            })*/}
{/*                .then(response => {*/}
{/*                    if (response.ok) {*/}
{/*                        // Show a success popup*/}
{/*                        alert('You applied successfully!');*/}
{/*                        // Optionally, update the UI (e.g., disable the "Apply Now" button)*/}
{/*                        document.querySelector(`.apply-btn`).disabled = true;*/}
{/*                    } else {*/}
{/*                        const modalContent = `*/}
{/*                        <div class="modal-content">*/}
{/*                            <h3>Oops...</h3>*/}
{/*                            <p>Applicant not found. Please log in.</p>*/}
{/*                            <a href="/Identity/Account/Login" class="btn btn-outline-light text-uppercase">Login</a>*/}
{/*                            <button onclick="closeModal()" class="btn">OK</button>*/}
{/*                        </div>*/}
{/*                    `;*/}

{/*                        // Create and show the modal*/}
{/*                        const modal = document.createElement('div');*/}
{/*                        modal.className = 'custom-modal';*/}
{/*                        modal.innerHTML = modalContent;*/}
{/*                        document.body.appendChild(modal);*/}
{/*                    }*/}
{/*                })*/}
{/*                .catch(error => {*/}
{/*                    console.error('Error:', error);*/}
{/*                    alert('An error occurred. Please try again.');*/}
{/*                });*/}
{/*        }*/}
{/*        // Create a mutation observer to watch for the popup*/}
{/*const observer = new MutationObserver((mutations) => {*/}
{/*            mutations.forEach((mutation) => {*/}
{/*                if (mutation.addedNodes.length) {*/}
{/*                    mutation.addedNodes.forEach((node) => {*/}
{/*                        // Check if this is the error popup with the "Oops..." message*/}
{/*                        if (node.textContent && node.textContent.includes('Applicant not found')) {*/}
{/*                            // Find the OK button*/}
{/*                            const okButton = node.querySelector('button');*/}
{/*                            if (okButton) {*/}
{/*                                // Create the login link*/}
{/*                                const loginLink = document.createElement('a');*/}
{/*                                loginLink.href = '/Identity/Account/Login';*/}
{/*                                loginLink.className = okButton.className; // Keep the same styling*/}
{/*                                loginLink.textContent = 'Login';*/}

{/*                                // Replace the OK button with the login link*/}
{/*                                okButton.parentNode.replaceChild(loginLink, okButton);*/}
{/*                            }*/}
{/*                        }*/}
{/*                    });*/}
{/*                }*/}
{/*            });*/}
{/*});*/}

{/*        // Start observing the document body for changes*/}
{/*        observer.observe(document.body, {*/}
{/*            childList: true,*/}
{/*        subtree: true*/}
{/*});*/}

    </script>
}