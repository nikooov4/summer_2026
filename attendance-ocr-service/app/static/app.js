const imageInput = document.getElementById("imageInput");
const demoCaseButton = document.getElementById("demoCaseButton");
const openUploadButton = document.getElementById("openUploadButton");
const cancelUploadButton = document.getElementById("cancelUploadButton");
const processUploadButton = document.getElementById("processUploadButton");
const uploadModal = document.getElementById("uploadModal");
const dropzone = document.getElementById("dropzone");
const dropzoneEmpty = document.getElementById("dropzoneEmpty");
const previewGrid = document.getElementById("previewGrid");
const demoCaseMeta = document.getElementById("demoCaseMeta");
const selectedFileName = document.getElementById("selectedFileName");
const statusText = document.getElementById("statusText");
const exportButton = document.getElementById("exportButton");
const confirmButton = document.getElementById("confirmButton");
const closeReviewButton = document.getElementById("closeReviewButton");
const reviewDrawer = document.getElementById("reviewDrawer");
const studentList = document.getElementById("studentList");
const summaryGrid = document.getElementById("summaryGrid");
const jsonOutput = document.getElementById("jsonOutput");
const attendanceTableBody = document.getElementById("attendanceTableBody");
const attendanceTable = document.querySelector(".grade-table");
const weekLabel = document.querySelector(".week-block strong");
const weekHint = document.querySelector(".week-block em");

const state = {
  files: [],
  previewUrls: [],
  images: [],
  demoCase: null,
  demoJournal: null,
  reviewConfirmed: false,
  job: null,
  review: null,
  exportResult: null,
};

demoCaseButton.addEventListener("click", () => loadDemoCase());
openUploadButton.addEventListener("click", () => openModal());
cancelUploadButton.addEventListener("click", () => closeModal({ resetFile: true }));
closeReviewButton.addEventListener("click", () => {
  reviewDrawer.hidden = true;
});

uploadModal.addEventListener("click", (event) => {
  if (event.target.matches("[data-close-modal]")) {
    closeModal();
  }
});

dropzone.addEventListener("dragover", (event) => {
  event.preventDefault();
  dropzone.classList.add("is-dragover");
});

dropzone.addEventListener("dragleave", () => {
  dropzone.classList.remove("is-dragover");
});

dropzone.addEventListener("drop", (event) => {
  event.preventDefault();
  dropzone.classList.remove("is-dragover");
  clearDemoCase();
  selectFiles(event.dataTransfer?.files);
});

imageInput.addEventListener("change", () => {
  clearDemoCase();
  selectFiles(imageInput.files);
});

processUploadButton.addEventListener("click", async () => {
  if (!state.files.length) {
    imageInput.click();
    return;
  }
  await processFiles(state.files);
});

confirmButton.addEventListener("click", async () => {
  if (!state.job) return;
  try {
    confirmButton.disabled = true;
    confirmButton.textContent = "Подтверждение...";
    const response = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/review/confirm`, {
      method: "POST",
    });
    if (!response.ok) throw await apiError(response);
    state.job = await response.json();
    state.reviewConfirmed = true;
    const reviewResponse = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/review`);
    if (!reviewResponse.ok) throw await apiError(reviewResponse);
    state.review = await reviewResponse.json();
    renderReview();
    confirmButton.textContent = "Подтверждено";
  } catch (error) {
    confirmButton.disabled = false;
    confirmButton.textContent = "Подтвердить";
    showError(error);
  }
});

exportButton.addEventListener("click", async () => {
  if (!state.job) return;
  try {
    const response = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/export`, {
      method: "POST",
    });
    if (!response.ok) throw await apiError(response);
    state.exportResult = await response.json();
    exportButton.disabled = true;
    renderJson();
  } catch (error) {
    showError(error);
  }
});

function openModal() {
  uploadModal.hidden = false;
  setStatus(state.files.length ? selectedFilesLabel() : "");
}

function closeModal(options = {}) {
  uploadModal.hidden = true;
  if (options.resetFile) {
    imageInput.value = "";
    clearSelectedFiles();
    setStatus("");
  }
}

function selectFiles(fileList) {
  const files = Array.from(fileList || []).filter((file) => file.type.startsWith("image/"));
  if (!files.length) {
    setStatus("Выберите изображение в формате PNG или JPG.");
    return;
  }

  const oversized = files.find((file) => file.size > 10 * 1024 * 1024);
  if (oversized) {
    setStatus(`Файл ${oversized.name} больше 10 МБ.`);
    return;
  }

  clearPreviewUrls();
  state.files = files;
  state.previewUrls = files.map((file) => URL.createObjectURL(file));
  renderPreviews();
  setStatus(selectedFilesLabel());
}

function clearSelectedFiles() {
  clearPreviewUrls();
  state.files = [];
  state.images = [];
  state.demoJournal = null;
  state.reviewConfirmed = false;
  clearDemoCase();
  renderPreviews();
}

async function processFiles(files) {
  processUploadButton.disabled = true;
  state.images = [];
  state.reviewConfirmed = false;
  setStatus(files.length === 1 ? "Загрузка изображения..." : `Загрузка изображений: 0 из ${files.length}`);

  try {
    for (const [index, file] of files.entries()) {
      const formData = new FormData();
      formData.append("file", file);
      const imageResponse = await fetch("/attendance/ocr/images", {
        method: "POST",
        body: formData,
      });
      if (!imageResponse.ok) throw await apiError(imageResponse);
      state.images.push(await imageResponse.json());
      if (files.length > 1) setStatus(`Загрузка изображений: ${index + 1} из ${files.length}`);
    }

    setStatus("Создание задачи...");
    const jobResponse = await fetch("/attendance/ocr/jobs", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        image_ids: state.images.map((image) => image.image_id),
        lesson: state.demoCase?.lesson || {
          date: "2026-05-20",
          group: "PI-101",
          subject: "Algorithms",
          teacher_id: "iiks-demo",
        },
      }),
    });
    if (!jobResponse.ok) throw await apiError(jobResponse);
    state.job = await jobResponse.json();

    setStatus("OCR обработка...");
    const mode = encodeURIComponent(state.demoCase?.mode || "stub");
    const processResponse = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/process?mode=${mode}`, {
      method: "POST",
    });
    if (!processResponse.ok) throw await apiError(processResponse);
    state.job = await processResponse.json();

    setStatus("Формирование проверки...");
    const reviewResponse = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/review`);
    if (!reviewResponse.ok) throw await apiError(reviewResponse);
    state.review = await reviewResponse.json();
    state.exportResult = null;

    closeModal();
    reviewDrawer.hidden = false;
    renderReview();
  } catch (error) {
    showError(error);
    setStatus(error?.detail || error?.message || "Ошибка обработки.");
  } finally {
    processUploadButton.disabled = false;
  }
}

function renderReview() {
  const review = state.review;
  if (!review?.decisions?.length) {
    studentList.innerHTML = '<p class="empty">Students were not found.</p>';
    return;
  }

  renderSummary(review.summary);
  renderAttendanceTable(review.decisions);
  const confirmed = state.reviewConfirmed;
  confirmButton.disabled = review.summary.needs_review_count > 0 || confirmed;
  confirmButton.textContent = confirmed ? "Подтверждено" : "Подтвердить";
  exportButton.disabled = state.job?.status !== "completed" || !confirmed;
  renderJson();

  studentList.innerHTML = review.decisions
    .map((decision) => {
      const statusClass = decision.resolved ? decision.status : "review";
      const statusText = decision.resolved ? statusLabel(decision.status) : "проверить";
      const name = decision.final_full_name || "Не сопоставлено";
      const candidates = (decision.candidates || [])
        .map((candidate) => `<option value="${escapeHtml(candidate.student_id)}">${escapeHtml(candidate.full_name)}</option>`)
        .join("");
      const controls = decision.resolved
        ? ""
        : `
          <div class="decision-actions">
            <button type="button" data-action="accept" data-decision-id="${escapeHtml(decision.decision_id)}">Подтвердить</button>
            <button type="button" data-action="mark_absent" data-decision-id="${escapeHtml(decision.decision_id)}">Отсутствует</button>
            <button type="button" data-action="ignore" data-decision-id="${escapeHtml(decision.decision_id)}">Не учитывать</button>
            ${
              candidates
                ? `<select data-candidate-for="${escapeHtml(decision.decision_id)}">${candidates}</select>
                   <button type="button" data-action="select_student" data-decision-id="${escapeHtml(decision.decision_id)}">Выбрать</button>`
                : ""
            }
          </div>
        `;
      return `
        <article class="student" data-resolved="${decision.resolved}">
          <div class="student-head">
            <div>
              <strong class="student-name">${escapeHtml(name)}</strong>
              <div class="student-meta">
                <span>OCR: ${escapeHtml(decision.source_text || "нет текста")}</span>
                <span>Уверенность: ${Number(decision.confidence || 0).toFixed(2)}</span>
              </div>
            </div>
            <span class="badge ${statusClass}">${statusText}</span>
          </div>
          ${controls}
        </article>
      `;
    })
    .join("");

  studentList.querySelectorAll("button[data-action]").forEach((button) => {
    button.addEventListener("click", () => applyDecision(button.dataset.decisionId, button.dataset.action));
  });
}

function renderSummary(summary) {
  summaryGrid.innerHTML = `
    <div><strong>${summary.auto_present_count ?? 0}</strong><span>зачтено</span></div>
    <div><strong>${summary.needs_review_count ?? 0}</strong><span>проверить</span></div>
    <div><strong>${summary.absent_count ?? 0}</strong><span>отсутствуют</span></div>
    <div><strong>${summary.unmatched_count ?? 0}</strong><span>не сопоставлено</span></div>
    <div><strong>${summary.confirmed_count ?? 0}</strong><span>подтверждено</span></div>
  `;
}

function renderAttendanceTable(decisions) {
  if (state.demoJournal?.rows?.length) {
    renderDemoJournalTable(decisions);
    return;
  }

  const rows = Array.from(attendanceTableBody.querySelectorAll("tr"));
  decisions.slice(0, rows.length).forEach((decision, index) => {
    const row = rows[index];
    const nameCell = row.querySelector("th span");
    const groupCell = row.querySelector("th small");
    const targetCell = row.querySelectorAll("td")[2];
    nameCell.textContent = decision.final_full_name || decision.source_text || nameCell.textContent;
    groupCell.textContent = groupForDecision(decision) || state.review?.lesson?.group || groupCell.textContent;

    if (!decision.resolved) {
      targetCell.innerHTML = '<button class="mark note" type="button">?</button>';
    } else if (decision.status === "present") {
      targetCell.innerHTML = '<button class="mark present" type="button">✓</button>';
    } else if (decision.status === "absent") {
      targetCell.innerHTML = '<button class="mark absent" type="button">Н</button>';
    } else {
      targetCell.innerHTML = '<button class="mark note" type="button">-</button>';
    }
  });
}

function renderDemoJournalTable(decisions) {
  const presentNames = new Set(
    decisions
      .filter((decision) => decision.status === "present" && decision.final_full_name)
      .map((decision) => decision.final_full_name),
  );
  const dateColumns = state.demoJournal.dateColumns;
  const targetDate = state.demoCase?.expected?.source_date || "20-05-2026";
  const columns = ["Student / Group", ...dateColumns, "Total"];

  attendanceTable.classList.add("journal-table");
  attendanceTable.querySelector("thead").innerHTML = `
    <tr>
      ${columns.map((column) => `<th>${escapeHtml(formatJournalHeader(column))}</th>`).join("")}
    </tr>
  `;

  attendanceTableBody.innerHTML = state.demoJournal.rows
    .map((row) => {
      const cells = [
        `<th><span>${escapeHtml(row["Student name"])}</span><small>${escapeHtml(row["Group"])}</small></th>`,
        ...dateColumns.map((column) => {
          const fromFinalJournal = (row[column] || "").trim() === "+";
          const fromCurrentOcr = column === targetDate && presentNames.has(row["Student name"]);
          const mark = fromFinalJournal || fromCurrentOcr;
          return `<td>${mark ? '<button class="mark present" type="button">✓</button>' : '<button class="mark absent" type="button">Н</button>'}</td>`;
        }),
        `<td class="total-cell">${escapeHtml(row[state.demoJournal.totalColumn] || "")}</td>`,
      ];
      return `<tr>${cells.join("")}</tr>`;
    })
    .join("");

  if (weekLabel) weekLabel.textContent = "18 May - 24 May 2026";
  if (weekHint) weekHint.textContent = "Demo: synthetic PI-101 attendance journal";
}

async function applyDecision(decisionId, action) {
  if (!state.job || !decisionId || !action) return;
  const decision = state.review.decisions.find((item) => item.decision_id === decisionId);
  const correction = {
    decision_id: decisionId,
    action,
    region_id: decision?.region_ids?.[0] || null,
    image_id: state.images[0]?.image_id || null,
  };
  if (action === "select_student") {
    const select = studentList.querySelector(`select[data-candidate-for="${CSS.escape(decisionId)}"]`);
    correction.selected_student_id = select?.value || null;
  }

  try {
    const response = await fetch(`/attendance/ocr/jobs/${encodeURIComponent(state.job.job_id)}/review`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ corrections: [correction] }),
    });
    if (!response.ok) throw await apiError(response);
    state.review = await response.json();
    renderReview();
  } catch (error) {
    showError(error);
  }
}

function renderJson() {
  jsonOutput.textContent = JSON.stringify(
    {
      image: state.images[0] || null,
      images: state.images,
      demoCase: state.demoCase,
      job: state.job,
      review: state.review,
      export: state.exportResult,
    },
    null,
    2,
  );
}

async function apiError(response) {
  const error = await response.json().catch(() => ({}));
  const detail = typeof error.detail === "object" ? error.detail.message : error.detail;
  return { detail: detail || `HTTP ${response.status}` };
}

function showError(error) {
  const detail = error?.detail || error?.message || "Ошибка выполнения действия.";
  reviewDrawer.hidden = false;
  studentList.insertAdjacentHTML("afterbegin", `<p class="empty">${escapeHtml(detail)}</p>`);
  jsonOutput.textContent = JSON.stringify(error, null, 2);
}

function statusLabel(status) {
  const labels = {
    completed: "подтверждено",
    present: "зачтено",
    absent: "отсутствует",
    ignored: "не учитывать",
    not_a_student: "не студент",
    needs_review: "проверить",
  };
  return labels[status] || status;
}

function setStatus(value) {
  statusText.textContent = value || "";
  statusText.classList.toggle("is-visible", Boolean(value));
}

function selectedFilesLabel() {
  if (state.files.length === 1) return `Выбран файл: ${state.files[0].name}`;
  return `Выбрано файлов: ${state.files.length}`;
}

async function loadDemoCase() {
  try {
    setStatus("Загрузка демо-кейса...");
    const manifestResponse = await fetch("/static/demo/public-sample/manifest.json");
    if (!manifestResponse.ok) throw new Error("Не удалось загрузить manifest демо-кейса.");
    const manifest = await manifestResponse.json();
    const files = [];
    for (const item of manifest.files || []) {
      const response = await fetch(item.url);
      if (!response.ok) throw new Error(`Не удалось загрузить ${item.filename}.`);
      const blob = await response.blob();
      files.push(new File([blob], item.filename, { type: blob.type || "image/jpeg" }));
    }
    state.demoJournal = await loadDemoJournal(manifest);
    state.demoCase = manifest;
    selectFiles(files);
    renderDemoCaseMeta();
    openModal();
    setStatus(`Демо-кейс готов: ${files.length} фото, ожидается ${manifest.expected?.recognized_unique_students || "несколько"} студентов.`);
  } catch (error) {
    clearDemoCase();
    showError(error);
    setStatus(error?.message || "Не удалось подготовить демо-кейс.");
  }
}

function renderDemoCaseMeta() {
  const demo = state.demoCase;
  if (!demo) {
    demoCaseMeta.hidden = true;
    demoCaseMeta.innerHTML = "";
    return;
  }

  demoCaseMeta.hidden = false;
  demoCaseMeta.innerHTML = `
    <strong>${escapeHtml(demo.title)}</strong>
    <span>${escapeHtml(demo.lesson?.date || "")} · ${escapeHtml(demo.lesson?.group || "")} · ${escapeHtml(demo.description || "")}</span>
  `;
}

function clearDemoCase() {
  state.demoCase = null;
  state.demoJournal = null;
  renderDemoCaseMeta();
}

function renderPreviews() {
  if (!state.files.length) {
    dropzoneEmpty.hidden = false;
    previewGrid.hidden = true;
    previewGrid.innerHTML = "";
    selectedFileName.textContent = "Нажмите или перетащите файлы";
    return;
  }

  dropzoneEmpty.hidden = true;
  previewGrid.hidden = false;
  previewGrid.innerHTML = state.files
    .map(
      (file, index) => `
        <span class="preview-card">
          <img src="${state.previewUrls[index]}" alt="${escapeHtml(file.name)}" />
          <span>
            <strong>${escapeHtml(file.name)}</strong>
            <small>${formatFileSize(file.size)}</small>
          </span>
        </span>
      `,
    )
    .join("");
}

function clearPreviewUrls() {
  state.previewUrls.forEach((url) => URL.revokeObjectURL(url));
  state.previewUrls = [];
}

function formatFileSize(bytes) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} КБ`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} МБ`;
}

async function loadDemoJournal(manifest) {
  const csvUrl = manifest?.artifacts?.csv;
  if (!csvUrl) return null;
  const response = await fetch(csvUrl);
  if (!response.ok) throw new Error("Не удалось загрузить CSV журнала демо-кейса.");
  const rows = parseCsv(await response.text());
  const headers = rows.headers || [];
  const dateColumns = headers.filter((header) => /^\d{2}-\d{2}-\d{4}$/.test(header));
  const totalColumn = headers.find((header) => header.startsWith("Total")) || headers[headers.length - 1];
  return { rows: rows.records, headers, dateColumns, totalColumn };
}

function parseCsv(text) {
  const rows = [];
  let cell = "";
  let row = [];
  let quoted = false;
  const source = text.replace(/^\uFEFF/, "");

  for (let index = 0; index < source.length; index += 1) {
    const char = source[index];
    const next = source[index + 1];
    if (quoted && char === '"' && next === '"') {
      cell += '"';
      index += 1;
    } else if (char === '"') {
      quoted = !quoted;
    } else if (!quoted && char === ",") {
      row.push(cell);
      cell = "";
    } else if (!quoted && (char === "\n" || char === "\r")) {
      if (char === "\r" && next === "\n") index += 1;
      row.push(cell);
      if (row.some((value) => value.length)) rows.push(row);
      row = [];
      cell = "";
    } else {
      cell += char;
    }
  }
  if (cell.length || row.length) {
    row.push(cell);
    rows.push(row);
  }

  const headers = rows.shift() || [];
  const records = rows.map((values) =>
    Object.fromEntries(headers.map((header, index) => [header, values[index] || ""])),
  );
  return { headers, records };
}

function formatJournalHeader(column) {
  if (/^\d{2}-\d{2}-\d{4}$/.test(column)) return column.slice(0, 5).replace("-", ".");
  if (column.startsWith("Total")) return "Total";
  return column;
}

function groupForDecision(decision) {
  const regionId = decision.region_ids?.[0];
  if (!regionId || !state.review?.ocr?.length) return null;
  for (const image of state.review.ocr) {
    const region = (image.regions || []).find((item) => item.region_id === regionId);
    if (region?.raw?.group) return region.raw.group;
  }
  return null;
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}
