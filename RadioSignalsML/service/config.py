from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path


@dataclass
class ServiceConfig:
    # DIGITAL_MODEL_PATH / FM_MODEL_PATH / LOCATION_LOOKUP_PATH allow overriding default artifact locations.
    """Holds runtime configuration for the prediction service."""

    digital_model_path: Path
    fm_model_path: Path
    location_lookup_path: Path
    host: str = "0.0.0.0"
    port: int = 8000

    @classmethod
    def from_env(cls) -> "ServiceConfig":
        base_dir = Path(os.getenv("RADIO_SIGNALS_ML_ROOT", Path(__file__).resolve().parent.parent))
        digital = Path(os.getenv("DIGITAL_MODEL_PATH", base_dir / "artifacts" / "best_digital_model.joblib"))
        fm = Path(os.getenv("FM_MODEL_PATH", base_dir / "artifacts" / "best_fm_model.joblib"))
        lookup = Path(os.getenv("LOCATION_LOOKUP_PATH", base_dir / "artifacts" / "location_lookup.json"))
        host = os.getenv("PREDICT_HOST", "0.0.0.0")
        port = int(os.getenv("PREDICT_PORT", "8000"))
        return cls(digital_model_path=digital, fm_model_path=fm, location_lookup_path=lookup, host=host, port=port)
