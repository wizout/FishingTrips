// Single-file modular vanilla JS app for FishingTrips API
const API = '/api';

// ---------- API helpers ----------
async function http(method, path, body) {
    const opts = { method, headers: {} };
    if (body !== undefined) {
        opts.headers['Content-Type'] = 'application/json';
        opts.body = JSON.stringify(body);
    }
    const res = await fetch(API + path, opts);
    if (res.status === 204) return null;
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
        const msg = data?.title || data?.errors ? JSON.stringify(data.errors || data.title) : `HTTP ${res.status}`;
        throw new Error(msg);
    }
    return data;
}
const api = {
    anglers: {
        list: () => http('GET', '/anglers'),
        create: d => http('POST', '/anglers', d),
        update: (id, d) => http('PUT', `/anglers/${id}`, d),
        delete: id => http('DELETE', `/anglers/${id}`),
        trips: id => http('GET', `/anglers/${id}/trips`),
    },
    guides: {
        list: () => http('GET', '/guides'),
        create: d => http('POST', '/guides', d),
        update: (id, d) => http('PUT', `/guides/${id}`, d),
        delete: id => http('DELETE', `/guides/${id}`),
    },
    waterbodies: {
        list: () => http('GET', '/waterbodies'),
        create: d => http('POST', '/waterbodies', d),
        update: (id, d) => http('PUT', `/waterbodies/${id}`, d),
        delete: id => http('DELETE', `/waterbodies/${id}`),
    },
    trips: {
        list: q => http('GET', '/fishingtrips' + (q ? '?' + new URLSearchParams(q) : '')),
        get: id => http('GET', `/fishingtrips/${id}`),
        create: d => http('POST', '/fishingtrips', d),
        update: (id, d) => http('PUT', `/fishingtrips/${id}`, d),
        delete: id => http('DELETE', `/fishingtrips/${id}`),
        participants: id => http('GET', `/fishingtrips/${id}/participants`),
        book: (id, anglerId) => http('POST', `/fishingtrips/${id}/book`, { anglerId }),
        cancel: (id, anglerId) => http('DELETE', `/fishingtrips/${id}/book/${anglerId}`),
        complete: (id, data) => http('POST', `/fishingtrips/${id}/complete`, data),
    },
};

// ---------- UI helpers ----------
const $ = sel => document.querySelector(sel);
const $$ = sel => document.querySelectorAll(sel);

function toast(msg, type = '') {
    const el = document.createElement('div');
    el.className = 'toast ' + type;
    el.textContent = msg;
    $('#toasts').appendChild(el);
    setTimeout(() => { el.style.opacity = '0'; setTimeout(() => el.remove(), 250); }, 3500);
}

function modal(title, html) {
    $('#modal-title').textContent = title;
    $('#modal-body').innerHTML = html;
    $('#modal-backdrop').classList.add('open');
}
function closeModal() { $('#modal-backdrop').classList.remove('open'); }
$('#modal-close').onclick = closeModal;
$('#modal-backdrop').onclick = e => { if (e.target.id === 'modal-backdrop') closeModal(); };

function fmtDate(s) {
    const d = new Date(s);
    return d.toLocaleString('uk-UA', { dateStyle: 'medium', timeStyle: 'short' });
}
function fmtDateInput(s) {
    const d = new Date(s);
    const pad = n => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// ---------- State ----------
const state = { anglers: [], guides: [], waterbodies: [], trips: [], currentTab: 'trips' };

async function loadAll() {
    const [a, g, w] = await Promise.all([api.anglers.list(), api.guides.list(), api.waterbodies.list()]);
    state.anglers = a; state.guides = g; state.waterbodies = w;
    populateFilters();
}

function populateFilters() {
    const wSel = $('#filter-waterbody');
    wSel.innerHTML = '<option value="">Усі водойми</option>' + state.waterbodies.map(w => `<option value="${w.id}">${w.name}</option>`).join('');
    const gSel = $('#filter-guide');
    gSel.innerHTML = '<option value="">Усі гіди</option>' + state.guides.map(g => `<option value="${g.id}">${g.fullName}</option>`).join('');
}

// ---------- Tabs ----------
$$('.tab').forEach(t => t.onclick = () => {
    $$('.tab').forEach(x => x.classList.remove('active'));
    $$('.panel').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    const tab = t.dataset.tab;
    $('#' + tab).classList.add('active');
    state.currentTab = tab;
    renderCurrent();
});

async function renderCurrent() {
    if (state.currentTab === 'trips') await renderTrips();
    else if (state.currentTab === 'waterbodies') renderWaterbodies();
    else if (state.currentTab === 'guides') renderGuides();
    else if (state.currentTab === 'anglers') renderAnglers();
}

// ---------- TRIPS ----------
async function renderTrips() {
    const filters = {};
    const wb = $('#filter-waterbody').value;
    const g = $('#filter-guide').value;
    const st = $('#filter-status').value;
    if (wb) filters.waterbodyId = wb;
    if (g) filters.guideId = g;
    const list = $('#trips-list');
    list.innerHTML = '<div class="empty"><div class="loader"></div></div>';
    try {
        let trips = await api.trips.list(filters);
        if (st) trips = trips.filter(t => t.status === st);
        state.trips = trips;
        if (!trips.length) { list.innerHTML = '<div class="empty">Виїздів немає. Створіть перший!</div>'; return; }
        list.innerHTML = '';
        for (const t of trips) list.appendChild(await tripCard(t));
    } catch (e) { list.innerHTML = `<div class="empty">Помилка: ${e.message}</div>`; }
}

async function tripCard(t) {
    const el = document.createElement('div');
    el.className = 'card';
    const isFull = t.freeSlots <= 0;
    const isOver = t.status === 'Completed' || t.status === 'Cancelled';
    el.innerHTML = `
        <div style="display:flex;justify-content:space-between;align-items:start">
            <div><h3>${t.waterbodyName}</h3>
            <div class="meta">Гід: ${t.guideName}</div></div>
            <span class="badge ${t.status}">${t.status}</span>
        </div>
        <div class="row"><span class="label">Початок</span><span>${fmtDate(t.startAt)}</span></div>
        <div class="row"><span class="label">Кінець</span><span>${fmtDate(t.endAt)}</span></div>
        <div class="row"><span class="label">Учасники</span><span>${t.bookedCount} / ${t.maxParticipants}</span></div>
        <div class="row"><span class="label">Ціна</span><span><b>${t.pricePerPerson} грн</b></span></div>
        <div class="actions">
            ${!isOver && !isFull ? `<button class="btn primary small" data-act="book" data-id="${t.id}">Забронювати</button>` : ''}
            <button class="btn small" data-act="participants" data-id="${t.id}">Учасники</button>
            ${!isOver ? `<button class="btn small" data-act="complete" data-id="${t.id}">Завершити</button>` : ''}
            <button class="btn small" data-act="edit-trip" data-id="${t.id}">Ред.</button>
            <button class="btn danger small" data-act="del-trip" data-id="${t.id}">×</button>
        </div>`;
    el.querySelectorAll('button[data-act]').forEach(b => b.onclick = () => handleTripAction(b.dataset.act, +b.dataset.id));
    return el;
}

async function handleTripAction(act, id) {
    if (act === 'book') openBookModal(id);
    else if (act === 'participants') openParticipantsModal(id);
    else if (act === 'complete') openCompleteModal(id);
    else if (act === 'edit-trip') openTripForm(state.trips.find(t => t.id === id));
    else if (act === 'del-trip') {
        if (!confirm('Видалити виїзд?')) return;
        try { await api.trips.delete(id); toast('Видалено', 'success'); renderTrips(); }
        catch (e) { toast(e.message, 'error'); }
    }
}

function openTripForm(trip) {
    const isEdit = !!trip;
    const html = `
        <div class="form-row row2">
            <div><label>Початок</label><input type="datetime-local" id="f-start" value="${trip ? fmtDateInput(trip.startAt) : ''}"></div>
            <div><label>Кінець</label><input type="datetime-local" id="f-end" value="${trip ? fmtDateInput(trip.endAt) : ''}"></div>
        </div>
        <div class="form-row row2">
            <div><label>Макс. учасників</label><input type="number" id="f-max" min="1" max="50" value="${trip?.maxParticipants ?? 4}"></div>
            <div><label>Ціна, грн</label><input type="number" id="f-price" min="0" step="50" value="${trip?.pricePerPerson ?? 1000}"></div>
        </div>
        <div class="form-row">
            <label>Водойма</label>
            <select id="f-wb">${state.waterbodies.map(w => `<option value="${w.id}" ${trip?.waterbodyId === w.id ? 'selected' : ''}>${w.name}</option>`).join('')}</select>
        </div>
        <div class="form-row">
            <label>Гід</label>
            <select id="f-guide">${state.guides.map(g => `<option value="${g.id}" ${trip?.guideId === g.id ? 'selected' : ''}>${g.fullName}</option>`).join('')}</select>
        </div>
        <div class="form-actions">
            <button class="btn" id="f-cancel">Скасувати</button>
            <button class="btn primary" id="f-save">${isEdit ? 'Зберегти' : 'Створити'}</button>
        </div>`;
    modal(isEdit ? 'Редагувати виїзд' : 'Новий виїзд', html);
    $('#f-cancel').onclick = closeModal;
    $('#f-save').onclick = async () => {
        const dto = {
            startAt: $('#f-start').value,
            endAt: $('#f-end').value,
            maxParticipants: +$('#f-max').value,
            pricePerPerson: +$('#f-price').value,
            waterbodyId: +$('#f-wb').value,
            guideId: +$('#f-guide').value,
        };
        try {
            if (isEdit) await api.trips.update(trip.id, dto); else await api.trips.create(dto);
            toast('Збережено', 'success'); closeModal(); renderTrips();
        } catch (e) { toast(e.message, 'error'); }
    };
}

function openBookModal(tripId) {
    const html = `
        <div class="form-row">
            <label>Рибалка</label>
            <select id="b-angler">${state.anglers.map(a => `<option value="${a.id}">${a.fullName} (${a.email})</option>`).join('')}</select>
        </div>
        <div class="form-actions">
            <button class="btn" id="b-cancel">Скасувати</button>
            <button class="btn primary" id="b-save">Забронювати</button>
        </div>`;
    modal('Бронювання', html);
    $('#b-cancel').onclick = closeModal;
    $('#b-save').onclick = async () => {
        try { await api.trips.book(tripId, +$('#b-angler').value); toast('Заброньовано', 'success'); closeModal(); renderTrips(); }
        catch (e) { toast(e.message, 'error'); }
    };
}

async function openParticipantsModal(tripId) {
    modal('Учасники виїзду', '<div class="loader"></div>');
    try {
        const ps = await api.trips.participants(tripId);
        const body = ps.length
            ? '<div class="participants">' + ps.map(p => `
                <div class="pill">
                    ${p.anglerName}
                    ${p.catchWeightKg != null ? ` · улов ${p.catchWeightKg} кг` : ''}
                    <button title="Скасувати" data-aid="${p.anglerId}">×</button>
                </div>`).join('') + '</div>'
            : '<div class="empty">Немає учасників</div>';
        $('#modal-body').innerHTML = body + '<div class="form-actions"><button class="btn" id="p-close">Закрити</button></div>';
        $('#p-close').onclick = closeModal;
        $$('#modal-body button[data-aid]').forEach(b => b.onclick = async () => {
            if (!confirm('Скасувати бронювання?')) return;
            try { await api.trips.cancel(tripId, +b.dataset.aid); toast('Скасовано', 'success'); openParticipantsModal(tripId); renderTrips(); }
            catch (e) { toast(e.message, 'error'); }
        });
    } catch (e) { $('#modal-body').innerHTML = `<div class="empty">Помилка: ${e.message}</div>`; }
}

async function openCompleteModal(tripId) {
    modal('Завершення виїзду', '<div class="loader"></div>');
    try {
        const ps = await api.trips.participants(tripId);
        if (!ps.length) { $('#modal-body').innerHTML = '<div class="empty">Немає учасників — нема кого відмічати</div>'; return; }
        const html = '<p>Введіть улов кожного учасника (кг), пусто — не з\'явився:</p>'
            + ps.map(p => `
                <div class="form-row row2">
                    <div><label>${p.anglerName}</label></div>
                    <div><input type="number" step="0.1" min="0" max="500" data-aid="${p.anglerId}" placeholder="0.0"></div>
                </div>`).join('')
            + `<div class="form-actions">
                <button class="btn" id="c-cancel">Скасувати</button>
                <button class="btn primary" id="c-save">Завершити виїзд</button>
            </div>`;
        $('#modal-body').innerHTML = html;
        $('#c-cancel').onclick = closeModal;
        $('#c-save').onclick = async () => {
            const data = { catchByAnglerId: {} };
            $$('#modal-body input[data-aid]').forEach(inp => {
                if (inp.value !== '') data.catchByAnglerId[+inp.dataset.aid] = +inp.value;
            });
            try { await api.trips.complete(tripId, data); toast('Виїзд завершено', 'success'); closeModal(); renderTrips(); }
            catch (e) { toast(e.message, 'error'); }
        };
    } catch (e) { $('#modal-body').innerHTML = `<div class="empty">${e.message}</div>`; }
}

// ---------- WATERBODIES ----------
function renderWaterbodies() {
    const el = $('#waterbodies-list');
    if (!state.waterbodies.length) { el.innerHTML = '<div class="empty">Немає водойм</div>'; return; }
    el.innerHTML = `<table><thead><tr><th>Назва</th><th>Тип</th><th>Розташування</th><th>Площа, га</th><th>Риба</th><th></th></tr></thead><tbody>${
        state.waterbodies.map(w => `<tr>
            <td><b>${w.name}</b></td><td>${w.type}</td><td>${w.location}</td><td>${w.areaHa}</td><td>${w.fishSpecies || '—'}</td>
            <td><div class="actions">
                <button class="btn small" data-act="edit-wb" data-id="${w.id}">Ред.</button>
                <button class="btn danger small" data-act="del-wb" data-id="${w.id}">×</button>
            </div></td></tr>`).join('')
    }</tbody></table>`;
    el.querySelectorAll('button[data-act]').forEach(b => b.onclick = () => {
        const w = state.waterbodies.find(x => x.id === +b.dataset.id);
        if (b.dataset.act === 'edit-wb') openWaterbodyForm(w);
        else if (b.dataset.act === 'del-wb') deleteRow('waterbodies', w.id, 'Водойму');
    });
}

function openWaterbodyForm(wb) {
    const isEdit = !!wb;
    const types = ['Lake', 'River', 'Pond', 'Reservoir', 'Sea'];
    modal(isEdit ? 'Редагувати водойму' : 'Нова водойма', `
        <div class="form-row"><label>Назва</label><input id="f-name" value="${wb?.name || ''}"></div>
        <div class="form-row row2">
            <div><label>Тип</label><select id="f-type">${types.map(t => `<option ${wb?.type === t ? 'selected' : ''}>${t}</option>`).join('')}</select></div>
            <div><label>Площа, га</label><input type="number" step="0.01" id="f-area" value="${wb?.areaHa || 1}"></div>
        </div>
        <div class="form-row"><label>Розташування</label><input id="f-loc" value="${wb?.location || ''}"></div>
        <div class="form-row"><label>Види риби</label><textarea id="f-species">${wb?.fishSpecies || ''}</textarea></div>
        <div class="form-actions">
            <button class="btn" id="x">Скасувати</button>
            <button class="btn primary" id="s">${isEdit ? 'Зберегти' : 'Створити'}</button>
        </div>`);
    $('#x').onclick = closeModal;
    $('#s').onclick = async () => {
        const dto = { name: $('#f-name').value, type: $('#f-type').value, location: $('#f-loc').value, areaHa: +$('#f-area').value, fishSpecies: $('#f-species').value };
        try {
            if (isEdit) await api.waterbodies.update(wb.id, dto); else await api.waterbodies.create(dto);
            await loadAll(); renderWaterbodies(); closeModal(); toast('Збережено', 'success');
        } catch (e) { toast(e.message, 'error'); }
    };
}

// ---------- GUIDES ----------
function renderGuides() {
    const el = $('#guides-list');
    if (!state.guides.length) { el.innerHTML = '<div class="empty">Немає гідів</div>'; return; }
    el.innerHTML = `<table><thead><tr><th>Ім'я</th><th>Ліцензія</th><th>Стаж</th><th>Біо</th><th></th></tr></thead><tbody>${
        state.guides.map(g => `<tr>
            <td><b>${g.fullName}</b></td><td>${g.licenseNumber}</td><td>${g.yearsExperience} р.</td>
            <td>${(g.bio || '').slice(0, 80)}${g.bio?.length > 80 ? '…' : ''}</td>
            <td><div class="actions">
                <button class="btn small" data-act="edit-g" data-id="${g.id}">Ред.</button>
                <button class="btn danger small" data-act="del-g" data-id="${g.id}">×</button>
            </div></td></tr>`).join('')
    }</tbody></table>`;
    el.querySelectorAll('button[data-act]').forEach(b => b.onclick = () => {
        const g = state.guides.find(x => x.id === +b.dataset.id);
        if (b.dataset.act === 'edit-g') openGuideForm(g);
        else deleteRow('guides', g.id, 'Гіда');
    });
}

function openGuideForm(g) {
    const isEdit = !!g;
    modal(isEdit ? 'Редагувати гіда' : 'Новий гід', `
        <div class="form-row"><label>Повне ім'я</label><input id="f-name" value="${g?.fullName || ''}"></div>
        <div class="form-row row2">
            <div><label>№ ліцензії</label><input id="f-lic" value="${g?.licenseNumber || ''}"></div>
            <div><label>Стаж, років</label><input type="number" min="0" max="80" id="f-years" value="${g?.yearsExperience || 0}"></div>
        </div>
        <div class="form-row"><label>Біо</label><textarea id="f-bio">${g?.bio || ''}</textarea></div>
        <div class="form-actions">
            <button class="btn" id="x">Скасувати</button>
            <button class="btn primary" id="s">${isEdit ? 'Зберегти' : 'Створити'}</button>
        </div>`);
    $('#x').onclick = closeModal;
    $('#s').onclick = async () => {
        const dto = { fullName: $('#f-name').value, licenseNumber: $('#f-lic').value, yearsExperience: +$('#f-years').value, bio: $('#f-bio').value };
        try {
            if (isEdit) await api.guides.update(g.id, dto); else await api.guides.create(dto);
            await loadAll(); renderGuides(); closeModal(); toast('Збережено', 'success');
        } catch (e) { toast(e.message, 'error'); }
    };
}

// ---------- ANGLERS ----------
function renderAnglers() {
    const el = $('#anglers-list');
    if (!state.anglers.length) { el.innerHTML = '<div class="empty">Немає рибалок</div>'; return; }
    el.innerHTML = `<table><thead><tr><th>Ім'я</th><th>Email</th><th>Телефон</th><th>Рівень</th><th>З нами з</th><th></th></tr></thead><tbody>${
        state.anglers.map(a => `<tr>
            <td><b>${a.fullName}</b></td><td>${a.email}</td><td>${a.phone || '—'}</td>
            <td>${a.level}</td><td>${new Date(a.registeredAt).toLocaleDateString('uk-UA')}</td>
            <td><div class="actions">
                <button class="btn small" data-act="history" data-id="${a.id}">Історія</button>
                <button class="btn small" data-act="edit-a" data-id="${a.id}">Ред.</button>
                <button class="btn danger small" data-act="del-a" data-id="${a.id}">×</button>
            </div></td></tr>`).join('')
    }</tbody></table>`;
    el.querySelectorAll('button[data-act]').forEach(b => b.onclick = () => {
        const a = state.anglers.find(x => x.id === +b.dataset.id);
        if (b.dataset.act === 'edit-a') openAnglerForm(a);
        else if (b.dataset.act === 'del-a') deleteRow('anglers', a.id, 'Рибалку');
        else if (b.dataset.act === 'history') openAnglerHistory(a);
    });
}

function openAnglerForm(a) {
    const isEdit = !!a;
    const levels = ['Beginner', 'Intermediate', 'Advanced', 'Expert'];
    modal(isEdit ? 'Редагувати' : 'Новий рибалка', `
        <div class="form-row"><label>Повне ім'я</label><input id="f-name" value="${a?.fullName || ''}"></div>
        <div class="form-row row2">
            <div><label>Email</label><input type="email" id="f-email" value="${a?.email || ''}"></div>
            <div><label>Телефон</label><input id="f-phone" value="${a?.phone || ''}"></div>
        </div>
        <div class="form-row"><label>Рівень</label><select id="f-level">${levels.map(l => `<option ${a?.level === l ? 'selected' : ''}>${l}</option>`).join('')}</select></div>
        <div class="form-actions">
            <button class="btn" id="x">Скасувати</button>
            <button class="btn primary" id="s">${isEdit ? 'Зберегти' : 'Створити'}</button>
        </div>`);
    $('#x').onclick = closeModal;
    $('#s').onclick = async () => {
        const dto = { fullName: $('#f-name').value, email: $('#f-email').value, phone: $('#f-phone').value, level: $('#f-level').value };
        try {
            if (isEdit) await api.anglers.update(a.id, dto); else await api.anglers.create(dto);
            await loadAll(); renderAnglers(); closeModal(); toast('Збережено', 'success');
        } catch (e) { toast(e.message, 'error'); }
    };
}

async function openAnglerHistory(a) {
    modal(`Історія: ${a.fullName}`, '<div class="loader"></div>');
    try {
        const trips = await api.anglers.trips(a.id);
        const body = trips.length
            ? trips.map(t => `<div class="card" style="margin-bottom:8px">
                <div style="display:flex;justify-content:space-between">
                    <b>${t.waterbodyName}</b><span class="badge ${t.status}">${t.status}</span>
                </div>
                <div class="meta">${fmtDate(t.startAt)} · гід ${t.guideName}</div>
            </div>`).join('')
            : '<div class="empty">Виїздів не було</div>';
        $('#modal-body').innerHTML = body + '<div class="form-actions"><button class="btn" id="h-close">Закрити</button></div>';
        $('#h-close').onclick = closeModal;
    } catch (e) { $('#modal-body').innerHTML = `<div class="empty">${e.message}</div>`; }
}

// ---------- Generic delete ----------
async function deleteRow(resource, id, label) {
    if (!confirm(`Видалити ${label.toLowerCase()}?`)) return;
    try { await api[resource].delete(id); toast('Видалено', 'success'); await loadAll(); renderCurrent(); }
    catch (e) { toast(e.message, 'error'); }
}

// ---------- Wire up ----------
$('#btn-new-trip').onclick = () => openTripForm(null);
$('#btn-new-waterbody').onclick = () => openWaterbodyForm(null);
$('#btn-new-guide').onclick = () => openGuideForm(null);
$('#btn-new-angler').onclick = () => openAnglerForm(null);
['filter-waterbody', 'filter-guide', 'filter-status'].forEach(id => $('#' + id).onchange = renderTrips);
$('#btn-clear-filters').onclick = () => {
    $('#filter-waterbody').value = ''; $('#filter-guide').value = ''; $('#filter-status').value = '';
    renderTrips();
};

// ---------- Init ----------
(async function init() {
    try { await loadAll(); await renderTrips(); }
    catch (e) { toast('Не вдалося завантажити: ' + e.message, 'error'); }
})();
