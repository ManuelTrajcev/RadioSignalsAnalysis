import {useCallback, useEffect, useState} from "react";
import homeRepository from "../repository/homeRepository.js";
import homePage from "../ui/pages/HomePage/HomePage.jsx";

const initialState = {
    homePage: "",
    loading: true,
};

const useHome = () => {
    const [state, setState] = useState(initialState);

    const fetchHonePage = useCallback(() =>  {
        setState((prev) => ({...prev, loading: false}));
        homeRepository
            .getHome()
            .then((response) => {
                setState((prev) => ({
                    ...prev,
                    homePage: response.data,
                    loading: false,
                }))
            })
            .catch((err) => {
                console.log(err)
            })
    }, [])

    // const fetchAllWorkspaces = useCallback(() => {
    //     setState((prev) => ({...prev, loading: true}));
    //     homeRepository
    //         .getHome()
    //         .then((response) => {
    //             setState((prev) => ({
    //                 ...prev,
    //                 allWorkspaces: response.data,
    //                 loading: false,
    //             }));
    //         })
    //         .catch((error) => {
    //             console.error("Error fetching all workspaces:", error);
    //             setState((prev) => ({...prev, loading: false}));
    //         });
    // }, []);
    //
    // const fetchMyWorkspaces = useCallback(() => {
    //     setState((prev) => ({...prev, loading: true}));
    //     homeRepository
    //         .findMine()
    //         .then((response) => {
    //             setState((prev) => ({
    //                 ...prev,
    //                 myWorkspaces: response.data,
    //                 loading: false,
    //             }));
    //         })
    //         .catch((error) => {
    //             console.error("Error fetching my workspaces:", error);
    //             setState((prev) => ({...prev, loading: false}));
    //         });
    // }, []);
    //
    // const onAccess = useCallback(async (id) => {
    //     try {
    //         const response = await homeRepository.accessWorkspace(id);
    //         return response.data;
    //     } catch (error) {
    //         console.error(`Error accessing workspace with ID ${id}:`, error);
    //         throw error;
    //     }
    // }, []);
    //
    // const onEdit = useCallback(async (id, data) => {
    //     console.log(data)
    //     try {
    //         const response = await homeRepository.editWorkspace(id, data);
    //         fetchMyWorkspaces();
    //         return response.data;
    //     } catch (error) {
    //         console.error(`Error editing workspace with ID ${id}:`, error);
    //         throw error;
    //     }
    // }, [fetchMyWorkspaces]);

    useEffect(() => {
        fetchHonePage();
    }, [fetchHonePage]);

    return {
        ...state,
        refetch: fetchHonePage
        // fetchAll: fetchAllWorkspaces,
        // fetchMine: fetchMyWorkspaces,
        // onAccess: onAccess,
        // onEdit: onEdit,
    };
};

export default useHome;
