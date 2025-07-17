import React, { useState } from "react";
import Image from 'next/image';
import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal"
import {ChevronDownIcon, ChevronRightIcon, ChevronLeftIcon} from "@heroicons/react/24/outline";
import {PlusCircleIcon} from "@heroicons/react/24/solid";
import AvatarCarousel from "./WidgetCardModals/AvatarCarousel";

const TeamMembersWidget: React.FC = () => {
    const [showTable, setShowTable] = useState(false);
    const [addMemberModal, setAddMemberModal] = useState(false);
    const avatars = Array.from({ length: 30 }, (_, i) => `https://i.pravatar.cc/150?img=${i + 1}`);

    const handleToggle = () => {
        setShowTable((prev) => !prev);
    };

    return (
        <div className="card-body">
            <div className="flex justify-between">
                <h2 className="card-title flex items-center">
                    Team Members
                    {/* Show Plus Icon only when table is shown */}
                    {showTable && (
                        <button onClick={() => setAddMemberModal(true)} className="ml-1">
                            <PlusCircleIcon
                                className="w-7 h-7 text-secondary" />
                        </button>
                    )}
                </h2>
                <button onClick={handleToggle} className="btn btn-sm btn-ghost">
                    {showTable ? (
                        <ChevronDownIcon
                            className="size-6 rotate-180"
                        />
                    ) : (
                        <ChevronDownIcon
                            className="size-6"
                        />
                    )}
                </button>
            </div>

            {!showTable ? (
                <AvatarCarousel avatars={avatars} />
            ) : (
                // Team Members Table
                <div className="overflow-x-auto">
                    <div className="flex justify-between items-center">
                    </div>
                    <table className="table">
                    {/* head */}
                        <thead>
                            <tr className="text-secondary-content">
                            <th className="pl-17 flex items-center">
                                Name
                                <div className="pl-2">
                                    <ChevronDownIcon
                                        className="size-5"
                                    />
                                </div>
                            </th>
                            <th>Role</th>
                            <th className="flex items-center">
                                Last Login
                                <div className="pl-2">
                                    <ChevronDownIcon
                                        className="size-5"
                                    />
                                </div>
                            </th>
                            <th></th>
                            </tr>
                        </thead>
                    <tbody>
                        {/* row 1 */}
                        <tr>
                            <td>
                                <div className="flex items-center gap-3">
                                <div className="avatar">
                                    <div className="mask mask-circle h-10 w-10">
                                    <Image
                                        src="https://img.daisyui.com/images/profile/demo/2@94.webp"
                                        alt="Avatar Tailwind CSS Component" />
                                    </div>
                                </div>
                                <div>
                                    <div className="">Jaren Brownlee</div>
                                </div>
                                </div>
                            </td>
                        <td>
                            Admin
                            <br />
                        </td>
                        <td>2025-06-30T14:48:00</td>
                        </tr>
                        {/* row 2 */}
                        <tr>
                            <td>
                                <div className="flex items-center gap-3">
                                <div className="avatar">
                                    <div className="mask mask-circle h-10 w-10">
                                    <Image
                                        src="https://img.daisyui.com/images/profile/demo/3@94.webp"
                                        alt="Avatar Tailwind CSS Component" />
                                    </div>
                                </div>
                                <div>
                                    <div className="">Autumn Combs</div>
                                </div>
                                </div>
                            </td>
                        <td>
                            Editor
                            <br />
                        </td>
                        <td>2025-06-30T14:48:00</td>
                        </tr>
                        {/* row 3 */}
                        <tr>
                            <td>
                                <div className="flex items-center gap-3">
                                    <div className="avatar">
                                        <div className="mask mask-circle h-10 w-10">
                                            <Image
                                                src="https://img.daisyui.com/images/profile/demo/4@94.webp"
                                                alt="Avatar Tailwind CSS Component" />
                                        </div>
                                    </div>
                                    <div>
                                        <div className="">Jason Kuipers</div>
                                    </div>
                                </div>
                            </td>
                        <td>
                            Developer
                            <br />
                        </td>
                        <td>2025-06-29T14:48:00</td>
                        </tr>
                        {/* row 4 */}
                        <tr>
                            <td>
                                <div className="flex items-center gap-3">
                                    <div className="avatar">
                                        <div className="mask mask-circle h-10 w-10">
                                            <Image
                                                src="https://img.daisyui.com/images/profile/demo/5@94.webp"
                                                alt="Avatar Tailwind CSS Component" />
                                        </div>
                                    </div>
                                    <div>
                                        <div className="">Isaac Huffman</div>
                                    </div>
                                </div>
                            </td>
                        <td>
                            Viewer
                            <br />
                        </td>
                        <td>2025-06-27T10:00:00</td>
                        </tr>
                    </tbody>
                    </table>

                    {/* Table Toggle Buttons */}
                    <div className="flex justify-end">
                        <button className="btn join-item p-2 rounded-r-none">
                            <ChevronLeftIcon
                                className="size-6" />
                        </button>
                        <button className="btn join-item p-2 rounded-l-none">
                            <ChevronRightIcon
                                className="size-6" />
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