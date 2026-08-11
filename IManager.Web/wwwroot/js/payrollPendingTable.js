let processModel;
let processBtn;

const getProcessModal = () => {
    if (!processModel) {
        const modalElement = document.getElementById("processModal");

        if (!modalElement) {
            console.error("Modal não encontrado");
            return null;
        }

        processModel = bootstrap.Modal.getOrCreateInstance(modalElement);
    }

    return processModel;
};

document.addEventListener('click', async (e) => {
    const btn = e.target.closest('#process-btn');
    if (!btn) return;

    btn.disabled = true;

    const employeeId = btn.dataset.employeeId;
    const competenceDate = btn.dataset.competenceDate;

    try {
        const response = await processPayroll(
            employeeId,
            competenceDate,
            false
        );

        if (!response.ok) {
            const errorData = await response.json();

            processBtn = btn;

            showProcessModal(btn, errorData.errors);
            return;
        }

        sessionStorage.setItem(
            'payrollCompetenceDate',
            competenceDate
        );

        window.location.reload();

    } catch (e) {
        console.error(e);
        alert('Erro de comunicação com o servidor.');
    } finally {
        btn.disabled = false;
    }
});

document.addEventListener('submit', async (e) => {
    const form = e.target.closest('#processForm');
    if (!form) return;

    e.preventDefault();

    const btn = form.querySelector('button[type="submit"]');

    const employeeId =
        document.getElementById('process-employeeId').textContent;

    const conpetenceDate =
        document.getElementById('process-competenceDate').textContent;

    btn.disabled = true;

    try {
        const response = await processPayroll(
            employeeId,
            conpetenceDate,
            true
        );

        if (!response.ok) {
            const errorData = await response.json();

            document.getElementById('process-errors').innerHTML =
                `<ul>${errorData.errors
                    ?.map(e => `<li>${e}</li>`)
                    .join("") ?? ""
                }</ul>`;

            return;
        }

        processBtn = null;

        const modal = getProcessModal();

        if (modal) {
            modal.hide();
        }

        sessionStorage.setItem(
            'payrollCompetenceDate',
            conpetenceDate
        );

        window.location.reload();

    } catch (err) {
        console.error(err);
        alert('Erro de comunicação com o servidor.');
    } finally {
        btn.disabled = false;
    }
});

const processPayroll = async (employeeId, competenceDate, isForced) => {
    const response = await fetch('/Payrolls/Process', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            employeeIds: [employeeId],
            competenceDate: competenceDate,
            isForced: isForced
        })
    });

    return response;
};

const showProcessModal = (btn, errors) => {
    document.getElementById('process-employeeId').textContent =
        btn.dataset.employeeId;

    document.getElementById('process-employeeName').textContent =
        btn.dataset.employeeName;

    document.getElementById('process-competenceDate').textContent =
        btn.dataset.competenceDate;

    document.getElementById('process-hoursWorked').textContent =
        btn.dataset.hoursWorked;

    document.getElementById('process-daysWorked').textContent =
        btn.dataset.daysWorked;

    document.getElementById('process-isConsistent').textContent =
        btn.dataset.isConsistent;

    document.getElementById('process-errors').innerHTML = '';

    if (errors && errors.length > 0) {
        document.getElementById('process-errors').innerHTML =
            `<ul>${errors.map(e => `<li>${e}</li>`).join("")}</ul>`;
    }

    const modal = getProcessModal();

    if (modal) {
        modal.show();
    }
};