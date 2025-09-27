import axiosInstance from "../axios/axios.js";

const measurementRepository = {
  createMeasurement: async (payload) => {
    // payload is MeasurementDto shape
    const res = await axiosInstance.post("/measurements", payload);
    return res.data;
  },
  predictField: async (payload) => {
    // payload is PredictionDto shape (same as MeasurementDto minus E-field, Status, Remarks)
    const res = await axiosInstance.post("/predict", payload);
    return res.data; // { electricFieldDbuvPerM, technology, model }
  },
  fetchMeasurements: async (params = {}) => {
    // params: municipalityId?, settlementId?, dateFrom?, dateTo?, technology?
    const res = await axiosInstance.get("/measurements", { params });
    console.log(res.data);
    return res.data; // MeasurementResponseDto[]
  },
  updateMeasurement: async (id, payload) => {
    const res = await axiosInstance.put(`/measurements/${id}`, payload);
    return res.data;
  },
  deleteMeasurement: async (id) => {
    await axiosInstance.delete(`/measurements/${id}`);
  },
};

export default measurementRepository;
