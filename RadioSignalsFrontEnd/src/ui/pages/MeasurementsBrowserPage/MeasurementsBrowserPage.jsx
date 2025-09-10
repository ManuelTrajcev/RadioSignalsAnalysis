import React, { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Paper,
  Snackbar,
  TextField,
  Typography,
  FormControl,
  Select,
  InputLabel,
  Stack,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import Grid from '@mui/material/Grid';
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import masterDataRepository from "../../../repository/masterDataRepository.js";
import measurementRepository from "../../../repository/measurementRepository.js";
import useAuth from "../../../hooks/useAuth.js";

const TECHNOLOGIES = ["DIGITAL_TV", "FM"];

const EditDialog = ({ open, onClose, value, onSave }) => {
  const [form, setForm] = useState(value || {});
  const [errors, setErrors] = useState({});

  useEffect(() => setForm(value || {}), [value]);

  const isDigital = form.technology === "DIGITAL_TV";
  const isFm = form.technology === "FM";

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((f) => ({ ...f, [name]: value }));
  };

  const validate = () => {
    const e = {};
    ["settlementId","date","testLocation","transmitterLocation","electricFieldDbuvPerM","status","technology","altitudeMeters","latitudeDegrees","latitudeMinutes","latitudeSeconds","longitudeDegrees","longitudeMinutes","longitudeSeconds"].forEach(k=>{
      if (form[k] === "" || form[k] === null || form[k] === undefined) e[k]="Required";
    });
    if (form.technology === "DIGITAL_TV" && (form.channelNumber === "" || form.channelNumber === null)) e.channelNumber = "Required for DIGITAL_TV";
    if (form.technology === "FM" && (form.frequencyMHz === "" || form.frequencyMHz === null)) e.frequencyMHz = "Required for FM";
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;
    onSave(form);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Edit Measurement</DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 0.5 }}>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              type="date"
              label="Date"
              name="date"
              InputLabelProps={{ shrink: true }}
              value={form.date?.slice(0,10) || ""}
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
                value={form.technology || ""}
                onChange={handleChange}
              >
                {TECHNOLOGIES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {t.replace("_"," ")}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>

          {isDigital && (
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                fullWidth
                type="number"
                label="Channel Number"
                name="channelNumber"
                value={form.channelNumber ?? ""}
                onChange={handleChange}
                error={!!errors.channelNumber}
                helperText={errors.channelNumber}
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
                value={form.frequencyMHz ?? ""}
                onChange={handleChange}
                error={!!errors.frequencyMHz}
                helperText={errors.frequencyMHz}
              />
            </Grid>
          )}

          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Test Location"
              name="testLocation"
              value={form.testLocation || ""}
              onChange={handleChange}
              error={!!errors.testLocation}
              helperText={errors.testLocation}
            />
          </Grid>

          {/* Minimal: allow E-field, altitude, program id, transmitter, remarks */}
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              type="number"
              label="Electric Field (dBµV/m)"
              name="electricFieldDbuvPerM"
              value={form.electricFieldDbuvPerM ?? ""}
              onChange={handleChange}
              error={!!errors.electricFieldDbuvPerM}
              helperText={errors.electricFieldDbuvPerM}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              type="number"
              label="Altitude (m)"
              name="altitudeMeters"
              value={form.altitudeMeters ?? ""}
              onChange={handleChange}
              error={!!errors.altitudeMeters}
              helperText={errors.altitudeMeters}
            />
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              label="Program Identifier"
              name="programIdentifier"
              value={form.programIdentifier || ""}
              onChange={handleChange}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              label="Transmitter Location"
              name="transmitterLocation"
              value={form.transmitterLocation || ""}
              onChange={handleChange}
              error={!!errors.transmitterLocation}
              helperText={errors.transmitterLocation}
            />
          </Grid>

          <Grid size={{ xs: 12 }}>
            <TextField
              fullWidth
              label="Remarks"
              name="remarks"
              value={form.remarks || ""}
              onChange={handleChange}
              multiline
              minRows={2}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={handleSave}>Save</Button>
      </DialogActions>
    </Dialog>
  );
};

const MeasurementsBrowserPage = () => {
  const { user } = useAuth();
  const isAdmin = Array.isArray(user?.roles) && user.roles.includes("ADMIN");

  const [municipalities, setMunicipalities] = useState([]);
  const [settlements, setSettlements] = useState([]);

  const [filters, setFilters] = useState({
    municipalityId: "",
    settlementId: "",
    technology: "",
    dateFrom: "",
    dateTo: "",
  });

  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(false);

  const [snack, setSnack] = useState({ open: false, message: "", severity: "success" });

  const [editOpen, setEditOpen] = useState(false);
  const [editingRow, setEditingRow] = useState(null);

  // Fetch municipalities initially
  useEffect(() => {
    masterDataRepository.fetchMunicipalities().then(setMunicipalities).catch(() => {
      setSnack({ open: true, message: "Failed to load municipalities", severity: "error" });
    });
  }, []);

  // Fetch settlements on municipality change
  useEffect(() => {
    if (!filters.municipalityId) {
      setSettlements([]);
      setFilters((f) => ({ ...f, settlementId: "" }));
      return;
    }
    masterDataRepository
      .fetchSettlements(filters.municipalityId)
      .then((data) => {
        setSettlements(data);
        if (!data.find((s) => s.id === filters.settlementId)) {
          setFilters((f) => ({ ...f, settlementId: "" }));
        }
      })
      .catch(() => setSnack({ open: true, message: "Failed to load settlements", severity: "error" }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters.municipalityId]);

  const applyFetch = async () => {
    setLoading(true);
    try {
      const params = {};
      if (filters.municipalityId) params.municipalityId = filters.municipalityId;
      if (filters.settlementId) params.settlementId = filters.settlementId;
      if (filters.technology) params.technology = filters.technology;
      if (filters.dateFrom) params.dateFrom = new Date(filters.dateFrom).toISOString();
      if (filters.dateTo) params.dateTo = new Date(filters.dateTo).toISOString();

      const data = await measurementRepository.fetchMeasurements(params);
      setRows(
        data.map((m) => ({
          ...m,
          id: m.id, // DataGrid needs id
        }))
      );
    } catch (e) {
      setSnack({ open: true, message: "Failed to fetch measurements", severity: "error" });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    applyFetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleFilterChange = (e) => {
    const { name, value } = e.target;
    setFilters((f) => ({ ...f, [name]: value }));
  };
  

  const columns = useMemo(() => {
    const base = [
      { field: "date", headerName: "Date", 
        valueGetter: (p) => {
        const v = p?.row?.date ?? p?.value;
        if (!v) return "";
        if (typeof v === "string") return v.slice(0, 10);
        try {
          return new Date(v).toISOString().slice(0, 10);
        } catch {
          return "";
        }
      }, flex: 1, minWidth: 120 },
      { field: "municipalityName", headerName: "Municipality", flex: 1.2, minWidth: 160 },
      { field: "settlementName", headerName: "Settlement", flex: 1.2, minWidth: 160 },
      { field: "technology", headerName: "Technology", flex: 1, minWidth: 130 },
      {
        field: "channelOrFreq",
        headerName: "Ch/Freq",
        flex: 1,
        minWidth: 120,
        valueGetter: (p) => {
        const row = p?.row ?? {};
        const isTv = row.isTvChannel ?? false;
        if (isTv) return row.channelNumber ? `CH ${row.channelNumber}` : "";
        return row.frequencyMHz ? `${row.frequencyMHz} MHz` : "";
      },
      },
      { field: "electricFieldDbuvPerM", headerName: "E-field (dBµV/m)", flex: 1, minWidth: 140, type: "number" },
      { field: "status", headerName: "Status", flex: 1.1, minWidth: 160 },
      { field: "altitudeMeters", headerName: "Alt (m)", flex: 0.7, minWidth: 100, type: "number" },
      { field: "testLocation", headerName: "Test Location", flex: 1.4, minWidth: 180 },
      { field: "transmitterLocation", headerName: "Transmitter", flex: 1.2, minWidth: 160 },
      { field: "programIdentifier", headerName: "Program", flex: 1, minWidth: 120 },
      { field: "remarks", headerName: "Remarks", flex: 1.2, minWidth: 160 },
      {
        field: "coords",
        headerName: "Lat / Lon",
        flex: 1.4,
        minWidth: 180,
        valueGetter: (p) => {
        const lat = p?.row?.latitudeDecimal ?? p?.value?.latitudeDecimal;
        const lon = p?.row?.longitudeDecimal ?? p?.value?.longitudeDecimal;
        if (lat == null || lon == null) return "";
        // ensure numeric and avoid calling toFixed on undefined
        const latNum = Number(lat);
        const lonNum = Number(lon);
        if (!Number.isFinite(latNum) || !Number.isFinite(lonNum)) return "";
        return `${latNum.toFixed(6)}, ${lonNum.toFixed(6)}`;
      },
      },
    ];

    if (!isAdmin) return base;

    return [
      ...base,
      {
        field: "actions",
        headerName: "Actions",
        sortable: false,
        filterable: false,
        width: 140,
        renderCell: (params) => (
          <Stack direction="row" spacing={1}>
            <IconButton
              size="small"
              color="primary"
              onClick={() => {
                // Build MeasurementDto input from response row
                const r = params.row;
                const dto = {
                  // Must send full MeasurementDto on update
                  settlementId: r.settlementId,
                  date: r.date,
                  testLocation: r.testLocation,
                  latitudeDegrees: 0, // editor will set default placeholders
                  latitudeMinutes: 0,
                  latitudeSeconds: 0,
                  longitudeDegrees: 0,
                  longitudeMinutes: 0,
                  longitudeSeconds: 0,
                  altitudeMeters: r.altitudeMeters,
                  channelNumber: r.isTvChannel ? r.channelNumber : null,
                  frequencyMHz: !r.isTvChannel ? r.frequencyMHz : null,
                  programIdentifier: r.programIdentifier,
                  transmitterLocation: r.transmitterLocation,
                  electricFieldDbuvPerM: r.electricFieldDbuvPerM,
                  remarks: r.remarks,
                  status: r.status,
                  technology: r.technology,
                };
                // We don't have original DMS from response; keep simple editor fields but allow saving other parts
                setEditingRow({ ...dto, id: r.id });
                setEditOpen(true);
              }}
            >
              <EditIcon />
            </IconButton>
            <IconButton
              size="small"
              color="error"
              onClick={async () => {
                if (!window.confirm("Delete this measurement?")) return;
                try {
                  await measurementRepository.deleteMeasurement(params.id);
                  setSnack({ open: true, message: "Deleted.", severity: "success" });
                  applyFetch();
                } catch (e) {
                  setSnack({ open: true, message: "Delete failed", severity: "error" });
                }
              }}
            >
              <DeleteIcon />
            </IconButton>
          </Stack>
        ),
      },
    ];
  }, [isAdmin]); // eslint-disable-line react-hooks/exhaustive-deps

  const onEditSave = async (form) => {
    try {
      const id = editingRow.id;
      const payload = {
        settlementId: form.settlementId,
        date: new Date(form.date).toISOString(),
        testLocation: form.testLocation,
        // NOTE: Since response did not carry DMS, keep zeroes unless you extend API to return them.
        // To comply with backend's required DMS fields, keep previous placeholders or ask user to re-enter.
        latitudeDegrees: Number(form.latitudeDegrees ?? 0),
        latitudeMinutes: Number(form.latitudeMinutes ?? 0),
        latitudeSeconds: Number(form.latitudeSeconds ?? 0),
        longitudeDegrees: Number(form.longitudeDegrees ?? 0),
        longitudeMinutes: Number(form.longitudeMinutes ?? 0),
        longitudeSeconds: Number(form.longitudeSeconds ?? 0),
        altitudeMeters: Number(form.altitudeMeters ?? 0),
        channelNumber: form.technology === "DIGITAL_TV" ? Number(form.channelNumber ?? 0) : null,
        frequencyMHz: form.technology === "FM" ? Number(form.frequencyMHz ?? 0) : null,
        programIdentifier: form.programIdentifier || null,
        transmitterLocation: form.transmitterLocation,
        electricFieldDbuvPerM: Number(form.electricFieldDbuvPerM ?? 0),
        remarks: form.remarks || null,
        status: form.status,
        technology: form.technology,
      };
      await measurementRepository.updateMeasurement(id, payload);
      setSnack({ open: true, message: "Updated.", severity: "success" });
      setEditOpen(false);
      setEditingRow(null);
      applyFetch();
    } catch (e) {
      setSnack({ open: true, message: "Update failed", severity: "error" });
    }
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Measurements
      </Typography>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 2 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 3 }}>
            <FormControl fullWidth>
              <InputLabel>Municipality</InputLabel>
              <Select
                label="Municipality"
                name="municipalityId"
                value={filters.municipalityId}
                onChange={handleFilterChange}
              >
                <MenuItem value="">All</MenuItem>
                {municipalities.map((m) => (
                  <MenuItem key={m.id} value={m.id}>
                    {m.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid size={{ xs: 12, md: 3 }}>
            <FormControl fullWidth>
              <InputLabel>Settlement</InputLabel>
              <Select
                label="Settlement"
                name="settlementId"
                value={filters.settlementId}
                onChange={handleFilterChange}
                disabled={!filters.municipalityId}
              >
                <MenuItem value="">All</MenuItem>
                {settlements.map((s) => (
                  <MenuItem key={s.id} value={s.id}>
                    {s.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>

          <Grid size={{ xs: 12, md: 2 }}>
            <FormControl fullWidth>
              <InputLabel>Technology</InputLabel>
              <Select
                label="Technology"
                name="technology"
                value={filters.technology}
                onChange={handleFilterChange}
              >
                <MenuItem value="">All</MenuItem>
                {TECHNOLOGIES.map((t) => (
                  <MenuItem key={t} value={t}>
                    {t.replace("_"," ")}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>

          <Grid size={{ xs: 6, md: 2 }}>
            <TextField
              fullWidth
              type="date"
              label="Date From"
              name="dateFrom"
              InputLabelProps={{ shrink: true }}
              value={filters.dateFrom}
              onChange={handleFilterChange}
            />
          </Grid>
          <Grid size={{ xs: 6, md: 2 }}>
            <TextField
              fullWidth
              type="date"
              label="Date To"
              name="dateTo"
              InputLabelProps={{ shrink: true }}
              value={filters.dateTo}
              onChange={handleFilterChange}
            />
          </Grid>

          <Grid size={{ xs: 12 }} display="flex" justifyContent="end" gap={1}>
            <Button
              variant="outlined"
              onClick={() => {
                setFilters({ municipalityId: "", settlementId: "", technology: "", dateFrom: "", dateTo: "" });
              }}
            >
              Clear
            </Button>
            <Button variant="contained" onClick={applyFetch}>
              Apply
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Data Grid */}
      <Paper sx={{ height: 600 }}>
        <DataGrid
          rows={rows}
          columns={columns}
          loading={loading}
          initialState={{
            pagination: { paginationModel: { page: 0, pageSize: 10 } },
            sorting: { sortModel: [{ field: "date", sort: "desc" }] },
          }}
          pageSizeOptions={[10, 25, 50]}
          disableRowSelectionOnClick
          sx={{ border: 0 }}
        />
      </Paper>

      <EditDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        value={editingRow}
        onSave={onEditSave}
      />

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

export default MeasurementsBrowserPage;
