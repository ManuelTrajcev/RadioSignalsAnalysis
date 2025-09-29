from __future__ import annotations

from datetime import datetime
from typing import Literal, Optional

from pydantic import BaseModel, Field, validator


class PredictionInput(BaseModel):
    """Validated payload accepted by /predict. Mirrors backend DTO semantics."""

    technology: Literal["DIGITAL_TV", "FM"] = Field(..., description="Signal family to predict (training uses DIGITAL_TV or FM)")
    date: datetime = Field(..., description="Measurement date used to derive year/month features")
    latitude: float = Field(..., description="Latitude in decimal degrees (WGS84)")
    longitude: float = Field(..., description="Longitude in decimal degrees (WGS84)")
    elevation_m: float = Field(..., description="Site altitude in meters above sea level")
    population: float = Field(0, description="Latest known settlement population")
    households: float = Field(0, description="Latest known settlement households count")
    registry_number: Optional[str] = Field(None, description="Settlement registry identifier; maps to training place_id")
    municipality: Optional[str] = Field(None, description="Fallback municipality name when registry lookup missing")
    settlement: Optional[str] = Field(None, description="Fallback settlement name when registry lookup missing")
    program_identifier: Optional[str] = Field(None, max_length=255)
    transmitter_location: Optional[str] = Field(None, max_length=255)
    channel_number: Optional[int] = Field(None, description="Required when technology=DIGITAL_TV")
    frequency_mhz: Optional[float] = Field(None, description="Required when technology=FM")

    @validator("channel_number", always=True)
    def validate_channel(cls, value, values):
        if values.get("technology") == "DIGITAL_TV" and value is None:
            raise ValueError("channel_number is required for DIGITAL_TV predictions")
        return value

    @validator("frequency_mhz", always=True)
    def validate_frequency(cls, value, values):
        if values.get("technology") == "FM" and value is None:
            raise ValueError("frequency_mhz is required for FM predictions")
        return value


class PredictionResponse(BaseModel):
    """Service response containing prediction in dBµV/m."""

    technology: Literal["digital", "fm"]
    field_dbuv_m: float = Field(..., description="Predicted electric field strength")
    features: dict[str, float | str]
    model_version: str = Field(..., description="Semantic identifier for the loaded pipeline artifact")
