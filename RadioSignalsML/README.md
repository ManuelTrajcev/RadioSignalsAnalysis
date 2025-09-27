# Running the Project

Follow the steps below to set up and train the signal models.

---

## 1. Activate Virtual Environment

First, enter your virtual environment in **Windows Terminal**:

```bash
.venv\Scripts\activate
```

---

## 2. Upload Dataset

Place your dataset (e.g. `signal_data.xlsx`) into the project directory.

---

## 3. Install Dependencies

Install all required Python packages:

```bash
pip install -r requirements.txt
```

---

## 4. Train Models

Run the training script:

```bash
py train_signal_models.py --data signal_data.xlsx --out ./artifacts
```

* `--data` : Path to your input dataset (`.xlsx` file).
* `--out`  : Output directory where trained models and metrics will be saved.

Example output artifacts:

* `best_digital_model.joblib`
* `best_fm_model.joblib`
* `metrics_digital.json`
* `metrics_fm.json`

---

## 5. Results

The script will:

* Clean and preprocess the dataset.
* Train and evaluate **Decision Tree** and **Random Forest** models.
* Automatically select the best-performing model for **digital TV signals** and **FM signals**.
* Save models and metrics in the `artifacts/` folder.

Console output includes:

* Cross-validation results (RMSE, MAE, R²).
* Holdout test performance.
* Paths to saved models.