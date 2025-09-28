import axiosInstance from "../axios/axios.js";

const predictionRepository = {
  predictField: async (payload) => {
    const res = await axiosInstance.post("/predictions", payload);
    return res.data;
  },
};

export default predictionRepository;
