// src/pages/MapPage/MapPage.jsx
import React, { useEffect, useMemo, useState } from "react";
import axiosInstance from "../../../axios/axios.js";
import {
    MapContainer,
    TileLayer,
    Popup,
    Tooltip,
    CircleMarker,
    useMap,
    LayersControl,
    ScaleControl,
} from "react-leaflet";
import "leaflet/dist/leaflet.css";
import L from "leaflet";
import "leaflet.heat";
import { Box, Button, MenuItem, Select, TextField } from "@mui/material";

/* =========================================================================
   Помошни функции (реупотребливи и изолирани)
   ========================================================================= */

/** Ако нема decimal, сметај од степени/минути/секунди */
const toDecimal = (deg, min, sec) =>
    typeof deg === "number" && typeof min === "number" && typeof sec === "number"
        ? deg + min / 60 + sec / 3600
        : null;

/** Извлечи [lat,lng] од measurement (поддржува и DMS и decimal) */
const getLatLng = (m) => {
    const lat =
        m.latitudeDecimal ??
        toDecimal(m.latitudeDegrees, m.latitudeMinutes, m.latitudeSeconds);
    const lng =
        m.longitudeDecimal ??
        toDecimal(m.longitudeDegrees, m.longitudeMinutes, m.longitudeSeconds);
    if (typeof lat === "number" && typeof lng === "number") return [lat, lng];
    return null;
};

/* ----------  Датуми и URL helpers  ---------- */

/** Конвертира локален input датум (од <input type="datetime-local">) во ISO UTC со 'Z'.
 *  Ако endOfDay=true → 23:59:59.999Z, инаку → 00:00:00.000Z.
 */
const toUtcIso = (d, endOfDay = false) => {
    if (!d) return undefined;
    const local = new Date(d);
    if (!endOfDay) local.setHours(0, 0, 0, 0);
    else local.setHours(23, 59, 59, 999);
    const utc = new Date(
        Date.UTC(
            local.getFullYear(),
            local.getMonth(),
            local.getDate(),
            local.getHours(),
            local.getMinutes(),
            local.getSeconds(),
            local.getMilliseconds()
        )
    );
    return utc.toISOString();
};

/** ISO → формат за <input type="datetime-local"> (YYYY-MM-DDTHH:mm) */
const isoToInputLocal = (iso) => {
    if (!iso) return "";
    const ms = Date.parse(iso);
    if (Number.isNaN(ms)) return "";
    const d = new Date(ms);
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(
        d.getHours()
    )}:${pad(d.getMinutes())}`;
};

/** Прочитај филтри од URL (?technology=..&dateFrom=ISO&dateTo=ISO) */
const readFiltersFromUrl = () => {
    const p = new URLSearchParams(window.location.search);
    return {
        technology: p.get("technology") || "",
        dateFromIso: p.get("dateFrom") || "",
        dateToIso: p.get("dateTo") || "",
    };
};

/** Запиши филтри во URL без reload (за shareable линк) */
const writeFiltersToUrl = (params) => {
    const p = new URLSearchParams(window.location.search);
    Object.entries(params).forEach(([k, v]) => {
        if (v) p.set(k, v);
        else p.delete(k);
    });
    const newUrl = `${window.location.pathname}?${p.toString()}`;
    window.history.replaceState(null, "", newUrl);
};

/* ----------  CSV export helpers  ---------- */

/** Ескеп на CSV поле според RFC4180 */
const csvEscape = (v) => {
    if (v == null) return "";
    const s = String(v);
    return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
};

/** Преземи CSV од тековните measurements */
const downloadCsv = (rows) => {
    if (!rows?.length) return;
    const header = [
        "id",
        "date",
        "technology",
        "electricFielddbuvPerM",
        "channelNumber",
        "frequencyMHz",
        "latitude",
        "longitude",
        "testLocation",
        "settlementName",
        "municipalityName",
    ];

    const body = rows
        .map((m) => {
            const pos = getLatLng(m) || [null, null];
            return [
                m.id,
                m.date,
                m.technology,
                m.electricFielddbuvPerM,
                m.channelNumber,
                m.frequencyMHz,
                pos[0],
                pos[1],
                m.testLocation,
                m.settlementName,
                m.municipalityName,
            ]
                .map(csvEscape)
                .join(",");
        })
        .join("\n");

    // BOM (\uFEFF) за Excel да препознае UTF-8 правилно
    const blob = new Blob(["\uFEFF" + header.join(",") + "\n" + body], {
        type: "text/csv;charset=utf-8;",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "measurements.csv";
    a.click();
    URL.revokeObjectURL(url);
};

/* ----------  Бои/радиус (со нормализација на сетот)  ---------- */

function lerp(a, b, t) {
    return a + (b - a) * t;
}
function lerpColor(c1, c2, t) {
    const a = parseInt(c1.slice(1), 16);
    const b = parseInt(c2.slice(1), 16);
    const r = Math.round(lerp((a >> 16) & 255, (b >> 16) & 255, t));
    const g = Math.round(lerp((a >> 8) & 255, (b >> 8) & 255, t));
    const bch = Math.round(lerp(a & 255, b & 255, t));
    return `#${((1 << 24) + (r << 16) + (g << 8) + bch).toString(16).slice(1)}`;
}
const stops = ["#2e7d32", "#fbc02d", "#f57c00", "#c62828"]; // green→yellow→orange→red

/** Едноставен перцентил (со сортирање) за стабилна нормализација */
const percentile = (arr, p) => {
    if (!arr.length) return undefined;
    const a = [...arr].sort((x, y) => x - y);
    const idx = Math.min(a.length - 1, Math.max(0, Math.round((p / 100) * (a.length - 1))));
    return a[idx];
};

function getGradientColor(v, min, max) {
    if (v == null || Number.isNaN(v)) return "#1976d2";
    const lo = min ?? 40,
        hi = Math.max(lo + 1, max ?? 170);
    const x = Math.max(lo, Math.min(hi, v));
    const t = (x - lo) / (hi - lo);
    if (t <= 1 / 3) return lerpColor(stops[0], stops[1], t * 3);
    if (t <= 2 / 3) return lerpColor(stops[1], stops[2], (t - 1 / 3) * 3);
    return lerpColor(stops[2], stops[3], (t - 2 / 3) * 3);
}
const getRadius = (v, min, max) => {
    if (v == null) return 6;
    const lo = min ?? 40,
        hi = Math.max(lo + 1, max ?? 170);
    const x = Math.max(lo, Math.min(hi, v));
    return 4 + ((x - lo) / (hi - lo)) * 8; // 4..12 px
};

/* =========================================================================
   Leaflet помошни компоненти
   ========================================================================= */

/** Автоматски fit на мапата според тековните точки */
function FitToData({ points }) {
    const map = useMap();
    useEffect(() => {
        if (!points?.length) return;
        if (points.length === 1) {
            map.setView(points[0], 13);
        } else {
            const bounds = L.latLngBounds(points);
            map.fitBounds(bounds, { padding: [40, 40] });
        }
    }, [points, map]);
    return null;
}

/** Легенда со динамички min/max (од тековниот сет) */
function LegendControl({ minVal, maxVal }) {
    const map = useMap();
    useEffect(() => {
        const legend = L.control({ position: "bottomright" });
        legend.onAdd = () => {
            const div = L.DomUtil.create("div", "signal-legend");
            div.innerHTML = `
        <div style="
          background:#fff;border-radius:12px;box-shadow:0 6px 20px rgba(0,0,0,.15);
          padding:12px 14px;font: 12px/1.2 system-ui, -apple-system, Segoe UI, Roboto, Arial;">
          <div style="font-weight:600; margin-bottom:8px;">E-field (dBµV/m)</div>
          <div style="display:flex; align-items:center; gap:10px;">
            <span style="width:34px; text-align:right;">${minVal ?? 40}</span>
            <div style="flex:1;height:12px;border-radius:6px;
              background: linear-gradient(90deg, ${stops[0]}, ${stops[1]}, ${stops[2]}, ${stops[3]});
              outline:1px solid rgba(0,0,0,.08);"></div>
            <span style="width:34px;">${maxVal ?? 170}</span>
          </div>
          <div style="display:flex; justify-content:space-between; margin-top:6px; opacity:.8;">
            <span>слаб</span><span>силен</span>
          </div>
        </div>`;
            return div;
        };
        legend.addTo(map);
        return () => legend.remove();
    }, [map, minVal, maxVal]);
    return null;
}

/** Heatmap со радиус што се прилагодува на zoom */
function HeatmapOverlay({
                            points,
                            baseRadius = 18,
                            blur = 20,
                            max = 1.0,
                            minOpacity = 0.2,
                            gradient,
                        }) {
    const map = useMap();
    const [radius, setRadius] = useState(baseRadius);

    useEffect(() => {
        if (!map) return;
        const update = () => {
            const z = map.getZoom();
            const r = Math.max(10, Math.min(40, Math.round(z * 2))); // едноставно скалирање
            setRadius(r);
        };
        update();
        map.on("zoomend", update);
        return () => map.off("zoomend", update);
    }, [map]);

    useEffect(() => {
        if (!map || !points?.length) return;
        const layer = L.heatLayer(points, {
            radius,
            blur,
            max,
            minOpacity,
            gradient: gradient ?? {
                0.0: "#2e7d32",
                0.33: "#fbc02d",
                0.66: "#f57c00",
                1.0: "#c62828",
            },
        });
        layer.addTo(map);
        return () => layer.remove();
    }, [map, points, radius, blur, max, minOpacity, gradient]);

    return null;
}

/* =========================================================================
   Главна компонента
   ========================================================================= */

const MapPage = () => {
    const [measurements, setMeasurements] = useState([]);
    const [loading, setLoading] = useState(true);

    // Филтри во состојба (UI)
    const [technology, setTechnology] = useState("");
    const [dateFrom, setDateFrom] = useState("");
    const [dateTo, setDateTo] = useState("");

    /** Централизирано вчитување според params (филтри) */
    async function fetchMeasurements(params = {}) {
        try {
            const res = await axiosInstance.get("/measurements", { params });
            setMeasurements(res.data || []);
        } catch (err) {
            console.error("Failed to load measurements", err);
        } finally {
            setLoading(false);
        }
    }

    /** Инициализација: прочитај филтри од URL, примени ги и направи fetch */
    useEffect(() => {
        const { technology, dateFromIso, dateToIso } = readFiltersFromUrl();

        if (technology) setTechnology(technology);
        if (dateFromIso) setDateFrom(isoToInputLocal(dateFromIso));
        if (dateToIso) setDateTo(isoToInputLocal(dateToIso));

        const params = {
            ...(technology && { technology }),
            ...(dateFromIso && { dateFrom: dateFromIso }),
            ...(dateToIso && { dateTo: dateToIso }),
        };

        fetchMeasurements(params);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    /** Динамички min/max (перцентили) за нормализација */
    const [minVal, maxVal] = useMemo(() => {
        const vals = (measurements || [])
            .map((m) => m.electricFielddbuvPerM)
            .filter((v) => typeof v === "number" && !Number.isNaN(v));
        if (!vals.length) return [40, 170];
        const p5 = percentile(vals, 5);
        const p95 = percentile(vals, 95);
        if (p95 - p5 < 1) return [p5 - 1, p95 + 1]; // edge-case: сите исти
        return [p5, p95];
    }, [measurements]);

    /** Точки за мапа */
    const points = useMemo(() => {
        return (measurements || []).map(getLatLng).filter(Boolean);
    }, [measurements]);

    /** Heatmap data [lat,lng,intensity] нормализирана 0..1 */
    const heatmapData = useMemo(() => {
        return (measurements || [])
            .map((m) => {
                const pos = getLatLng(m);
                if (!pos) return null;
                const v = m.electricFielddbuvPerM;
                if (v == null || Number.isNaN(v)) return [pos[0], pos[1], 0.0];
                const x = Math.max(minVal, Math.min(maxVal, v));
                const intensity = (x - minVal) / (maxVal - minVal);
                return [pos[0], pos[1], intensity];
            })
            .filter(Boolean);
    }, [measurements, minVal, maxVal]);

    /* -------------------------------- Render -------------------------------- */

    return (
        <div
            style={{
                height: "calc(100vh - 120px)",
                width: "100%",
                borderRadius: 16,
                overflow: "hidden",
                boxShadow: "0 14px 40px rgba(0,0,0,0.14)",
                background: "linear-gradient(180deg, #fafafa, #ffffff)",
                border: "1px solid rgba(0,0,0,.06)",
                position: "relative",
            }}
        >
            <div style={{ padding: "16px 18px 0 18px" }}>
                <h2 style={{ margin: 0 }}>📡 Signal Map</h2>
                <p style={{ marginTop: 6, opacity: 0.9 }}>
                    Interactive view of measured field strength with points and heatmap overlay.
                </p>
            </div>

            {/* ---------------------------------------------------------------------
         Филтер-бар: технологија + од/до + Apply/Reset + Export CSV
         URL-синк: на Apply/Reset ги пишуваме вредностите во query string
         --------------------------------------------------------------------- */}
            <Box sx={{ display: "flex", gap: 1.5, alignItems: "center", p: "8px 18px 10px" }}>
                <Select
                    value={technology}
                    onChange={(e) => setTechnology(e.target.value)}
                    displayEmpty
                    size="small"
                    sx={{ minWidth: 180, background: "#fff" }}
                >
                    <MenuItem value="">All technologies</MenuItem>
                    <MenuItem value="ANALOG_TV">ANALOG_TV</MenuItem>
                    <MenuItem value="DIGITAL_TV">DIGITAL_TV</MenuItem>
                    <MenuItem value="FM">FM</MenuItem>
                    <MenuItem value="DAB">DAB</MenuItem>
                </Select>

                <TextField
                    label="From"
                    type="datetime-local"
                    size="small"
                    value={dateFrom}
                    onChange={(e) => setDateFrom(e.target.value)}
                    sx={{ background: "#fff" }}
                    InputLabelProps={{ shrink: true }}
                />
                <TextField
                    label="To"
                    type="datetime-local"
                    size="small"
                    value={dateTo}
                    onChange={(e) => setDateTo(e.target.value)}
                    sx={{ background: "#fff" }}
                    InputLabelProps={{ shrink: true }}
                />

                {/* APPLY: fetch + запиши во URL (shareable линк) */}
                <Button
                    variant="contained"
                    size="small"
                    onClick={() => {
                        setLoading(true);
                        const params = {
                            ...(technology && { technology }),
                            ...(dateFrom && { dateFrom: toUtcIso(dateFrom, false) }),
                            ...(dateTo && { dateTo: toUtcIso(dateTo, true) }),
                        };

                        // URL sync ↓
                        writeFiltersToUrl({
                            ...(technology && { technology }),
                            ...(dateFrom && { dateFrom: toUtcIso(dateFrom, false) }),
                            ...(dateTo && { dateTo: toUtcIso(dateTo, true) }),
                        });

                        fetchMeasurements(params);
                    }}
                >
                    Apply
                </Button>

                {/* RESET: чистење на состојба + URL + refetch */}
                <Button
                    size="small"
                    onClick={() => {
                        setTechnology("");
                        setDateFrom("");
                        setDateTo("");

                        // URL sync (исчисти параметри)
                        writeFiltersToUrl({ technology: "", dateFrom: "", dateTo: "" });

                        setLoading(true);
                        fetchMeasurements();
                    }}
                >
                    Reset
                </Button>

                {/* EXPORT CSV од тековните резултати */}
                <Button
                    size="small"
                    onClick={() => downloadCsv(measurements)}
                >
                    Export CSV
                </Button>
            </Box>

            <MapContainer
                center={[41.996, 21.431]}
                zoom={11}
                style={{ height: "78%", width: "100%" }}
                zoomControl={true}
                preferCanvas={true}
            >
                {/* Контроли и слоеви */}
                <ScaleControl position="bottomleft" />
                <LegendControl minVal={minVal} maxVal={maxVal} />
                <FitToData points={points} />

                <LayersControl position="topleft">
                    <LayersControl.BaseLayer checked name="Carto — Light (Positron)">
                        <TileLayer
                            url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
                            attribution='&copy; OpenStreetMap contributors &copy; <a href="https://carto.com/">CARTO</a>'
                        />
                    </LayersControl.BaseLayer>

                    <LayersControl.BaseLayer name="Carto — Dark Matter">
                        <TileLayer
                            url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
                            attribution='&copy; OpenStreetMap contributors &copy; <a href="https://carto.com/">CARTO</a>'
                        />
                    </LayersControl.BaseLayer>

                    <LayersControl.BaseLayer name="Esri — World Imagery">
                        <TileLayer
                            url="https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"
                            attribution="Tiles &copy; Esri"
                        />
                    </LayersControl.BaseLayer>

                    {/* HEATMAP слој */}
                    <LayersControl.Overlay checked name="Heatmap">
                        <HeatmapOverlay
                            points={heatmapData}
                            baseRadius={18}
                            blur={20}
                            max={1.0}
                            minOpacity={0.2}
                        />
                    </LayersControl.Overlay>

                    {/* Слој: поединечни маркери */}
                    <LayersControl.Overlay checked name="Signal points">
                        <div>
                            {measurements.map((m) => {
                                const pos = getLatLng(m);
                                if (!pos) return null;

                                const v = m.electricFielddbuvPerM;
                                const color = getGradientColor(v, minVal, maxVal);
                                const radius = getRadius(v, minVal, maxVal);

                                return (
                                    <CircleMarker
                                        key={m.id ?? `${pos[0]}-${pos[1]}-${m.date ?? ""}`}
                                        center={pos}
                                        radius={radius}
                                        pathOptions={{
                                            color,
                                            weight: 1.5,
                                            fillColor: color,
                                            fillOpacity: 0.85,
                                        }}
                                    >
                                        <Tooltip direction="top" offset={[0, -4]} opacity={0.95}>
                                            <div style={{ fontWeight: 600 }}>
                                                {m.testLocation ?? "Unnamed location"}
                                            </div>
                                            <div style={{ fontSize: 12, opacity: 0.9 }}>
                                                {v != null ? `${v} dBµV/m` : "—"}
                                            </div>
                                        </Tooltip>

                                        <Popup>
                                            <div style={{ minWidth: 240, lineHeight: 1.45 }}>
                                                <div style={{ fontWeight: 700, marginBottom: 6 }}>
                                                    {m.testLocation ?? "Unnamed location"}
                                                </div>
                                                <div><b>Tech:</b> {m.technology ?? "—"}</div>
                                                {v != null && <div><b>E-field:</b> {v} dBµV/m</div>}
                                                <div>
                                                    <b>Date:</b>{" "}
                                                    {m.date ? new Date(m.date).toLocaleString() : "—"}
                                                </div>
                                                {m.settlementName && (
                                                    <div><b>Settlement:</b> {m.settlementName}</div>
                                                )}
                                                {typeof m.channelNumber === "number" && (
                                                    <div><b>Channel:</b> {m.channelNumber}</div>
                                                )}
                                                {typeof m.frequencyMHz === "number" && (
                                                    <div><b>Frequency:</b> {m.frequencyMHz} MHz</div>
                                                )}
                                            </div>
                                        </Popup>
                                    </CircleMarker>
                                );
                            })}
                        </div>
                    </LayersControl.Overlay>
                </LayersControl>
            </MapContainer>

            {loading && (
                <div
                    style={{
                        position: "absolute",
                        inset: 0,
                        display: "grid",
                        placeItems: "center",
                        background: "rgba(255,255,255,0.6)",
                        backdropFilter: "blur(2px)",
                        fontWeight: 600,
                    }}
                >
                    Loading measurements…
                </div>
            )}
        </div>
    );
};

export default MapPage;
