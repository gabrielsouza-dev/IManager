document.addEventListener('click', (e) => {
    const btn = e.target.closest('#approve-btn, #reject-btn');
    if (!btn) return;

    const prefix = btn.id === 'approve-btn' ? 'approve' : 'reject';

    modifyTextContent(prefix, btn);
});

document.addEventListener('submit', async (e) => {
    const form = e.target.closest('#approveForm, #rejectForm');
    if (!form) return;

    e.preventDefault();

    const prefix = form.id === 'approveForm' ? 'approve' : 'reject';
    var rejectReason = document.getElementById('rejectReason').value;
    if (prefix === 'reject' && !rejectReason) {
        alert('Porfavor, informe o motivo da reprovação.');
        return;
    }

    const btn = form.querySelector('button[type="submit"]');
    const id = document.getElementById(`${prefix}EntryId`).value;

    btn.disabled = true;

    try {
        const response = await fetch('/TimeEntries/ManageTimeEntryAction', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Id: id,
                IsApprove: prefix === 'approve',
                Comment: prefix === 'approve'
                    ? null
                    : rejectReason
            })
        });

        if (response.ok) {
            bootstrap.Modal.getInstance(
                document.getElementById(`${prefix}Modal`)
            )?.hide();

            document.getElementById('row-' + id)?.remove();
        } else {
            alert(await response.text() || 'Erro ao processar solicitação.');
        }
    } catch (err) {
        console.error(err);
        alert('Erro de comunicação com o servidor.');
    } finally {
        btn.disabled = false;
    }
});

const modifyTextContent = (prefix, btn) => {
    document.getElementById(`${prefix}EntryId`).value = btn.dataset.id;
    document.getElementById(`${prefix}Employee`).textContent = btn.dataset.employee;
    document.getElementById(`${prefix}Date`).textContent = btn.dataset.date;
    document.getElementById(`${prefix}OriginalChecks`).textContent = btn.dataset.originalchecks;
    document.getElementById(`${prefix}NewChecks`).textContent = btn.dataset.newchecks;
    document.getElementById(`${prefix}Total`).textContent = btn.dataset.total;
    document.getElementById(`${prefix}AdjustmentReason`).textContent = btn.dataset.adjustmentreason;

};