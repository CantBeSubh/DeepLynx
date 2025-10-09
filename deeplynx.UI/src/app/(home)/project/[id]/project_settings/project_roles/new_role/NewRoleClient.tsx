"use client";

import React, { useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import AddRole from "../../../../../components/ProjectSettingsTable/ProjectTables/AddRole";
import { RoleResponseDto, PermissionResponseDto } from "../../../../../types/types";

type Props = {
    projectId: number;
};

export default function NewRoleClient({ projectId }: Props) {
    const { t } = useLanguage();
    const [role, setRole] = useState<RoleResponseDto>({
        id: 0,
        name: '',
        description: '',
        lastUpdatedAt: '',
        lastUpdatedBy: '',
        projectId: projectId,
    });
    const [permissions, setPermissions] = useState<PermissionResponseDto[]>([]);

    const handleSaveRole = () => {
        // Logic to save the role
        console.log("Role Saved:", role);
        // You might want to call a service here to actually create the role
    };

    const handleCancel = () => {
        // Logic to handle cancellation
        console.log("Role creation canceled");
    };

    return (
        <div>
            <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
                <div>
                    <h1 className="text-2xl font-bold text-info-content">
                        {t.translations.ADD_NEW_ROLE}
                    </h1>
                </div>
            </div>

            <div className="flex w-full gap-8 p-8">
                <div className="w-full">
                    <AddRole
                        role={role}
                        setRole={setRole}
                        permissions={permissions}
                        setPermissions={setPermissions}
                        onCancel={handleCancel}
                        onSave={handleSaveRole}
                    />
                </div>
            </div>
        </div>
    );
};