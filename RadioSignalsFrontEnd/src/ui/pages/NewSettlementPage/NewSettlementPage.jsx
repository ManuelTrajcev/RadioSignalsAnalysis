import React, {useEffect, useState} from "react";
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
import settlementRepository from "../../../repository/settlementRepository.js"; // NEW IMPORT
import useAuth from "../../../hooks/useAuth.js";

// Initial state for the form
const emptyForm = {
    municipalityId: "",
    newSettlementName: "",
    registryNumber: "",
    population: "",
    houseHolds: "",
};

const NewSettlementPage = () => {
    const {user: _user} = useAuth();
    const [municipalities, setMunicipalities] = useState([]);
    const [submitting, setSubmitting] = useState(false);
    const [snack, setSnack] = useState({open: false, message: "", severity: "success"});

    const [form, setForm] = useState(emptyForm);
    const [errors, setErrors] = useState({});

    useEffect(() => {
        masterDataRepository
            .fetchMunicipalities()
            .then(setMunicipalities)
            .catch(() => setSnack({open: true, message: "Failed to load municipalities", severity: "error"}));
    }, []);

    const handleChange = (e) => {
        const {name, value} = e.target;
        setForm({...form, [name]: value});
        if (errors[name]) {
            setErrors((prev) => {
                const newErrors = {...prev};
                delete newErrors[name];
                return newErrors;
            });
        }
    };

    const validate = () => {
        const e = {};

        if (!form.municipalityId) {
            e.municipalityId = "Municipality is required";
        }

        if (!form.newSettlementName || form.newSettlementName.trim() === "") {
            e.newSettlementName = "Settlement name is required";
        }

        setErrors(e);
        return Object.keys(e).length === 0;
    };

    const onSubmit = async (e) => {
        e.preventDefault();
        if (!validate()) return;
        setSubmitting(true);

        const payload = {
            municipalityId: form.municipalityId,
            name: form.newSettlementName.trim(),
            population: form.population,
            houseHolds: form.houseHolds,
            registryNumber: form.registryNumber
        };

        try {
            await settlementRepository.createSettlement(payload);

            setSnack({open: true, message: "New settlement saved successfully.", severity: "success"});
            setForm(emptyForm);
            setErrors({});
        } catch (err) {
            // Improved error handling for common API response structure
            const errorMessage = err.response?.data?.message || err.response?.data || "Failed to save new settlement";
            setSnack({
                open: true,
                message: errorMessage,
                severity: "error",
            });
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Box>
            <Typography variant="h4" sx={{mb: 2}}>
                Додај ново населено место
            </Typography>
            <Paper sx={{p: 3}}>
                <Box component="form" onSubmit={onSubmit}>
                    <Grid container spacing={3}>
                        <Grid size={{xs: 12, md: 4}}>
                            <FormControl fullWidth error={!!errors.municipalityId}>
                                <InputLabel>Општина</InputLabel>
                                <Select
                                    label="Општина"
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
                                    Општината е задолжителна
                                </Typography>
                            )}
                        </Grid>
                        <Grid size={{xs: 12, md: 4}}>
                            <TextField
                                fullWidth
                                label="Име на населено место"
                                name="newSettlementName"
                                value={form.newSettlementName}
                                onChange={handleChange}
                                error={!!errors.newSettlementName}
                                helperText={errors.newSettlementName && "Името е задолжително"}
                            />
                        </Grid>
                        <Grid size={{xs: 12, md: 4}}>
                            <TextField
                                fullWidth
                                label="Регистарски број"
                                name="registryNumber"
                                value={form.registryNumber}
                                onChange={handleChange}
                                error={!!errors.registryNumber}
                                helperText={errors.registryNumber}
                            />
                        </Grid>
                        <Grid size={{xs: 12, md: 4}}>
                            <TextField
                                fullWidth
                                label="Број на жители"
                                name="population"
                                value={form.population}
                                onChange={handleChange}
                                error={!!errors.population}
                                helperText={errors.population}
                            />
                        </Grid>
                        <Grid size={{xs: 12, md: 4}}>
                            <TextField
                                fullWidth
                                label="Домаќинства"
                                name="houseHolds"
                                value={form.houseHolds}
                                onChange={handleChange}
                                error={!!errors.houseHolds}
                                helperText={errors.houseHolds}
                            />
                        </Grid>
                        <Grid item xs={12} display="flex" justifyContent="flex-end">
                            <Button variant="contained" type="submit" disabled={submitting}>
                                {submitting ? "Се зачувува..." : "Зачувај населено место"}
                            </Button>
                        </Grid>
                    </Grid>
                </Box>
            </Paper>
            <Snackbar
                open={snack.open}
                autoHideDuration={4000}
                onClose={() => setSnack((s) => ({...s, open: false}))}
                anchorOrigin={{vertical: "bottom", horizontal: "center"}}
            >
                <Alert
                    onClose={() => setSnack((s) => ({...s, open: false}))}
                    severity={snack.severity}
                    variant="filled"
                    sx={{width: "100%"}}
                >
                    {snack.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};

export default NewSettlementPage;
