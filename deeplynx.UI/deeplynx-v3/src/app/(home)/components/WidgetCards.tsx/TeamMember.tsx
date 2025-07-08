import React, { useState, ReactNode } from "react";

const TeamMembersWidget = () => {
    const [showTable, setShowTable] = useState(false);

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
                        <button className="ml-1">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                fill="oklch(44.08% 0.141 255.19)"
                                viewBox="0 0 24 24"
                                strokeWidth={1.5}
                                stroke="white"
                                className="w-7 h-7 rounded-full"
                            >
                                <circle
                                    cx="12"
                                    cy="12"
                                    r="12"
                                    fill="btn-secondary"
                                />
                                <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                d="M12 8v8m4-4h-8"
                                />
                            </svg>
                        </button>
                    )}
                </h2>
                <button onClick={handleToggle} className="btn btn-sm btn-ghost">
                    {showTable ? (
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            fill="none"
                            viewBox="0 0 24 24"
                            strokeWidth={1.5}
                            stroke="currentColor"
                            className="size-6 rotate-180"
                            >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                d="m19.5 8.25-7.5 7.5-7.5-7.5"
                            />
                        </svg>
                    ) : (
                        <svg
                            xmlns="http://www.w3.org/2000/svg"
                            fill="none"
                            viewBox="0 0 24 24"
                            strokeWidth={1.5}
                            stroke="currentColor"
                            className="size-6"
                            >
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                d="m19.5 8.25-7.5 7.5-7.5-7.5"
                            />
                        </svg>
                    )}
                </button>
            </div>

            {!showTable ? (
                <div className="flex items-center">
                    <div className="flex justify-between round-box space-x-4 pt-4 pb-4">
                        <div className="flex items-center space-x-2">

                            {/* Left Button */}
                            <div className="flex items-center justify-cetner">
                                <button className="flex-shrink-0">
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                        strokeWidth="1.5"
                                        stroke="currentColor"
                                        className="size-6">
                                        <path
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            d="M15.75 19.5 8.25 12l7.5-7.5"
                                        />
                                    </svg>
                                </button>
                            </div>

                            {/* Avatar Icons */}
                            <div className="flex-grow flex items-center space-x-2 overflow-x-auto">
                                <p className="text-base-300 mb-2"></p>
                                {[...Array(8)].map((_, i) => (
                                    <div key={i} className="avatar inline-block">
                                        <div className="w-10 rounded-full">
                                        <img
                                            src={`https://i.pravatar.cc/150?img=${i + 1}`}
                                            alt="avatar"
                                        />
                                        </div>
                                    </div>
                                ))}
                                <PlusIcon />
                            </div>

                            {/* Right Button */}
                            <div className="flex items-center justify-center">
                                <button className="flex-shrink-0">
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none" viewBox="0 0 24 24"
                                        strokeWidth="1.5"
                                        stroke="currentColor"
                                        className="size-6">
                                        <path
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            d="m8.25 4.5 7.5 7.5-7.5 7.5"
                                        />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            ) : (
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
                                    <DownArrow />
                                </div>
                            </th>
                            <th>Role</th>
                            <th className="flex items-center">
                                Last Login
                                <div className="pl-2">
                                    <DownArrow />
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
                                    <img
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
                                    <img
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
                                            <img
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
                                            <img
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
                    <div className="flex justify-end">
                        <button className="btn join-item p-2 rounded-r-none">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 24 24"
                                strokeWidth="2"
                                stroke="currentColor"
                                className="w-5 h-5">
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    d="M15 19l-7-7 7-7"
                                />
                            </svg>
                        </button>
                        <button className="btn join-item p-2 rounded-l-none">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 24 24"
                                strokeWidth="2"
                                stroke="currentColor"
                                className="w-5 h-5">
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    d="M9 5l7 7-7 7"
                                />
                            </svg>
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

const PlusIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="oklch(44.08% 0.141 255.19)"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="white"
    className="w-8 h-8 rounded-full"
  >
    <circle
        cx="12"
        cy="12"
        r="12"
        fill="btn-secondary"
    />
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 8v8m4-4h-8"
    />
  </svg>
);

const DownArrow = () => (
    <svg
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        strokeWidth={2}
        stroke="currentColor"
        className="size-4"
        >
        <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="m19.5 8.25-7.5 7.5-7.5-7.5"
        />
    </svg>
);

export default TeamMembersWidget;