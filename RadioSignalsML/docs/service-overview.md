# RadioSignalsML — FastAPI Service Overview

## Purpose

This microservice exposes two HTTP endpoints that wrap two trained scikit-learn
pipelines (scikit-learn `Pipeline` objects serialised with `joblib`).  Given a
radio measurement site and metadata, it returns a predicted electric field
strength value in **dBµV/m**.

The service is the inference half of the RadioSignals system.  Training is done
offline in `train_signal_models.py`; the resulting `.joblib` artefacts are
committed to `artifacts/` and mounted at runtime.

---

## Directory Layout (RadioSignalsML only)

```
RadioSignalsML/
├── artifacts/
│   ├── best_digital_model.joblib   # trained DIGITAL_TV pipeline
│   ├── best_fm_model.joblib        # trained FM pipeline
│   └── location_lookup.json        # registry_number → {municipality, settlement}
├── service/
│   ├── __init__.py
│   ├── config.py           # ServiceConfig dataclass, reads env vars
│   ├── feature_mapping.py  # raw payload → FeatureVector
│   ├── location_lookup.py  # loads location_lookup.json
│   ├── model_store.py      # lazy-loads joblib pipelines, calls .predict()
│   ├── predictor.py        # facade: FeatureVector → prediction dict
│   ├── schemas.py          # Pydantic request/response models
│   └── main.py             # FastAPI app factory + uvicorn entry point
├── requirements.txt
└── ...                     # training scripts, data (not needed at runtime)
```

---

## Modules

### `service/config.py` — `ServiceConfig`

A frozen `@dataclass` populated from environment variables via
`ServiceConfig.from_env()`.

| Env var | Default | Description |
|---|---|---|
| `RADIO_SIGNALS_ML_ROOT` | directory two levels above `config.py` | Base path used to resolve artifact paths |
| `DIGITAL_MODEL_PATH` | `$ROOT/artifacts/best_digital_model.joblib` | Path to the Digital TV joblib pipeline |
| `FM_MODEL_PATH` | `$ROOT/artifacts/best_fm_model.joblib` | Path to the FM joblib pipeline |
| `LOCATION_LOOKUP_PATH` | `$ROOT/artifacts/location_lookup.json` | Path to the location registry JSON |
| `PREDICT_HOST` | `0.0.0.0` | Host uvicorn binds to |
| `PREDICT_PORT` | `8000` | Port uvicorn listens on |

---

### `service/location_lookup.py` — `load_location_lookup`

Reads `location_lookup.json`, a flat JSON object keyed by `registry_number`
(string-coerced).  Each value is `{"municipality": "...", "settlement": "..."}`.

Returns an empty dict if the file is missing (graceful degradation — the feature
mapper will then require explicit municipality/settlement fields in the payload).

---

### `service/feature_mapping.py` — `build_feature_vector`

Converts the raw prediction payload (a `dict`) into a `FeatureVector` dataclass
that exactly matches the column schema the trained pipelines expect.

Key responsibilities:

- Normalises `technology` (`"DIGITAL_TV"` / `"FM"` → internal `"digital"` / `"fm"`)
- Parses `date` from ISO-8601 string, epoch-milliseconds integer, or Python `datetime`; extracts `year` and `month` features
- Resolves `municipality` and `settlement` via the location registry (falls back to payload fields)
- Coerces all numeric fields, with sensible defaults for optional ones
- Appends the technology-specific feature: `tv_channel` for DIGITAL_TV, `fm_freq_mhz` for FM
- Raises `FeatureMappingError` (HTTP 400) for unresolvable payloads

---

### `service/model_store.py` — `ModelStore`

Lazy-loads and caches the two scikit-learn pipelines on first use.

- Reads `feature_names_in_` from the pipeline (set by scikit-learn ≥ 1.0) and
  fills any missing columns with `0` to stay robust against minor feature set
  differences between pipeline versions.
- Returns `(float prediction, str version)` where `version` is the stem of the
  `.joblib` filename (e.g. `"best_digital_model"`).
- Suppresses `InconsistentVersionWarning` so minor scikit-learn patch upgrades
  don't produce noise in logs.

---

### `service/predictor.py` — `SignalPredictorService`

Thin facade that wires `build_feature_vector → ModelStore.predict` and shapes
the result into a plain dict consumed by the endpoint handler.

---

### `service/schemas.py` — Pydantic models

**`PredictionInput`** (request body):

| Field | Type | Required | Notes |
|---|---|---|---|
| `technology` | `"DIGITAL_TV"` \| `"FM"` | yes | |
| `date` | `datetime` | yes | ISO-8601 |
| `latitude` | `float` | yes | WGS84 |
| `longitude` | `float` | yes | WGS84 |
| `elevation_m` | `float` | yes | metres above sea level |
| `population` | `float` | no | defaults to 0 |
| `households` | `float` | no | defaults to 0 |
| `registry_number` | `str` | no | maps to training `place_id` |
| `municipality` | `str` | no | fallback when registry missing |
| `settlement` | `str` | no | fallback when registry missing |
| `program_identifier` | `str` | no | ≤ 255 chars |
| `transmitter_location` | `str` | no | ≤ 255 chars |
| `channel_number` | `int` | if DIGITAL_TV | validated via `@validator` |
| `frequency_mhz` | `float` | if FM | validated via `@validator` |

**`PredictionResponse`** (response body):

| Field | Type | Description |
|---|---|---|
| `technology` | `"digital"` \| `"fm"` | normalised technology key |
| `field_dbuv_m` | `float` | predicted electric field strength (dBµV/m) |
| `features` | `dict` | feature vector sent to the model (useful for debugging) |
| `model_version` | `str` | stem of the loaded `.joblib` file |

---

### `service/main.py` — FastAPI application factory

`build_app()`:
1. Creates `ServiceConfig` from environment.
2. Instantiates `SignalPredictorService` (loads location lookup; pipelines are
   lazy-loaded on first `/predict` call).
3. Registers two routes:

| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Liveness check — returns `{"status": "ok"}` |
| `POST` | `/predict` | Main inference endpoint |

The module-level `app = build_app()` allows uvicorn to import the app object
via the string `"service.main:app"`.

---

## Runtime Dependencies

From `requirements.txt`:

| Package | Role |
|---|---|
| `numpy` | numerical foundation for scikit-learn |
| `pandas` | builds the single-row `DataFrame` passed to `.predict()` |
| `scikit-learn` | runs the trained pipelines |
| `joblib` | deserialises `.joblib` artefacts |
| `openpyxl` | Excel support (used during training; not required at inference, but included) |
| `fastapi` | HTTP framework |
| `uvicorn[standard]` | ASGI server (includes `uvloop` + `httptools` for performance) |
| `pydantic` | request/response validation |


---

## Known Limitations / Notes

- Models are loaded lazily on the first prediction request.  Cold-start latency
  for the first request to each technology will be higher.
- `signal_data.xlsx` and `train_signal_models.py` are training artefacts and
  are **not** needed at inference time.
- The service has no authentication.  It is expected to sit behind the .NET
  backend or an API gateway, not exposed directly to the public internet.
