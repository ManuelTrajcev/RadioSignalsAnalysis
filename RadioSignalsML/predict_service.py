"""
FastAPI inference service for RadioSignals models.

Data contract (v1): expects already-derived features matching the
training pipeline semantics produced in train_signal_models.py:

- technology: "DIGITAL_TV" or "FM" (case-insensitive). Also accepts
  shorthand "digital" | "fm".
- numeric: latitude, longitude, elevation_m, year, month,
  population, households, tv_channel (digital), fm_freq_mhz (fm)
- categorical: municipality, settlement, program_id, emitter

Notes
- Keeps feature names identical to training. Any Excel-specific
  parsing is reserved for training only; this service only consumes
  app data.
- Column order and preprocessing semantics are loaded from metrics_*.json
  saved during training to ensure training/inference parity.
- Configuration is environment-driven:
  * MODEL_DIR: directory with joblib + metrics JSON (default: artifacts)
  * PORT: service port (default: 8000)

Minimal run: `MODEL_DIR=artifacts uvicorn predict_service:app --host 0.0.0.0 --port 8000`
"""

from __future__ import annotations

import os
import json
from pathlib import Path
from typing import Optional, Dict, Any

import joblib
import pandas as pd
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field, validator


# ----------------------------
# Configuration and model load
# ----------------------------
MODEL_DIR = Path(os.getenv("MODEL_DIR", "artifacts")).resolve()

_DIGITAL_MODEL_PATH = MODEL_DIR / "best_digital_model.joblib"
_FM_MODEL_PATH = MODEL_DIR / "best_fm_model.joblib"
_DIGITAL_METRICS_PATH = MODEL_DIR / "metrics_digital.json"
_FM_METRICS_PATH = MODEL_DIR / "metrics_fm.json"

if not _DIGITAL_MODEL_PATH.exists() or not _FM_MODEL_PATH.exists():
    raise RuntimeError(
        f"Model artifacts not found under {MODEL_DIR}. "
        f"Expected: {_DIGITAL_MODEL_PATH.name}, {_FM_MODEL_PATH.name}"
    )

def _load_metrics(path: Path) -> Dict[str, Any]:
    try:
        with open(path, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception as e:
        raise RuntimeError(f"Failed to load metrics file {path}: {e}")


_digital_metrics = _load_metrics(_DIGITAL_METRICS_PATH)
_fm_metrics = _load_metrics(_FM_METRICS_PATH)

_digital_features = {
    "numeric": _digital_metrics["features"]["numeric"],
    "categorical": _digital_metrics["features"]["categorical"],
}
_fm_features = {
    "numeric": _fm_metrics["features"]["numeric"],
    "categorical": _fm_metrics["features"]["categorical"],
}

_digital_pipe = joblib.load(_DIGITAL_MODEL_PATH)
_fm_pipe = joblib.load(_FM_MODEL_PATH)


# ----------------------------
# Request/Response models
# ----------------------------
class PredictRequest(BaseModel):
    # Technology and basic geography
    technology: str = Field(..., description='"DIGITAL_TV" or "FM"; also accepts "digital" | "fm"')
    municipality: str = Field(..., description="Municipality name (string)")
    settlement: str = Field(..., description="Settlement name (string)")

    # Program and emitter naming (categorical)
    program_id: Optional[str] = Field(None, description="Program identifier (string, nullable)")
    emitter: Optional[str] = Field(None, description="Transmitter location / emitter (string, nullable)")

    # Coordinates (decimal) and environment
    latitude: float = Field(..., description="Latitude in decimal degrees")
    longitude: float = Field(..., description="Longitude in decimal degrees")
    elevation_m: float = Field(..., description="Altitude in meters")

    # Date-derived
    year: int = Field(..., description="Measurement year")
    month: int = Field(..., description="Measurement month (1-12)")

    # Demography (nullable; imputed by model if missing)
    population: Optional[float] = Field(None, description="Settlement population")
    households: Optional[float] = Field(None, description="Settlement households")

    # Tech-specific numeric
    tv_channel: Optional[int] = Field(None, description="TV channel number (DIGITAL_TV)")
    fm_freq_mhz: Optional[float] = Field(None, description="FM frequency in MHz (FM)")

    @validator("technology")
    def _normalize_tech(cls, v: str) -> str:
        t = v.strip().lower()
        if t in ("digital_tv", "digital", "dtv", "dvb"):
            return "digital"
        if t in ("fm",):
            return "fm"
        raise ValueError("technology must be DIGITAL_TV or FM")

    @validator("month")
    def _valid_month(cls, v: int) -> int:
        if not (1 <= v <= 12):
            raise ValueError("month must be 1..12")
        return v


class PredictResponse(BaseModel):
    field_dbuv_m: float
    tech: str
    used_model: str


# ----------------------------
# FastAPI app
# ----------------------------
app = FastAPI(title="RadioSignals ML Predict API", version="1.0.0")


@app.get("/health")
def health() -> Dict[str, str]:
    return {
        "status": "ok",
        "model_dir": str(MODEL_DIR),
    }


def _build_row(req: PredictRequest) -> pd.DataFrame:
    # Determine feature schema for chosen tech
    feats = _digital_features if req.technology == "digital" else _fm_features
    cols = feats["numeric"] + feats["categorical"]

    # Map input to training feature names. Keys not applicable for the chosen
    # tech (e.g., tv_channel for FM) are set to None so imputers can handle them.
    values: Dict[str, Any] = {
        # Numeric (shared)
        "latitude": req.latitude,
        "longitude": req.longitude,
        "elevation_m": req.elevation_m,
        "year": req.year,
        "month": req.month,
        "population": req.population,
        "households": req.households,
        # Tech-specific numeric
        "tv_channel": req.tv_channel if req.technology == "digital" else None,
        "fm_freq_mhz": req.fm_freq_mhz if req.technology == "fm" else None,
        # Categorical
        "municipality": req.municipality,
        "settlement": req.settlement,
        "program_id": req.program_id,
        "emitter": req.emitter,
    }

    # Ensure all expected columns are present in the right order
    row = {c: values.get(c, None) for c in cols}
    return pd.DataFrame([row])


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest) -> PredictResponse:
    try:
        X = _build_row(req)
        if req.technology == "digital":
            yhat = float(_digital_pipe.predict(X)[0])
            model_name = _digital_metrics.get("winner", "unknown")
        else:
            yhat = float(_fm_pipe.predict(X)[0])
            model_name = _fm_metrics.get("winner", "unknown")
        return PredictResponse(field_dbuv_m=yhat, tech=req.technology, used_model=model_name)
    except Exception as e:
        # Keep error generic to callers; details are often noisy and not actionable
        raise HTTPException(status_code=400, detail=f"Prediction failed: {type(e).__name__}: {e}")


if __name__ == "__main__":
    import uvicorn

    port = int(os.getenv("PORT", "8000"))
    uvicorn.run("predict_service:app", host="0.0.0.0", port=port, reload=False)

