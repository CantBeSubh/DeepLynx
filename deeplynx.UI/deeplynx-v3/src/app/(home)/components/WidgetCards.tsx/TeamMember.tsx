import React, { useState, ReactNode } from "react";

const TeamMembersWidget = () => {
    const [showTable, setShowTable] = useState(false);

    const handleToggle = () => {
        setShowTable((prev) => !prev);
    };

    return (
        <div className="card-body">
            <div className="flex justify-between">
                <h2 className="card-title">Team Members</h2>
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
                <div className="caousel flex items-center round-box space-x-4 p-4">
                    <div className="flex items-center space-x-2">
                    <p className="text-base-300 mb-2"></p>
                    {[...Array(5)].map((_, i) => (
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
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="table">
                    {/* head */}
                        <thead>
                            <tr className="text-secondary-content">
                            <th className="pl-17">Name</th>
                            <th>Role</th>
                            <th>Last Login</th>
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
                                    <div className="">Natalie Hergesheimer</div>
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
      d="M12 4.5v15m7.5-7.5h-15"
    />
  </svg>
);

export default TeamMembersWidget;