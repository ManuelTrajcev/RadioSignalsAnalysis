#!/usr/bin/env python3
import json
import joblib
import pandas as pd

def predict_one(model_path: str, sample: dict) -> float:
    pipe = joblib.load(model_path)
    X = pd.DataFrame([sample])
    yhat = pipe.predict(X)[0]
    return float(yhat)

if __name__ == "__main__":
    sample_digital = {
        "latitude": 41.986,
        "longitude": 21.431,
        "elevation_m": 400,
        "year": 2015,
        "month": 5,
        "population": 2000,
        "households": 600,
        "tv_channel": 26,
        "municipality": "Скопје",
        "settlement": "Буковиќ",
        "program_id": "МТВ 1",
        "emitter": "р.Водно",
        "fm_freq_mhz": 0.0
    }
    print("Pred:", predict_one("artifacts/best_digital_model.joblib", sample_digital))
