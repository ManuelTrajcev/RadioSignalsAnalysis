import axios from "axios";

const axiosInstance = axios.create({
    baseURL: "http://localhost:5229/api",
    headers: {
        "Content-Type": "application/json",
    },
});

// axiosInstance.interceptors.request.use(
//     (config) => {
//         const jwtToken = localStorage.getItem("token");
//         if (jwtToken) {
//             config.headers.Authorization = `Bearer ${jwtToken}`;
//         }
//         return config;
//     },
//     (error) => {
//         if (error.response.status === 401 || error.response.status === 403) {
//             console.log("Invalid token");
//             localStorage.removeItem("token");
//             window.location.href = "/login";
//         }
//         return Promise.reject(error);
//     },
// );

export default axiosInstance;