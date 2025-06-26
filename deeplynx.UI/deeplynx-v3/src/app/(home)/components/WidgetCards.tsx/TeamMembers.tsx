import React, { useState, ReactNode } from "react";

// const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
// const toggleRow = (index: number) => {
//     setExpandedIndex(expandedIndex === index ? null : index);
//   };

const TeamMembersWidget = () => {
  return (
    <div className="card-body">
        <div className="flex justify-between">
            <h2 className="card-title">Team Members</h2>
            <button
                onClick={() => toggleRow(index)}
                aria-label="Expand row"
            >
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
            </button>
        </div>
      <div className="space-x-2">
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
      </div>
    </div>
  );
};

const PlusIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="none"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="currentColor"
    className="size-5"
  >
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 4.5v15m7.5-7.5h-15"
    />
  </svg>
);

export default TeamMembersWidget;