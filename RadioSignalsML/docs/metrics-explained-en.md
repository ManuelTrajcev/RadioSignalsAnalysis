# Model Metrics

## What the models do

Both models take information about a radio transmitter site (location, altitude,
channel, date, etc.) and **predict the electric field strength** at that site,
measured in **dBµV/m** (decibels-microvolts per metre).

Think of it like predicting the signal "loudness" a receiver would experience.
A higher dBµV/m value means a stronger, cleaner signal.

---

## The three numbers that matter

### MAE — Mean Absolute Error
**"On average, how many dBµV/m off is the prediction?"**

If MAE = 8, the model's predictions are typically within ±8 dBµV/m of the real
measured value. Lower is better.

> Analogy: if you ask someone to guess the temperature outside and they're
> usually within 8 degrees — that's your MAE.

---

### RMSE — Root Mean Square Error
**"What is the typical size of the prediction errors, punishing big mistakes more?"**

RMSE is similar to MAE but larger errors count more heavily. If RMSE is
noticeably larger than MAE it means there are occasional large misses.
Lower is better.

---

### R² — R-squared (Coefficient of Determination)
**"How much of the real variation in signal strength does the model explain?"**

Ranges from 0 to 1 (can go negative if the model is very poor):

| R² value | What it means |
|---|---|
| 1.0 | Perfect — model explains everything |
| 0.7 | Good — model explains 70% of the variation |
| 0.4 | Moderate — model captures roughly half the picture |
| 0.0 or below | Poor — no better than always guessing the average |

Higher is better.

---

## Current model results

### Digital TV model (RandomForest)

| Metric | Cross-validation | Hold-out test |
|---|---|---|
| MAE | **8.6 dBµV/m** | 8.6 dBµV/m |
| RMSE | **10.9 dBµV/m** | 11.0 dBµV/m |
| R² | **0.48** | 0.34 |

Trained on **2,343 measurements** across **468 unique sites**.

**Plain reading:** The Digital TV model is off by about 8–9 dBµV/m on a typical
prediction and explains roughly half of the real-world variation in signal
strength. Results are consistent between training and testing, which means the
model is not overfitting.

---

### FM model (RandomForest)

| Metric | Cross-validation | Hold-out test |
|---|---|---|
| MAE | **5.9 dBµV/m** | 6.5 dBµV/m |
| RMSE | **7.6 dBµV/m** | 8.8 dBµV/m |
| R² | **0.77** | 0.67 |

Trained on **288 measurements** across **68 unique sites**.

**Plain reading:** The FM model is more accurate — it is off by about 6–7
dBµV/m on average and explains around 70% of the variation. This is a strong
result for an engineering prediction task.

---

## What "cross-validation" vs "hold-out test" means

| Term | Plain meaning |
|---|---|
| **Cross-validation** | The model was tested on 5 different slices of the training data, so we see how it performs on data it has not memorised. |
| **Hold-out test** | A completely separate 20% of the data was locked away, never used in training, and used only once at the end to give an honest final score. |

A model that scores well on both is trustworthy. A large gap between the two
would signal a problem.

---

## Bottom line

| Model | Typical error | Quality |
|---|---|---|
| Digital TV | ~9 dBµV/m | Moderate — useful for planning, not a substitute for field measurement |
| FM | ~7 dBµV/m | Good — reliable for site screening and coverage estimation |

The predictions are best used to **prioritise which sites to inspect** and to
**flag anomalies** in measured data, rather than to replace on-site measurements.
