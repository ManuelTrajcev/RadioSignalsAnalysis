import axiosInstance from "../axios/axios.js";

const masterDataRepository = {
  fetchMunicipalities: async () => {
    const res = await axiosInstance.get("/master-data/municipalities");
    return res.data; // [{ id, name }]
  },
  fetchSettlements: async (municipalityId) => {
    if (!municipalityId) return [];
    const res = await axiosInstance.get(
      `/master-data/municipalities/${municipalityId}/settlements`
    );
    return res.data; // [{ id, name }]
  },
};

export default masterDataRepository;
