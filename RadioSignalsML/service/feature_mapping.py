from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime
from typing import Any, Dict, Mapping, Optional

import math


@dataclass(frozen=True)
class FeatureVector:
    """Represents the single-row feature payload expected by the trained pipeline."""

    technology: str
    features: Dict[str, Any]


class FeatureMappingError(ValueError):
    """Raised when incoming payload cannot be converted into model features."""


def _normalize_registry_number(value: Any) -> Optional[str]:
    if value is None:
        return None
    if isinstance(value, (int, float)) and not math.isnan(value):
        return str(int(value))
    text = str(value).strip()
    if not text:
        return None
    if text.endswith(".0"):
        text = text[:-2]
    return text


def _canonical_names(payload: Mapping[str, Any], lookup: Mapping[str, Mapping[str, Optional[str]]]) -> tuple[str, str]:
    """Resolve location names using place registry to stay aligned with training vocabulary."""
    registry = _normalize_registry_number(
        payload.get("registry_number")
        or payload.get("settlement_registry_number")
        or payload.get("registryNumber")
    )
    if registry and registry in lookup:
        entry = lookup[registry]
        muni = entry.get("municipality") or payload.get("municipality") or ""
        sett = entry.get("settlement") or payload.get("settlement") or ""
        return muni, sett

    muni = str(payload.get("municipality") or "").strip()
    sett = str(payload.get("settlement") or payload.get("settlement_name") or "").strip()
    if not muni or not sett:
        raise FeatureMappingError("Missing settlement or municipality information.")
    return muni, sett


def _coerce_float(value: Any, default: float | None = None) -> float:
    if value is None:
        if default is None:
            raise FeatureMappingError("Required numeric value missing.")
        return float(default)
    try:
        if isinstance(value, str):
            v = value.replace(',', '.').strip()
            if not v:
                raise ValueError
            return float(v)
        return float(value)
    except Exception as exc:
        if default is not None:
            return float(default)
        raise FeatureMappingError(f"Invalid numeric value: {value}") from exc


def _safe_text(value: Any, fallback: str) -> str:
    text = str(value).strip() if value is not None else ""
    return text or fallback


def build_feature_vector(payload: Mapping[str, Any], lookup: Mapping[str, Mapping[str, Optional[str]]]) -> FeatureVector:
    """Transforms raw prediction request into the schema the joblib pipeline expects."""

    tech_raw = str(payload.get("technology") or payload.get("tech") or "").strip().lower()
    if tech_raw in {"digital_tv", "digital"}:
        technology = "digital"
    elif tech_raw in {"fm", "analogue_fm"}:
        technology = "fm"
    else:
        raise FeatureMappingError("Technology must be DIGITAL_TV or FM.")

    try:
        measurement_date = payload.get("date") or payload.get("measurement_date")
        if isinstance(measurement_date, (int, float)):
            # epoch milliseconds from JS Date
            measurement_date = datetime.utcfromtimestamp(float(measurement_date) / 1000.0)
        elif isinstance(measurement_date, str):
            measurement_date = datetime.fromisoformat(measurement_date.replace("Z", "+00:00"))
        elif isinstance(measurement_date, datetime):
            pass
        else:
            raise FeatureMappingError("date field is required")
    except Exception as exc:
        raise FeatureMappingError("Unable to parse prediction date.") from exc

    year = measurement_date.year
    month = measurement_date.month

    muni, sett = _canonical_names(payload, lookup)

    lat = _coerce_float(payload.get("latitude") or payload.get("latitude_decimal") or payload.get("lat"))
    lon = _coerce_float(payload.get("longitude") or payload.get("longitude_decimal") or payload.get("lon"))
    elev = _coerce_float(payload.get("elevation_m") or payload.get("altitude_meters") or payload.get("altitude"))

    population = _coerce_float(payload.get("population"), default=0)
    households = _coerce_float(payload.get("households"), default=0)

    program_id = _safe_text(payload.get("program_identifier") or payload.get("programId"), "UNKNOWN")
    emitter = _safe_text(payload.get("transmitter_location") or payload.get("emitter"), "UNKNOWN")

    base = {
        "latitude": lat,
        "longitude": lon,
        "elevation_m": elev,
        "year": year,
        "month": month,
        "population": population,
        "households": households,
        "municipality": muni,
        "settlement": sett,
        "program_id": program_id,
        "emitter": emitter,
    }

    if technology == "digital":
        base["tv_channel"] = _coerce_float(payload.get("tv_channel") or payload.get("channel_number"))
    else:
        base["fm_freq_mhz"] = _coerce_float(payload.get("fm_freq_mhz") or payload.get("frequency_mhz"))

    return FeatureVector(technology=technology, features=base)
