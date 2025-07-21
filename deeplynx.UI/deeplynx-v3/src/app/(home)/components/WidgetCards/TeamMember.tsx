import React, { useState } from "react";
import Image from 'next/image';
import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal";
import { ChevronDownIcon, ChevronRightIcon, ChevronLeftIcon } from "@heroicons/react/24/outline";
import { PlusCircleIcon } from "@heroicons/react/24/solid";
import AvatarCarousel from "./WidgetCardModals/AvatarCarousel";
import { peopleData } from "@/app/(home)/dummy_data/data";

const TeamMembersWidget: React.FC = () => {
    const [showTable, setShowTable] = useState(false);
    const [addMemberModal, setAddMemberModal] = useState(false);
    const [currentPage, setCurrentPage] = useState(0);
    const usersPerPage = 4;

    const handleToggle = () => {
        setShowTable((prev) => !prev);
    };

    const totalPages = Math.ceil(peopleData.length / usersPerPage);

    const handleNext = () => {
        if (currentPage < totalPages - 1) {
            setCurrentPage(currentPage + 1);
        }
    };

    const handlePrev = () => {
        if (currentPage > 0) {
            setCurrentPage(currentPage - 1);
        }
    };

    // Calculate the start and end index for the current page
    const startIdx = currentPage * usersPerPage;
    const endIdx = startIdx + usersPerPage;
    const currentUsers = peopleData.slice(startIdx, endIdx);

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
                // Team Member Table
                <div className="overflow-x-auto">
                    <table className="table">
                        <thead>
                            <tr className="text-secondary-content">
                                <th className="pl-17 flex items-center">
                                    Name
                                    <div className="pl-2">
                                        <ChevronDownIcon className="size-5" />
                                    </div>
                                </th>
                                <th>Role</th>
                                <th className="flex items-center">
                                    Last Login
                                    <div className="pl-2">
                                        <ChevronDownIcon className="size-5" />
                                    </div>
                                </th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {currentUsers.map(person => (
                                <tr key={person.id}>
                                    <td>
                                        <div className="flex items-center gap-3">
                                            <div className="avatar">
                                                <div className="mask mask-circle h-10 w-10">
                                                    <Image
                                                        src={person.image}
                                                        alt={`${person.name} avatar`}
                                                        width="300"
                                                        height="300"
                                                    />
                                                </div>
                                            </div>
                                            <div>
                                                <div className="">{person.name}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td>
                                        <div>
                                            <div className="">{person.role}</div>
                                        </div>
                                        <br />
                                    </td>
                                    <td>2025-06-30T14:48:00</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    {/* Table Toggle Buttons*/}
                    <div className="flex justify-end">
                        <button
                            className="btn join-item p-2 rounded-r-none"
                            onClick={handlePrev}
                            disabled={currentPage === 0}
                        >
                            <ChevronLeftIcon className="size-6" />
                        </button>
                        <button
                            className="btn join-item p-2 rounded-l-none"
                            onClick={handleNext}
                            disabled={currentPage === totalPages- 1}
                        >
                            <ChevronRightIcon className="size-6" />
                        </button>
                    </div>
                </div>
            )}

            {/* Create Link Modal */}
            <AddMember
                isOpen={addMemberModal}
                onClose={() => setAddMemberModal(false)}
            />
        </div>
    );
};

export default TeamMembersWidget;