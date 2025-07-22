import React, { useState } from "react";
import { ChevronDownIcon } from "@heroicons/react/24/outline";
import { PlusCircleIcon } from "@heroicons/react/24/solid";
import AvatarCarousel from "./WidgetCardModals/AvatarCarousel";
import { peopleData } from "@/app/(home)/dummy_data/data";
import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal";
import { Column, TeamMember } from "@/app/(home)/types/types";
import AvatarCell from "../Avatar";
import GenericTable from "../GenericTable";

const TeamMembersWidget: React.FC = () => {
    const [showTable, setShowTable] = useState(false);
    const [addMemberModal, setAddMemberModal] = useState(false);

    const handleToggle = () => {
        setShowTable((prev) => !prev);
    };

    const teamMemberColumns: Column<TeamMember>[] = [
        {
            header: "Name",
            data: "name",
            cell: (row) => (
                <div className="flex items-center gap-3">
                    <div className="avatar">
                        <div className="mask mask-circle h-10 w-10">
                            <AvatarCell name= {row.name} image= {row.image} />
                        </div>
                    </div>
                    <div>{row.name}</div>
                </div>
            ),
            sortable: true,
        },
        {
            header: "Role",
            data: "role",
            sortable: true,
        },
        {
            header: "Last Login",
            data: "lastLogin",
            sortable: true,
        },
    ];

    return (
        <div className="card-body">
            <div className="flex justify-between">
                <h2 className="card-title flex items-center">
                    Team Members
                    {showTable && (
                        <button onClick={() => setAddMemberModal(true)} className="ml-1">
                            <PlusCircleIcon className="w-7 h-7 text-secondary" />
                        </button>
                    )}
                </h2>
                <button onClick={handleToggle} className="btn btn-sm btn-ghost">
                    {showTable ? (
                        <ChevronDownIcon className="size-6 rotate-180" />
                    ) : (
                        <ChevronDownIcon className="size-6" />
                    )}
                </button>
            </div>

            {!showTable ? (
                <AvatarCarousel people={peopleData} />
            ) : (
                <GenericTable
                    columns={teamMemberColumns}
                    data={peopleData}
                    enablePagination
                    rowsPerPage={4}
                />
            )}

            <AddMember
                isOpen={addMemberModal}
                onClose={() => setAddMemberModal(false)}
            />
        </div>
    );
};

export default TeamMembersWidget;