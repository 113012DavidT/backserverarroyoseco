const API_URL = "/api/Gastronomias";
const CACHE_KEY = "gastronomia:last-data:v1";
const form = document.getElementById("search-form");
const statusEl = document.getElementById("app-status");
const listEl = document.getElementById("resultados");
const emptyEl = document.getElementById("vacio");
const loadingEl = document.getElementById("loading");
const installBtn = document.getElementById("install-btn");
const statTotal = document.getElementById("stat-total");
const statCache = document.getElementById("stat-cache");

let deferredPrompt = null;
let currentData = [];
let cacheHit = false;

function setStatus(text) {
  statusEl.textContent = text;
}

function normalizeText(value) {
  return String(value || "").trim().toLowerCase();
}

function formatItem(item) {
  const nombre = item.nombre || item.Nombre || "Sin nombre";
  const ubicacion = item.ubicacion || item.Ubicacion || "Ubicacion no disponible";
  const mesas = item.mesas?.length || item.Mesas?.length || 0;

  return `
    <li>
      <h3 class="result-name">${nombre}</h3>
      <p class="result-meta">${ubicacion} | ${mesas} mesas registradas</p>
    </li>
  `;
}

function render(items) {
  currentData = items;
  listEl.innerHTML = items.map(formatItem).join("");
  statTotal.textContent = String(items.length);
  statCache.textContent = cacheHit ? "100%" : "0%";
  emptyEl.hidden = items.length > 0;
}

function readCachedData() {
  try {
    const raw = localStorage.getItem(CACHE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed.data)) return null;
    return parsed;
  } catch {
    return null;
  }
}

function writeCachedData(data) {
  try {
    localStorage.setItem(CACHE_KEY, JSON.stringify({
      date: new Date().toISOString(),
      data
    }));
  } catch {
    // localStorage puede fallar en modo privado.
  }
}

async function fetchData() {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 9000);

  try {
    const response = await fetch(API_URL, {
      method: "GET",
      signal: controller.signal,
      headers: {
        "Accept": "application/json"
      }
    });

    if (!response.ok) {
      throw new Error(`Error HTTP ${response.status}`);
    }

    const data = await response.json();
    const list = Array.isArray(data) ? data : [];
    cacheHit = false;
    writeCachedData(list);
    setStatus("Datos actualizados desde el servidor");
    return list;
  } finally {
    clearTimeout(timeoutId);
  }
}

function applyFilters() {
  const ubicacion = normalizeText(document.getElementById("filtro-ubicacion").value);
  const personas = Number.parseInt(document.getElementById("filtro-personas").value, 10) || 1;

  const filtered = currentData.filter((item) => {
    const rawUbicacion = item.ubicacion || item.Ubicacion || "";
    const itemUbicacion = normalizeText(rawUbicacion);
    const mesas = item.mesas || item.Mesas || [];

    const byLocation = !ubicacion || itemUbicacion.includes(ubicacion);
    const byCapacity = mesas.length === 0 || mesas.some((mesa) => {
      const capacidad = mesa.capacidad ?? mesa.Capacidad ?? 0;
      return capacidad >= personas;
    });

    return byLocation && byCapacity;
  });

  render(filtered);
}

async function loadData() {
  loadingEl.hidden = false;

  const cached = readCachedData();
  if (cached?.data?.length) {
    cacheHit = true;
    render(cached.data);
    setStatus(`Mostrando cache local (${new Date(cached.date).toLocaleString()})`);
  }

  try {
    const fromApi = await fetchData();
    render(fromApi);
  } catch (error) {
    if (!cached?.data?.length) {
      setStatus("No fue posible cargar datos. Revisa conexion o CORS.");
      render([]);
    } else {
      setStatus("Sin red: usando datos guardados.");
    }
    console.error(error);
  } finally {
    loadingEl.hidden = true;
  }
}

function registerInstallPrompt() {
  window.addEventListener("beforeinstallprompt", (event) => {
    event.preventDefault();
    deferredPrompt = event;
    installBtn.hidden = false;
  });

  installBtn.addEventListener("click", async () => {
    if (!deferredPrompt) return;

    deferredPrompt.prompt();
    await deferredPrompt.userChoice;
    deferredPrompt = null;
    installBtn.hidden = true;
  });
}

function registerServiceWorker() {
  if (!("serviceWorker" in navigator)) return;

  window.addEventListener("load", async () => {
    try {
      await navigator.serviceWorker.register("/publica/gastronomia/sw.js", {
        scope: "/publica/gastronomia/"
      });
      setStatus("PWA activa. Cache inteligente habilitado.");
    } catch (error) {
      console.error("No se pudo registrar service worker", error);
      setStatus("No se pudo activar cache offline.");
    }
  });
}

form.addEventListener("submit", (event) => {
  event.preventDefault();
  applyFilters();
});

registerInstallPrompt();
registerServiceWorker();
loadData();
