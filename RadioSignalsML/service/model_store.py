from __future__ import annotations

from pathlib import Path
from typing import Dict, Tuple

import joblib
import pandas as pd
import warnings
from sklearn.exceptions import InconsistentVersionWarning

from .feature_mapping import FeatureVector

# Ignore sklearn persistence warnings when minor patch versions differ.
warnings.filterwarnings("ignore", category=InconsistentVersionWarning)


class ModelStore:
    """Loads and caches the trained scikit-learn pipelines for prediction."""

    def __init__(self, digital_path: Path, fm_path: Path):
        self._paths: Dict[str, Path] = {
            "digital": digital_path,
            "fm": fm_path,
        }
        self._models: Dict[str, any] = {}
        self._versions: Dict[str, str] = {}

    def _load(self, technology: str):
        if technology not in self._paths:
            raise KeyError(f"Unsupported technology: {technology}")
        if technology not in self._models:
            model_path = self._paths[technology]
            if not model_path.exists():
                raise FileNotFoundError(f"Model artifact missing: {model_path}")
            self._models[technology] = joblib.load(model_path)
            self._versions[technology] = model_path.stem
        return self._models[technology]

    def predict(self, vector: FeatureVector) -> Tuple[float, str]:
        model = self._load(vector.technology)
        row = dict(vector.features)
        expected = getattr(model, "feature_names_in_", None)
        if expected is not None:
            for col in expected:
                if col not in row:
                    row[col] = 0
            payload = pd.DataFrame([[row[col] for col in expected]], columns=list(expected))
        else:
            payload = pd.DataFrame([row])
        prediction = model.predict(payload)[0]
        return float(prediction), self._versions[vector.technology]

    def get_version(self, technology: str) -> str:
        self._load(technology)
        return self._versions.get(technology, "unknown")
