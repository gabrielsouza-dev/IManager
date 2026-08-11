document.addEventListener('click', (e) => {
    const btn = e.target.closest('.time-entry-action');
    if (!btn) return;

    const prefix = btn.dataset.action;

    modifyTextContent(prefix, btn);
});

document.addEventListener('submit', async (e) => {
    const form = e.target.closest('#approveForm, #rejectForm');
    if (!form) return;

    e.preventDefault();

    const prefix = form.id === 'approveForm'
        ? 'approve'
        : 'reject';

    const rejectReason = document
        .getElementById('rejectReason')
        .value
        .trim();

    if (prefix === 'reject' && !rejectReason) {
        alert('Por favor, informe o motivo da reprovação.');
        return;
    }

    const btn = form.querySelector('button[type="submit"]');

    const id = document
        .getElementById(`${prefix}EntryId`)
        .value;

    btn.disabled = true;

    try {
        const response = await manageTimeEntryAction(
            id,
            prefix,
            rejectReason
        );

        if (!response.ok) {
            await handleErrorResponse(response);
            return;
        }

        const modal = bootstrap.Modal.getInstance(
            document.getElementById(`${prefix}Modal`)
        );

        if (modal) {
            modal.hide();
        }

        window.location.reload();

    } catch (err) {
        console.error(err);
        alert('Erro de comunicação com o servidor.');
    } finally {
        btn.disabled = false;
    }
});

const manageTimeEntryAction = async (
    id,
    prefix,
    rejectReason
) => {
    return await fetch('/TimeEntries/ManageTimeEntryAction', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Id: id,
            IsApprove: prefix === 'approve',
            Comment: prefix === 'approve'
                ? null
                : rejectReason
        })
    });
};

const handleErrorResponse = async (response) => {
    try {
        const contentType = response.headers.get('content-type');

        if (contentType?.includes('application/json')) {
            const errorData = await response.json();

            const message =
                errorData.errors?.join(', ') ||
                errorData.message ||
                'Erro ao processar solicitação.';

            alert(message);

            return;
        }

        const error = await response.text();

        alert(
            error ||
            'Erro ao processar solicitação.'
        );

    } catch (err) {
        console.error(err);
        alert('Erro ao processar solicitação.');
    }
};

const modifyTextContent = (prefix, btn) => {
    document.getElementById(`${prefix}EntryId`).value =
        btn.dataset.id;

    document.getElementById(`${prefix}Employee`).textContent =
        btn.dataset.employee;

    document.getElementById(`${prefix}Date`).textContent =
        btn.dataset.date;

    document.getElementById(`${prefix}OriginalChecks`).textContent =
        btn.dataset.originalchecks;

    document.getElementById(`${prefix}NewChecks`).textContent =
        btn.dataset.newchecks;

    document.getElementById(`${prefix}Total`).textContent =
        btn.dataset.total;

    document.getElementById(`${prefix}AdjustmentReason`).textContent =
        btn.dataset.adjustmentreason;
};