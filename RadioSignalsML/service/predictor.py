from __future__ import annotations

from typing import Any, Dict

from .config import ServiceConfig
from .feature_mapping import FeatureMappingError, FeatureVector, build_feature_vector
from .location_lookup import load_location_lookup
from .model_store import ModelStore


class SignalPredictorService:
    """Convenience façade that converts payloads and invokes the trained pipelines."""

    def __init__(self, config: ServiceConfig):
        self._config = config
        self._lookup = load_location_lookup(config.location_lookup_path)
        self._models = ModelStore(config.digital_model_path, config.fm_model_path)

    def predict(self, payload: Dict[str, Any]) -> Dict[str, Any]:
        vector = build_feature_vector(payload, self._lookup)
        value, version = self._models.predict(vector)
        return {
            "technology": vector.technology,
            "field_dbuv_m": value,
            "features": vector.features,
            "model_version": version,
        }


__all__ = [
    "SignalPredictorService",
    "ServiceConfig",
    "FeatureMappingError",
]
