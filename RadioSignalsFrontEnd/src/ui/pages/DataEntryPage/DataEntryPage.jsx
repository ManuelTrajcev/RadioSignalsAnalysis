import React, { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  MenuItem,
  Paper,
  Snackbar,
  TextField,
  Typography,
  FormControl,
  InputLabel,
  Select,
} from "@mui/material";
import Grid from '@mui/material/Grid';
import masterDataRepository from "../../../repository/masterDataRepository.js";
import measurementRepository from "../../../repository/measurementRepository.js";
import useAuth from "../../../hooks/useAuth.js";

const TECHNOLOGIES = ["DIGITAL_TV", "FM"];
const STATUSES = [
  "ExemptFromFee",
  "Covered",
  "PartiallyCovered",
  "NotCovered",
  "TheoreticallyCovered",
  "NoResidents2002",
  "RadioSignal",
  "DigitalSignal",
  "NoDigitalDevices",
  "Renamed",
  "DeviceIssues",
  "Uninhabited",
];

const emptyForm = {
  settlementId: "",
  date: "",
  testLocation: "",
  latitudeDegrees: "",
  latitudeMinutes: "",
  latitudeSeconds: "",
  longitudeDegrees: "",
  longitudeMinutes: "",
  longitudeSeconds: "",
  altitudeMeters: "",
  channelNumber: "",
  frequencyMHz: "",
  programIdentifier: "",
  transmitterLocation: "",
  electricFieldDbuvPerM: "",
  remarks: "",
  status: "",
  technology: "",
  municipalityId: "", // UI-only helper for cascade
};

const clamp = (v, min, max) => {
  if (v === "" || v === null || Number.isNaN(Number(v))) return v;
  const n = Number(v);
  if (Number.isNaN(n)) return v;
  return Math.min(Math.max(n, min), max);
};

const DataEntryPage = () => {
  const { user: _user } = useAuth();
  const [municipalities, setMunicipalities] = useState([]);
  const [settlements, setSettlements] = useState([]);
  const [submitting, setSubmitting] = useState(false);
  const [snack, setSnack] = useState({ open: false, message: "", severity: "success" });

  const [form, setForm] = useState(emptyForm);
  const isDigital = form.technology === "DIGITAL_TV";
  const isFm = form.technology === "FM";

  // Validation state
  const [errors, setErrors] = useState({});

  // Load municipalities
  useEffect(() => {
    masterDataRepository
      .fetchMunicipalities()
      .then(setMunicipalities)
      .catch(() => setSnack({ open: true, message: "Failed to load municipalities", severity: "error" }));
  }, []);

  // Load settlements on municipality change
  useEffect(() => {
    if (!form.municipalityId) {
      setSettlements([]);
      setForm((f) => ({ ...f, settlementId: "" }));
      return;
    }
    masterDataRepository
      .fetchSettlements(form.municipalityId)
      .then((data) => {
        setSettlements(data);
        // if previous settlement isn't in new list, clear it
        if (!data.find((s) => s.id === form.settlementId)) {
          setForm((f) => ({ ...f, settlementId: "" }));
        }
      })
      .catch(() => setSnack({ open: true, message: "Failed to load settlements", severity: "error" }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.municipalityId]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Basic range clamping for numeric inputs
    if (name === "latitudeDegrees") return setForm({ ...form, [name]: clamp(value, 0, 90) });
    if (name === "longitudeDegrees") return setForm({ ...form, [name]: clamp(value, 0, 180) });
    if (name === "latitudeMinutes" || name === "longitudeMinutes")
      return setForm({ ...form, [name]: clamp(value, 0, 59) });
    if (name === "latitudeSeconds" || name === "longitudeSeconds")
      return setForm({ ...form, [name]: clamp(value, 0, 59.999) });
    if (name === "altitudeMeters") return setForm({ ...form, [name]: clamp(value, -400, 9000) }); // Dead Sea to Everest+
    if (name === "electricFieldDbuvPerM") return setForm({ ...form, [name]: clamp(value, 0, 200) });
    if (name === "channelNumber") return setForm({ ...form, [name]: clamp(value, 1, 1000) });
    if (name === "frequencyMHz") return setForm({ ...form, [name]: clamp(value, 50, 1100) });

    setForm({ ...form, [name]: value });
  };

  // Conditional reset when switching technology
  useEffect(() => {
    setForm((f) => ({
      ...f,
      channelNumber: isDigital ? f.channelNumber : "",
      frequencyMHz: isFm ? f.frequencyMHz : "",
    }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.technology]);

  const required = (v) => v !== "" && v !== null && v !== undefined;

  const validate = () => {
    const e = {};

    // required fields
    [
      "municipalityId",
      "settlementId",
      "date",
      "testLocation",
      "latitudeDegrees",
      "latitudeMinutes",
      "latitudeSeconds",
      "longitudeDegrees",
      "longitudeMinutes",
      "longitudeSeconds",
      "altitudeMeters",
      "transmitterLocation",
      "electricFieldDbuvPerM",
      "status",
      "technology",
    ].forEach((k) => {
      if (!required(form[k])) e[k] = "Required";
    });

    // toggle requirements
    if (form.technology === "DIGITAL_TV") {
      if (!required(form.channelNumber)) e.channelNumber = "Required for DIGITAL_TV";
    } else if (form.technology === "FM") {
      if (!required(form.frequencyMHz)) e.frequencyMHz = "Required for FM";
    }

    // numeric ranges sanity (already clamped but double-check)
    const numIn = (k, min, max) => {
      const n = Number(form[k]);
      if (Number.isNaN(n) || n < min || n > max) e[k] = `Must be between ${min} and ${max}`;
    };

    numIn("latitudeDegrees", 0, 90);
    numIn("latitudeMinutes", 0, 59);
    numIn("latitudeSeconds", 0, 59.999);
    numIn("longitudeDegrees", 0, 180);
    numIn("longitudeMinutes", 0, 59);
    numIn("longitudeSeconds", 0, 59.999);
    numIn("electricFieldDbuvPerM", 0, 200);

    if (form.technology === "DIGITAL_TV") numIn("channelNumber", 1, 1000);
    if (form.technology === "FM") numIn("frequencyMHz", 50, 1100);

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const buildPayload = () => {
    return {
      settlementId: form.settlementId,
      date: new Date(form.date).toISOString(),
      testLocation: form.testLocation,
      latitudeDegrees: Number(form.latitudeDegrees),
      latitudeMinutes: Number(form.latitudeMinutes),
      latitudeSeconds: Number(form.latitudeSeconds),
      longitudeDegrees: Number(form.longitudeDegrees),
      longitudeMinutes: Number(form.longitudeMinutes),
      longitudeSeconds: Number(form.longitudeSeconds),
      altitudeMeters: Number(form.altitudeMeters),
      channelNumber: form.technology === "DIGITAL_TV" ? Number(form.channelNumber) : null,
      frequencyMHz: form.technology === "FM" ? Number(form.frequencyMHz) : null,
      programIdentifier: form.programIdentifier || null,
      transmitterLocation: form.transmitterLocation,
      electricFieldDbuvPerM: Number(form.electricFieldDbuvPerM),
      remarks: form.remarks || null,
      status: form.status, // enums serialized as strings by backend
      technology: form.technology,
    };
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    if (!validate()) return;
    setSubmitting(true);
    try {
      await measurementRepository.createMeasurement(buildPayload());
      setSnack({ open: true, message: "Measurement saved.", severity: "success" });
      setForm(emptyForm);
      setSettlements([]);
    } catch (err) {
      setSnack({
        open: true,
        message: err?.response?.data || "Failed to save measurement",
        severity: "error",
      });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Data Entry
      </Typography>
      <Paper sx={{ p: 3 }}>
        <Box component="form" onSubmit={onSubmit}>
          <Grid container spacing={2}>
            {/* Municipality / Settlement */}
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth error={!!errors.municipalityId}>
                <InputLabel>Municipality</InputLabel>
                <Select
                  label="Municipality"
                  name="municipalityId"
                  value={form.municipalityId}
                  onChange={handleChange}
                >
                  {municipalities.map((m) => (
                    <MenuItem key={m.id} value={m.id}>
                      {m.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              {errors.municipalityId && (
                <Typography color="error" variant="caption">
                  {errors.municipalityId}
                </Typography>
              )}
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth error={!!errors.settlementId}>
                <InputLabel>Settlement</InputLabel>
                <Select
                  label="Settlement"
                  name="settlementId"
                  value={form.settlementId}
                  onChange={handleChange}
                  disabled={!form.municipalityId}
                >
                  {settlements.map((s) => (
                    <MenuItem key={s.id} value={s.id}>
                      {s.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              {errors.settlementId && (
                <Typography color="error" variant="caption">
                  {errors.settlementId}
                </Typography>
              )}
            </Grid>

            {/* Date / Technology */}
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                type="date"
                label="Date"
                name="date"
                InputLabelProps={{ shrink: true }}
                value={form.date}
                onChange={handleChange}
                error={!!errors.date}
                helperText={errors.date}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth error={!!errors.technology}>
                <InputLabel>Technology</InputLabel>
                <Select
                  label="Technology"
                  name="technology"
                  value={form.technology}
                  onChange={handleChange}
                >
                  {TECHNOLOGIES.map((t) => (
                    <MenuItem key={t} value={t}>
                      {t.replace("_", " ")}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              {errors.technology && (
                <Typography color="error" variant="caption">
                  {errors.technology}
                </Typography>
              )}
            </Grid>

            {/* Conditional Channel / Frequency */}
            {isDigital && (
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  type="number"
                  label="Channel Number"
                  name="channelNumber"
                  value={form.channelNumber}
                  onChange={handleChange}
                  error={!!errors.channelNumber}
                  helperText={errors.channelNumber || "Required for Digital TV"}
                />
              </Grid>
            )}
            {isFm && (
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  type="number"
                  label="Frequency (MHz)"
                  name="frequencyMHz"
                  value={form.frequencyMHz}
                  onChange={handleChange}
                  error={!!errors.frequencyMHz}
                  helperText={errors.frequencyMHz || "Required for FM"}
                />
              </Grid>
            )}

            {/* Location details */}
            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Test Location (description)"
                name="testLocation"
                value={form.testLocation}
                onChange={handleChange}
                error={!!errors.testLocation}
                helperText={errors.testLocation}
              />
            </Grid>

            {/* Coordinates DMS */}
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle1" sx={{ mb: 1 }}>
                Coordinates (DMS)
              </Typography>
              <Grid container spacing={2}>
                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label="Lat °"
                    name="latitudeDegrees"
                    value={form.latitudeDegrees}
                    onChange={handleChange}
                    error={!!errors.latitudeDegrees}
                    helperText={errors.latitudeDegrees}
                  />
                </Grid>
                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label="Lat '"
                    name="latitudeMinutes"
                    value={form.latitudeMinutes}
                    onChange={handleChange}
                    error={!!errors.latitudeMinutes}
                    helperText={errors.latitudeMinutes}
                  />
                </Grid>
                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label='Lat "'
                    name="latitudeSeconds"
                    value={form.latitudeSeconds}
                    onChange={handleChange}
                    error={!!errors.latitudeSeconds}
                    helperText={errors.latitudeSeconds}
                  />
                </Grid>

                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label="Lon °"
                    name="longitudeDegrees"
                    value={form.longitudeDegrees}
                    onChange={handleChange}
                    error={!!errors.longitudeDegrees}
                    helperText={errors.longitudeDegrees}
                  />
                </Grid>
                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label="Lon '"
                    name="longitudeMinutes"
                    value={form.longitudeMinutes}
                    onChange={handleChange}
                    error={!!errors.longitudeMinutes}
                    helperText={errors.longitudeMinutes}
                  />
                </Grid>
                <Grid size={{ xs: 4, md: 2 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label='Lon "'
                    name="longitudeSeconds"
                    value={form.longitudeSeconds}
                    onChange={handleChange}
                    error={!!errors.longitudeSeconds}
                    helperText={errors.longitudeSeconds}
                  />
                </Grid>
              </Grid>
            </Grid>

            {/* Altitude / E-field */}
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                type="number"
                label="Altitude (m)"
                name="altitudeMeters"
                value={form.altitudeMeters}
                onChange={handleChange}
                error={!!errors.altitudeMeters}
                helperText={errors.altitudeMeters}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                type="number"
                label="Electric Field (dBµV/m)"
                name="electricFieldDbuvPerM"
                value={form.electricFieldDbuvPerM}
                onChange={handleChange}
                error={!!errors.electricFieldDbuvPerM}
                helperText={errors.electricFieldDbuvPerM}
              />
            </Grid>

            {/* Program / Transmitter */}
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                label="Program Identifier (optional)"
                name="programIdentifier"
                value={form.programIdentifier}
                onChange={handleChange}
              />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                label="Transmitter Location"
                name="transmitterLocation"
                value={form.transmitterLocation}
                onChange={handleChange}
                error={!!errors.transmitterLocation}
                helperText={errors.transmitterLocation}
              />
            </Grid>

            {/* Status */}
            <Grid size={{ xs: 12, md: 6 }}>
              <FormControl fullWidth error={!!errors.status}>
                <InputLabel>Status</InputLabel>
                <Select
                  label="Status"
                  name="status"
                  value={form.status}
                  onChange={handleChange}
                >
                  {STATUSES.map((s) => (
                    <MenuItem key={s} value={s}>
                      {s}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              {errors.status && (
                <Typography color="error" variant="caption">
                  {errors.status}
                </Typography>
              )}
            </Grid>

            {/* Remarks */}
            <Grid size={{ xs: 12 }}>
              <TextField
                fullWidth
                label="Remarks"
                name="remarks"
                value={form.remarks}
                onChange={handleChange}
                multiline
                minRows={2}
              />
            </Grid>

            <Grid size={{ xs: 12 }} display="flex" justifyContent="end" gap={2}>
              <Button
                variant="outlined"
                onClick={() => {
                  setForm(emptyForm);
                  setSettlements([]);
                  setErrors({});
                }}
              >
                Clear
              </Button>
              <Button variant="contained" type="submit" disabled={submitting}>
                {submitting ? "Submitting..." : "Submit"}
              </Button>
            </Grid>
          </Grid>
        </Box>
      </Paper>

      <Snackbar
        open={snack.open}
        autoHideDuration={4000}
        onClose={() => setSnack((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          onClose={() => setSnack((s) => ({ ...s, open: false }))}
          severity={snack.severity}
          variant="filled"
          sx={{ width: "100%" }}
        >
          {snack.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};

export default DataEntryPage;
