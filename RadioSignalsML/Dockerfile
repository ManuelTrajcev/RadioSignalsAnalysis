FROM python:3.11-slim

# libgomp1 is required by scikit-learn's OpenMP-backed estimators at inference time
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgomp1 \
    && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --gecos "" appuser

WORKDIR /app

# Dependency layer — cached until requirements.txt changes
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Application source
COPY service/ ./service/

# ML artefacts baked into the image; override at runtime via env vars + volumes
COPY artifacts/ ./artifacts/

USER appuser

ENV RADIO_SIGNALS_ML_ROOT=/app
ENV PREDICT_HOST=0.0.0.0
ENV PREDICT_PORT=8000

EXPOSE 8000

CMD ["uvicorn", "service.main:app", "--host", "0.0.0.0", "--port", "8000"]
