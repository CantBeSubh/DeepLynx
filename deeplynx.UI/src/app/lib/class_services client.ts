'use client';

import { ClassResponseDto } from '../(home)/types/types';
import api from './api';

export const getClass = async (projectId: number, classId: number) => {
    try {
        const res = await api.get(`/projects/${projectId}/classes/GetClass/${classId}`);
        return res.data;
    } catch (error) {
        console.error("Error getting all tags:", error);
        throw error;
    }
}

