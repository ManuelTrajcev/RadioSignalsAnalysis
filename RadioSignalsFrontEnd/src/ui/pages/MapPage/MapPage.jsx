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
    Pane,
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

const readFiltersFromUrl = () => {
    const p = new URLSearchParams(window.location.search);
    return {
        technology: p.get("technology") || "",
        dateFromIso: p.get("dateFrom") || "",
        dateToIso: p.get("dateTo") || "",
    };
};

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

const csvEscape = (v) => {
    if (v == null) return "";
    const s = String(v);
    return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
};

/** Преземи CSV од тековните measurements */
const downloadCsv = (rows) => {
    try {
        if (!rows?.length) return;

        const header = [
            "id",
            "date",
            "technology",
            "electricFieldDbuvPerM",
            "channelNumber",
            "frequencyMHz",
            "latitude",
            "longitude",
            "testLocation",
            "settlementName",
            "municipalityName",
            "electricFieldStrength_dBuV_per_m",
        ];

        const body = rows
            .map((m) => {
                const pos = getLatLng(m) || [null, null];
                const lat = typeof pos[0] === "number" ? pos[0] : "";
                const lng = typeof pos[1] === "number" ? pos[1] : "";
                return [
                    m.id,
                    m.date,
                    m.technology,
                    m.electricFieldDbuvPerM,
                    m.channelNumber,
                    m.frequencyMHz,
                    lat,
                    lng,
                    m.testLocation,
                    m.settlementName,
                    m.municipalityName,
                    m.electricFieldDbuvPerM,
                ]
                    .map(csvEscape)
                    .join(",");
            })
            .join("\n");

        const csv = "\uFEFF" + header.join(",") + "\n" + body; // BOM за Excel
        const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = "measurements.csv";
        a.style.display = "none";
        document.body.appendChild(a);
        a.click();
        setTimeout(() => {
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }, 250);
    } catch (e) {
        console.error("CSV export failed:", e);
    }
};

/* ----------  Бои/радиус (со нормализација на сетот)  ---------- */

function lerp(a, b, t) { return a + (b - a) * t; }
function lerpColor(c1, c2, t) {
    const a = parseInt(c1.slice(1), 16);
    const b = parseInt(c2.slice(1), 16);
    const r = Math.round(lerp((a >> 16) & 255, (b >> 16) & 255, t));
    const g = Math.round(lerp((a >> 8) & 255, (b >> 8) & 255, t));
    const bch = Math.round(lerp(a & 255, b & 255, t));
    return `#${((1 << 24) + (r << 16) + (g << 8) + bch).toString(16).slice(1)}`;
}
const stops = ["#2e7d32", "#fbc02d", "#f57c00", "#c62828"];

const percentile = (arr, p) => {
    if (!arr.length) return undefined;
    const a = [...arr].sort((x, y) => x - y);
    const idx = Math.min(a.length - 1, Math.max(0, Math.round((p / 100) * (a.length - 1))));
    return a[idx];
};

function getGradientColor(v, min, max) {
    if (v == null || Number.isNaN(v)) return "#1976d2";
    const lo = min ?? 40, hi = Math.max(lo + 1, max ?? 170);
    const x = Math.max(lo, Math.min(hi, v));
    const t = (x - lo) / (hi - lo);
    if (t <= 1 / 3) return lerpColor(stops[0], stops[1], t * 3);
    if (t <= 2 / 3) return lerpColor(stops[1], stops[2], (t - 1 / 3) * 3);
    return lerpColor(stops[2], stops[3], (t - 2 / 3) * 3);
}
const getRadius = (v, min, max) => {
    if (v == null) return 6;
    const lo = min ?? 40, hi = Math.max(lo + 1, max ?? 170);
    const x = Math.max(lo, Math.min(hi, v));
    return 4 + ((x - lo) / (hi - lo)) * 8;
};

/* =========================================================================
   Leaflet помошни компоненти
   ========================================================================= */

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

/** Heatmap со радиус што се прилагодува на zoom (во засебен pane со pointer-events: none) */
function HeatmapOverlay({
                            points,
                            baseRadius = 18,
                            blur = 20,
                            max = 1.0,
                            minOpacity = 0.2,
                            gradient,
                            pane = "heatmap",
                        }) {
    const map = useMap();
    const [radius, setRadius] = useState(baseRadius);

    useEffect(() => {
        if (!map) return;
        const update = () => {
            const z = map.getZoom();
            const r = Math.max(10, Math.min(40, Math.round(z * 2)));
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
            pane,
        });
        layer.addTo(map);
        return () => layer.remove();
    }, [map, points, radius, blur, max, minOpacity, gradient, pane]);

    return null;
}

/* =========================================================================
   Главна компонента
   ========================================================================= */

const MapPage = () => {
    const [measurements, setMeasurements] = useState([]);
    const [loading, setLoading] = useState(true);

    const [technology, setTechnology] = useState("");
    const [dateFrom, setDateFrom] = useState("");
    const [dateTo, setDateTo] = useState("");

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

    const [minVal, maxVal] = useMemo(() => {
        const vals = (measurements || [])
            .map((m) => m.electricFieldDbuvPerM)
            .filter((v) => typeof v === "number" && !Number.isNaN(v));
        if (!vals.length) return [40, 170];
        const p5 = percentile(vals, 5);
        const p95 = percentile(vals, 95);
        if (p95 - p5 < 1) return [p5 - 1, p95 + 1];
        return [p5, p95];
    }, [measurements]);

    const points = useMemo(
        () => (measurements || []).map(getLatLng).filter(Boolean),
        [measurements]
    );

    const heatmapData = useMemo(() => {
        return (measurements || [])
            .map((m) => {
                const pos = getLatLng(m);
                if (!pos) return null;
                const v = m.electricFieldDbuvPerM;
                if (v == null || Number.isNaN(v)) return [pos[0], pos[1], 0.0];
                const x = Math.max(minVal, Math.min(maxVal, v));
                const intensity = (x - minVal) / (maxVal - minVal);
                return [pos[0], pos[1], intensity];
            })
            .filter(Boolean);
    }, [measurements, minVal, maxVal]);

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
                <h2 style={{ margin: 0 }}>📡 Мапа на сигнали</h2>
                <p style={{ marginTop: 6, opacity: 0.9 }}>
                    Интерактивен приказ на измерената јачина на полето со точки и heatmap.
                </p>
            </div>

            {/* Филтри + Export */}
            <Box sx={{ display: "flex", gap: 1.5, alignItems: "center", p: "8px 18px 10px" }}>
                <Select
                    value={technology}
                    onChange={(e) => setTechnology(e.target.value)}
                    displayEmpty
                    size="small"
                    sx={{ minWidth: 180, background: "#fff" }}
                >
                    <MenuItem value="">Сите технологии</MenuItem>
                    <MenuItem value="ANALOG_TV">АНАЛОГНА ТВ</MenuItem>
                    <MenuItem value="DIGITAL_TV">ДИГИТАЛНА ТВ</MenuItem>
                    <MenuItem value="FM">FM</MenuItem>
                    <MenuItem value="DAB">DAB</MenuItem>
                </Select>

                <TextField
                    label="Од"
                    type="datetime-local"
                    size="small"
                    value={dateFrom}
                    onChange={(e) => setDateFrom(e.target.value)}
                    sx={{ background: "#fff" }}
                    InputLabelProps={{ shrink: true }}
                />
                <TextField
                    label="До"
                    type="datetime-local"
                    size="small"
                    value={dateTo}
                    onChange={(e) => setDateTo(e.target.value)}
                    sx={{ background: "#fff" }}
                    InputLabelProps={{ shrink: true }}
                />

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
                        writeFiltersToUrl(params);
                        fetchMeasurements(params);
                    }}
                >
                    Примени
                </Button>

                <Button
                    size="small"
                    onClick={() => {
                        setTechnology("");
                        setDateFrom("");
                        setDateTo("");
                        writeFiltersToUrl({ technology: "", dateFrom: "", dateTo: "" });
                        setLoading(true);
                        fetchMeasurements();
                    }}
                >
                    Ресетирај
                </Button>

                <Button size="small" onClick={() => downloadCsv(measurements)}>
                    Експортирај CSV
                </Button>
            </Box>

            <MapContainer
                center={[41.996, 21.431]}
                zoom={11}
                style={{ height: "78%", width: "100%" }}
                zoomControl={true}
                preferCanvas={true}
            >
                <Pane name="heatmap" style={{ zIndex: 400, pointerEvents: "none" }} />
                <Pane name="markers" style={{ zIndex: 650 }} />

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

                    {/* HEATMAP слој (во pane="heatmap") */}
                    <LayersControl.Overlay checked name="Heatmap">
                        <HeatmapOverlay
                            points={heatmapData}
                            baseRadius={18}
                            blur={20}
                            max={1.0}
                            minOpacity={0.2}
                            pane="heatmap"
                        />
                    </LayersControl.Overlay>

                    {/* Слој: поединечни маркери (во pane="markers") */}
                    <LayersControl.Overlay checked name="Signal points">
                        <div>
                            {measurements.map((m) => {
                                const pos = getLatLng(m);
                                if (!pos) return null;

                                const v = m.electricFieldDbuvPerM;
                                const color = getGradientColor(v, minVal, maxVal);
                                const radius = getRadius(v, minVal, maxVal);

                                return (
                                    <CircleMarker
                                        key={m.id ?? `${pos[0]}-${pos[1]}-${m.date ?? ""}`}
                                        center={pos}
                                        radius={radius}
                                        pane="markers"
                                        pathOptions={{
                                            color,
                                            weight: 1.5,
                                            fillColor: color,
                                            fillOpacity: 0.85,
                                        }}
                                    >
                                        <Tooltip direction="top" offset={[0, -4]} opacity={0.95} sticky>
                                            <div style={{ fontWeight: 600 }}>
                                                {m.testLocation ?? "Непозната локација"}
                                            </div>
                                            <div style={{ fontSize: 12, opacity: 0.9 }}>
                                                {v != null
                                                    ? `Јачина на електрично поле: ${v} dBµV/m`
                                                    : "Јачина на електрично поле: —"}
                                            </div>
                                        </Tooltip>

                                        <Popup>
                                            <div style={{ minWidth: 240, lineHeight: 1.45 }}>
                                                <div style={{ fontWeight: 700, marginBottom: 6 }}>
                                                    {m.testLocation ?? "Непозната локација"}
                                                </div>
                                                <div><b>Технологија:</b> {m.technology ?? "—"}</div>
                                                {v != null && <div><b>Електрично поле:</b> {v} dBµV/m</div>}
                                                <div>
                                                    <b>Датум:</b>{" "}
                                                    {m.date ? new Date(m.date).toLocaleString() : "—"}
                                                </div>
                                                {m.settlementName && (
                                                    <div><b>Населено место:</b> {m.settlementName}</div>
                                                )}
                                                {typeof m.channelNumber === "number" && (
                                                    <div><b>Канал:</b> {m.channelNumber}</div>
                                                )}
                                                {typeof m.frequencyMHz === "number" && (
                                                    <div><b>Фреквенција:</b> {m.frequencyMHz} MHz</div>
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
                    Се вчитуваат мерењата…
                </div>
            )}
        </div>
    );
};

export default MapPage;
