document.addEventListener('DOMContentLoaded', function () {
    // Function to toggle collapsible content
    function toggleCollapsibleContent(button) {
        const content = button.closest('.card, .row').querySelector('.collapsible-content');
        content.classList.toggle('show');
        const chevron = button.querySelector('.chevron-icon');
        chevron.textContent = content.classList.contains('show') ? '▼' : '▶';
    }

        // Function to remove an item (experience, skill, or project)
        function removeItem(button) {
            const item = button.closest('.card, .row');
item.remove();
        }

        // Add event listeners for existing toggle and close buttons
        document.querySelectorAll('.toggle-experience').forEach(button => {
    button.addEventListener('click', function () {
        toggleCollapsibleContent(this);
    });
        });

        document.querySelectorAll('.toggle-skill').forEach(button => {
    button.addEventListener('click', function () {
        toggleCollapsibleContent(this);
    });
        });

        document.querySelectorAll('.toggle-project').forEach(button => {
    button.addEventListener('click', function () {
        toggleCollapsibleContent(this);
    });
        });

        document.querySelectorAll('.remove-experience, .remove-skill, .remove-project').forEach(button => {
    button.addEventListener('click', function () {
        removeItem(this);
    });
        });

// Experience template
const experienceTemplate = `
<div class="card mb-3">
    <div class="card-body">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <button type="button" class="btn btn-outline-secondary toggle-experience">
                <span class="chevron-icon">▼</span>
                Experience {display_index}
            </button>
            <button type="button" class="btn-close remove-experience" aria-label="Close"></button>
        </div>
        <div class="collapsible-content show">
            <input type="hidden" name="Experiences[{index}].Id" value="0" />
            <div class="mb-3">
                <input type="text" class="form-control" name="Experiences[{index}].Company" placeholder="Company" />
            </div>
            <div class="mb-3">
                <input type="text" class="form-control" name="Experiences[{index}].Position" placeholder="Position" />
            </div>
            <div class="row mb-3">
                <div class="col-md-6">
                    <input type="date" class="form-control" name="Experiences[{index}].StartDate" />
                </div>
                <div class="col-md-6">
                    <input type="date" class="form-control" name="Experiences[{index}].EndDate" />
                </div>
            </div>
            <div class="mb-3">
                <textarea class="form-control" name="Experiences[{index}].Description" rows="3" placeholder="Description"></textarea>
            </div>
        </div>
    </div>
</div>
`;

// Add new experience
document.getElementById('addExperience').addEventListener('click', function () {
            const container = document.getElementById('experiencesContainer');
const index = container.children.length;
const display_index = index + 1;
const newExperience = experienceTemplate.replace(/{index}/g, index).replace(/{display_index}/g, display_index);;
container.insertAdjacentHTML('beforeend', newExperience);

// Add event listeners for the new experience
const newExperienceCard = container.lastElementChild;
newExperienceCard.querySelector('.toggle-experience').addEventListener('click', function () {
    toggleCollapsibleContent(this);
            });
newExperienceCard.querySelector('.remove-experience').addEventListener('click', function () {
    removeItem(this);
            });
        });

// Skill template
const skillTemplate = `
<div class="row mb-3">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <button type="button" class="btn btn-outline-secondary toggle-skill">
            <span class="chevron-icon">▼</span>
            Skill {display_index}
        </button>
        <button type="button" class="btn-close remove-skill" aria-label="Close"></button>
    </div>
    <div class="collapsible-content show">
        <input type="hidden" name="Skills[{index}].Id" value="0" />
        <div class="row">
            <div class="col-md-8">
                <input type="text" class="form-control" name="Skills[{index}].Name" placeholder="Skill name" />
            </div>
            <div class="col-md-4">
                <select class="form-control" name="Skills[{index}].Level">
                    <option value="Beginner">Beginner</option>
                    <option value="Intermediate">Intermediate</option>
                    <option value="Advanced">Advanced</option>
                    <option value="Expert">Expert</option>
                </select>
            </div>
        </div>
    </div>
</div>
`;

// Add new skill
document.getElementById('addSkill').addEventListener('click', function () {
            const container = document.getElementById('skillsContainer');
const index = container.children.length;
const display_index = index + 1;
const newSkill = skillTemplate.replace(/{index}/g, index).replace(/{display_index}/g, display_index);
container.insertAdjacentHTML('beforeend', newSkill);

// Add event listeners for the new skill
const newSkillRow = container.lastElementChild;
newSkillRow.querySelector('.toggle-skill').addEventListener('click', function () {
    toggleCollapsibleContent(this);
            });
newSkillRow.querySelector('.remove-skill').addEventListener('click', function () {
    removeItem(this);
            });
        });

// Project template
const projectTemplate = `
<div class="card mb-3">
    <div class="card-body">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <button type="button" class="btn btn-outline-secondary toggle-project">
                <span class="chevron-icon">▼</span>
                Project {display_index}
            </button>
            <button type="button" class="btn-close remove-project" aria-label="Close"></button>
        </div>
        <div class="collapsible-content show">
            <input type="hidden" name="Projects[{index}].Id" value="0" />
            <div class="mb-3">
                <input type="text" class="form-control" name="Projects[{index}].Name" placeholder="Project name" />
            </div>
            <div class="mb-3">
                <textarea class="form-control" name="Projects[{index}].Description" rows="3" placeholder="Project description"></textarea>
            </div>
        </div>
    </div>
</div>
`;

// Add new project
document.getElementById('addProject').addEventListener('click', function () {
            const container = document.getElementById('projectsContainer');
const index = container.children.length;
const display_index = index + 1;
const newProject = projectTemplate.replace(/{index}/g, index).replace(/{display_index}/g, display_index);;
container.insertAdjacentHTML('beforeend', newProject);

// Add event listeners for the new project
const newProjectCard = container.lastElementChild;
newProjectCard.querySelector('.toggle-project').addEventListener('click', function () {
    toggleCollapsibleContent(this);
            });
newProjectCard.querySelector('.remove-project').addEventListener('click', function () {
    removeItem(this);
            });
        });
    });
