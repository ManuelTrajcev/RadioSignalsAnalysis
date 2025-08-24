import axiosInstance from "../axios/axios.js";

const homeRepository = {
    getHome: async () => {
        return await axiosInstance.get("/home");
    },
    // findMine: async () => {
    //     return await axiosInstance.get("/workspace/my-workspaces");
    // },
    // accessWorkspace: async (id) => {
    //     return await axiosInstance.get(`/workspace/${id}`);
    // },
    // editWorkspace: async (id, data) => {
    //     return await axiosInstance.post(
    //         `/workspace/edit/${id}`,
    //         data
    //     );
    // },
    // add: async (data) => {
    //     return await axiosInstance.post("/workspace/add", data);
    // },
    // update: async (id, data) => {
    //     return await axiosInstance.put(`/workspace/edit/${id}`, data);
    // },
    // delete: async (id) => {
    //     return await axiosInstance.delete(`/workspace/delete/${id}`);
    // }
};

export default homeRepository;
