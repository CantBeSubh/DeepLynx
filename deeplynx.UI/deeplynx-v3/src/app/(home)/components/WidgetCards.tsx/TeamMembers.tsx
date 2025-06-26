import React, { useState, ReactNode } from "react";

// const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
// const toggleRow = (index: number) => {
//     setExpandedIndex(expandedIndex === index ? null : index);
//   };

const TeamMembersWidget = () => {
  return (
    <div className="card-body">
      <h2 className="card-title">Team Members</h2>
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

export default TeamMembersWidget;