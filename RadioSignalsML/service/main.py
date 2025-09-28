from __future__ import annotations

from fastapi import FastAPI, HTTPException

from .config import ServiceConfig
from .feature_mapping import FeatureMappingError
from .predictor import SignalPredictorService
from .schemas import PredictionInput, PredictionResponse


def build_app() -> FastAPI:
    config = ServiceConfig.from_env()
    predictor = SignalPredictorService(config)

    app = FastAPI(
        title="Radio Signals Prediction API",
        version="1.0.0",
        summary="Inference endpoint for trained electric field models",
    )

    @app.get("/health")
    def health() -> dict[str, str]:
        return {"status": "ok"}

    @app.post("/predict", response_model=PredictionResponse)
    def predict(req: PredictionInput) -> PredictionResponse:
        try:
            payload = predictor.predict(req.dict())
            print(payload)
            return PredictionResponse(**payload)
        except FeatureMappingError as exc:
            raise HTTPException(status_code=400, detail=str(exc))

    return app


app = build_app()


if __name__ == "__main__":
    import uvicorn

    cfg = ServiceConfig.from_env()
    # uvicorn reads host/port and reload via env; config documented inline for ops
    uvicorn.run("service.main:app", host=cfg.host, port=cfg.port, reload=False)
