import axiosInstance from "../axios/axios.js";

const settlementRepository = {
    createSettlement: async (settlementData) => {
        const res = await axiosInstance.post("/settlement", settlementData);
        return res.data;
    }
};

export default settlementRepository;
