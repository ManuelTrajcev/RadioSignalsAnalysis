import os, re, json, math, argparse, warnings
from pathlib import Path
import numpy as np
import pandas as pd

warnings.filterwarnings("ignore")


# Cleaning helpers
def normalize_text(s):
    if pd.isna(s): return s
    return str(s).replace("\u200b", "").strip()


def rebuild_dataframe(df):
    # Reconstruct header from first row and drop empty columns
    header_row = df.iloc[0].astype(str).tolist()
    df2 = df.copy().iloc[1:].reset_index(drop=True)
    df2.columns = header_row
    df2 = df2.loc[:, [c for c in df2.columns if isinstance(c, str) and c.strip() != ""]]
    df2.columns = [normalize_text(c) for c in df2.columns]
    df2 = df2.applymap(normalize_text)
    return df2


def parse_date(x):
    if pd.isna(x): return pd.NaT
    s = str(x).strip().replace(".", "/").replace(",", "/").replace("-", "/")
    for fmt in ("%d/%m/%Y", "%d/%m/%y", "%Y/%m/%d", "%d/%m/%Y %H:%M:%S"):
        try:
            return pd.to_datetime(s, format=fmt, errors="raise")
        except Exception:
            pass
    return pd.to_datetime(s, errors="coerce", dayfirst=True)


def parse_field_strength(s):
    if pd.isna(s): return np.nan
    s = str(s).strip().replace(",", ".")
    m = re.search(r"(\d+(\.\d+)?)", s)
    if m:
        val = float(m.group(1))
        if "<" in s:
            val = max(0.0, val - 0.1)
        return val
    return np.nan


def _num_from_text(s):
    s = str(s)
    m = re.search(r"(-?\d+(?:[.,]\d+)?)", s)
    return float(m.group(1).replace(",", ".")) if m else np.nan

def detect_tech(settlement_raw, chfreq_val, program):
    st = str(settlement_raw or "").lower()
    s  = str(chfreq_val or "").lower()
    p  = str(program or "").lower()

    if "дигитал" in s: return "digital"
    if "ф.м" in st or "фм" in st: return "fm"
    if "мра" in p or "радио" in p: return "fm"

    num = _num_from_text(s)  # works for "89,8 MHz"
    if not np.isnan(num):
        if 21 <= num <= 65 and float(num).is_integer():
            return "digital"              # TV channels first
        if 87.0 <= num <= 107.9: return "fm"
        if 65.0 <= num < 70.0:   return "fm"  # edge cases logged near band edge

    if "мтв" in p: return "digital"
    return None


def extract_channel_or_freq(chfreq_val):
    s = str(chfreq_val or "").lower()
    num = _num_from_text(s)
    if "дигитал" in s:
        return (int(num) if not np.isnan(num) else np.nan, np.nan)
    if not np.isnan(num):
        if 21 <= num <= 65 and float(num).is_integer():
            return (int(num), np.nan)      # TV channel
        # treat decimal/typical FM band as frequency
        if 87.0 <= num <= 107.9 or (65.0 <= num < 70.0):
            return (np.nan, float(num))
    return (np.nan, np.nan)



def load_and_clean(excel_path):
    xls = pd.ExcelFile(excel_path)
    raw = pd.read_excel(excel_path, sheet_name=0)
    df = rebuild_dataframe(raw)

    # Find coordinate block positions (between "Координати" and "Надм.височина(м)")
    cols = df.columns.tolist()
    coord_slice = (cols.index("Координати"), cols.index("Надм.височина(м)")) if (
                "Координати" in cols and "Надм.височина(м)" in cols) else None

    def parse_dms_block_row(row):
        if coord_slice is None: return np.nan, np.nan
        start, end = coord_slice
        seq = [str(v).strip() for v in row.iloc[start:end].tolist() if pd.notna(v) and str(v).strip() != "nan"]
        if not seq: return np.nan, np.nan
        seq = [s.replace("Е", "E").replace("е", "e") for s in seq]
        try:
            n_idx, e_idx = seq.index("N"), seq.index("E")
        except ValueError:
            return np.nan, np.nan

        def take_after(idx, k):
            out, i = [], idx + 1
            while len(out) < k and i < len(seq):
                s = seq[i].replace(",", ".")
                if re.match(r"^-?\d+(\.\d+)?$", s): out.append(float(s))
                i += 1
            return out if len(out) == k else None

        n, e = take_after(n_idx, 3), take_after(e_idx, 3)
        if not n or not e: return np.nan, np.nan
        dms = lambda d, m, s: d + m / 60 + s / 3600
        return dms(*n), dms(*e)

    target_hdr = "Ел.поле(dBµV/m)" if "Ел.поле(dBµV/m)" in df.columns else "Ел.поле(dBμV/m)"
    cores = ["Општина", "Населено место", "Матичен број", "Население", "Домаќинства", "Дата", "Потесна локација",
             "Надм.височина(м)", "Канал-Фрекв.", "Програма-Идентиф.", "Објект од каде се емитира", target_hdr]
    rename = {cores[0]: "municipality", cores[1]: "settlement_raw", cores[2]: "place_id", cores[3]: "population",
              cores[4]: "households", cores[5]: "date", cores[6]: "sublocation", cores[7]: "elevation_m",
              cores[8]: "ch_freq_raw", cores[9]: "program_id", cores[10]: "emitter", cores[11]: "field_dbuv_m"}
    data = df[cores].rename(columns=rename).copy()

    # Coords
    lat, lon = [], []
    for _, row in df.iterrows():
        a, b = parse_dms_block_row(row);
        lat.append(a);
        lon.append(b)
    data["latitude"], data["longitude"] = lat, lon

    data["date"] = data["date"].map(parse_date)
    data["field_dbuv_m"] = data["field_dbuv_m"].map(parse_field_strength)
    data["elevation_m"] = pd.to_numeric(
        data["elevation_m"].astype(str).str.replace(",", ".").str.extract(r"([\-]?\d+(\.\d+)?)")[0], errors="coerce")
    data["settlement"] = (
        data["settlement_raw"].astype(str).str.replace(r"\*\*", "", regex=True).str.replace(r"\(.*?\)", "",
                                                                                            regex=True).str.strip())

    techs, tvch, fmf = [], [], []
    for idx, r in data.iterrows():
        techs.append(detect_tech(r["settlement_raw"], r["ch_freq_raw"], r['program_id']))
        ch, fr = extract_channel_or_freq(r["ch_freq_raw"]);
        tvch.append(ch);
        fmf.append(fr)
    data["tech"], data["tv_channel"], data["fm_freq_mhz"] = techs, tvch, fmf

    data["year"], data["month"] = data["date"].dt.year, data["date"].dt.month
    data = data[data["field_dbuv_m"].notna()].copy()
    return data


def prepare_subset(data, tech):
    if tech == "digital":
        s = data[(data["tech"] == "digital") & (data["tv_channel"].notna())].copy()
        s["group_key"] = s["settlement"].astype(str) + "_ch" + s["tv_channel"].astype(int).astype(str)
    else:
        s = data[(data["tech"] == "fm") & (data["fm_freq_mhz"].notna())].copy()
        s["group_key"] = s["settlement"].astype(str) + "_fm" + s["fm_freq_mhz"].round(1).astype(str)
    keep = s["group_key"].value_counts()
    keep = keep[keep >= 3].index
    return s[s["group_key"].isin(keep)].copy()


# Modeling
from sklearn.pipeline import Pipeline
from sklearn.compose import ColumnTransformer
from sklearn.impute import SimpleImputer
from sklearn.preprocessing import OneHotEncoder, StandardScaler
from sklearn.tree import DecisionTreeRegressor
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import RandomizedSearchCV, GroupKFold, GroupShuffleSplit, cross_validate
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
import joblib


def build_preprocessor(numeric, categorical, rare_threshold=10):
    num = Pipeline([("imputer", SimpleImputer(strategy="median")), ("scaler", StandardScaler())])
    cat = Pipeline([("imputer", SimpleImputer(strategy="most_frequent")),
                    ("onehot", OneHotEncoder(handle_unknown="ignore", min_frequency=rare_threshold))])
    return ColumnTransformer([("num", num, numeric), ("cat", cat, categorical)])


def train(subset, tech, out_dir, random_iter=15):
    target = "field_dbuv_m"
    num = ["latitude", "longitude", "elevation_m", "year", "month", "population", "households"] + (
        ["tv_channel"] if tech == "digital" else ["fm_freq_mhz"])
    cat = ["municipality", "settlement", "program_id", "emitter"]
    for c in ["population", "households"]:
        subset[c] = pd.to_numeric(subset[c].astype(str).str.replace(",", ".").str.extract(r"(\d+(\.\d+)?)")[0],
                                  errors="coerce")

    X = subset[num + cat].copy();
    y = subset[target].astype(float).values;
    groups = subset["group_key"].astype(str).values
    gkf = GroupKFold(n_splits=5 if len(np.unique(groups)) >= 5 else max(2, len(np.unique(groups))))
    pre = build_preprocessor(num, cat, 10)

    dt = DecisionTreeRegressor(random_state=42)
    rf = RandomForestRegressor(random_state=42, n_estimators=300, n_jobs=-1)

    dt_params = {"model__max_depth": list(range(3, 26)), "model__min_samples_split": [2, 5, 10, 15],
                 "model__min_samples_leaf": [1, 2, 4, 8], "model__max_features": ["sqrt", "log2", 0.6, 0.8, None]}
    rf_params = {"model__n_estimators": [200, 300, 400], "model__max_depth": [None, 10, 20, 30],
                 "model__min_samples_split": [2, 5, 10], "model__min_samples_leaf": [1, 2, 4],
                 "model__max_features": ["sqrt", "log2", 0.5, None], "model__bootstrap": [True]}

    def fit_search(model, params, iters):
        pipe = Pipeline([("pre", pre), ("model", model)])
        search = RandomizedSearchCV(pipe, params, n_iter=iters, cv=gkf.split(X, y, groups=groups), n_jobs=-1,
                                    scoring="neg_root_mean_squared_error", random_state=42, verbose=0)
        search.fit(X, y)
        best = search.best_estimator_
        cv = cross_validate(best, X, y, cv=gkf.split(X, y, groups=groups),
                            scoring={"rmse": "neg_root_mean_squared_error", "mae": "neg_mean_absolute_error",
                                     "r2": "r2"}, n_jobs=-1)
        return best, {"cv_rmse_mean": float(np.mean(-cv["test_rmse"])), "cv_rmse_std": float(np.std(-cv["test_rmse"])),
                      "cv_mae_mean": float(np.mean(-cv["test_mae"])), "cv_mae_std": float(np.std(cv["test_mae"])),
                      "cv_r2_mean": float(np.mean(cv["test_r2"])), "cv_r2_std": float(np.std(cv["test_r2"])),
                      "best_params": search.best_params_}

    best_dt, m_dt = fit_search(dt, dt_params, random_iter)
    best_rf, m_rf = fit_search(rf, rf_params, random_iter)

    winner_name, winner = ("RandomForest", best_rf) if m_rf["cv_rmse_mean"] <= m_dt["cv_rmse_mean"] else (
    "DecisionTree", best_dt)
    winner_metrics = m_rf if winner_name == "RandomForest" else m_dt

    gss = GroupShuffleSplit(n_splits=1, test_size=0.2, random_state=42)
    tr, te = next(gss.split(X, y, groups=groups))
    winner.fit(X.iloc[tr], y[tr]);
    yp = winner.predict(X.iloc[te])
    holdout = {"rmse": float(math.sqrt(mean_squared_error(y[te], yp))), "mae": float(mean_absolute_error(y[te], yp)),
               "r2": float(r2_score(y[te], yp)), "n_test": int(len(te))}

    out_dir.mkdir(parents=True, exist_ok=True)
    model_path = out_dir / f"best_{tech}_model.joblib"
    joblib.dump(winner, model_path)

    metrics = {"tech": tech, "winner": winner_name, "winner_cv": winner_metrics, "dt_cv": m_dt, "rf_cv": m_rf,
               "holdout": holdout, "rows": int(len(subset)), "groups": int(len(np.unique(groups))),
               "features": {"numeric": num, "categorical": cat}}
    with open(out_dir / f"metrics_{tech}.json", "w", encoding="utf-8") as f:
        json.dump(metrics, f, ensure_ascii=False, indent=2)
    return metrics, str(model_path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--data", type=str, required=True, help="Path to 'signal_data.xlsx'")
    ap.add_argument("--out", type=str, default="artifacts", help="Output folder")
    ap.add_argument("--iters", type=int, default=15, help="RandomizedSearch iterations (digital). FM uses half this.")
    args = ap.parse_args()

    data = load_and_clean(args.data)
    digital = prepare_subset(data, "digital")
    fm = prepare_subset(data, "fm")

    digital_metrics, dm_path = train(digital, "digital", Path(args.out), random_iter=max(5, args.iters))
    fm_iters = max(4, args.iters // 2)
    fm_metrics, fm_path = train(fm, "fm", Path(args.out), random_iter=fm_iters)

    print("Digital metrics:");
    print(json.dumps(digital_metrics, indent=2, ensure_ascii=False))
    print("\nFM metrics:");
    print(json.dumps(fm_metrics, indent=2, ensure_ascii=False))
    print(f"\nSaved models:\n- {dm_path}\n- {fm_path}")


if __name__ == "__main__":
    main()