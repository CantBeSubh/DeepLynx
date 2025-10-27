"use client";

import React, { useState, useEffect } from "react";
import GenericTable from "@/app/(home)/components/GenericTable";
import { Column } from "@/app/(home)/types/types";
import {
    getAllEventsPaginated,
    EventFilterParams,
} from "@/app/lib/event_services.client";
import { EventResponseDto, PaginatedEventsResponse } from "../types/responseDTOs"

const EventsTable = () => {
    const [data, setData] = useState<EventResponseDto[]>([]);
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 10,
        totalCount: 0,
    });
    const [loading, setLoading] = useState(false);
    const [filters, setFilters] = useState<EventFilterParams>({});

    // Fetch events from backend
    const fetchEvents = async (pageNumber: number, pageSize: number) => {
        setLoading(true);
        try {
            const result: PaginatedEventsResponse = await getAllEventsPaginated({
                pageNumber,
                pageSize,
                ...filters,
            });

            setData(result.items);
            setPagination({
                pageNumber: result.pageNumber,
                pageSize: result.pageSize,
                totalCount: result.totalCount,
            });
        } catch (error) {
            console.error("Failed to fetch events:", error);
            setData([]);
            setPagination({
                pageNumber: 1,
                pageSize: 10,
                totalCount: 0,
            });
        } finally {
            setLoading(false);
        }
    };

    // Initial fetch
    useEffect(() => {
        fetchEvents(1, 10);
    }, []);

    // Handle page changes
    const handlePageChange = (pageNumber: number) => {
        fetchEvents(pageNumber, pagination.pageSize);
    };

    // Handle page size changes (optional)
    const handlePageSizeChange = (pageSize: number) => {
        fetchEvents(1, pageSize);
    };

    // Define columns
    const columns: Column<EventResponseDto>[] = [
        {
            header: "ID",
            data: "id",
            sortable: false,
        },
        {
            header: "Operation",
            data: "operation",
            sortable: false,
        },
        {
            header: "Entity Type",
            data: "entityType",
            sortable: false,
        },
        {
            header: "Entity Name",
            data: "entityName",
            sortable: false,
        },
        {
            header: "Project",
            data: "projectName",
            sortable: false,
        },
        {
            header: "Data Source",
            data: "dataSourceName",
            sortable: false,
        },
        {
            header: "Updated By",
            data: "lastUpdatedBy",
            sortable: false,
        },
        {
            header: "Updated At",
            data: "lastUpdatedAt",
            sortable: false,
            cell: (row) => {
                const date = new Date(row.lastUpdatedAt);
                return date.toLocaleString();
            },
        },
    ];

    return (
        <div className="px-8">
            {loading && (
                <div className="text-center py-4 text-base-content">
                    Loading events...
                </div>
            )}
            <GenericTable
                columns={columns}
                data={data}
                title="Events"
                enablePagination={true}
                backendPagination={true}
                paginationMetadata={pagination}
                onPageChange={handlePageChange}
                onPageSizeChange={handlePageSizeChange}
                bordered={true}
                rowsPerPage={10}
            />
        </div>
    );
};

export default EventsTable;