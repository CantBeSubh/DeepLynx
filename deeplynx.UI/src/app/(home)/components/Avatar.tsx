"use client";

import React, { useState } from "react";

interface AvatarCellProps {
  name?: string;
  image?: string;
  showName?: boolean;
  size?: number; // Tailwind spacing unit (e.g. 10 = w-10 h-10)
}

const AvatarCell: React.FC<AvatarCellProps> = ({
  name,
  image,
  showName,
  size = 10, // default
}) => {
  const [imgError, setImgError] = useState(false);

  const getInitials = (fullName?: string) => {
    if (!fullName) return "?";
    const parts = fullName.trim().split(/\s+/);

    if (parts.length === 1) {
      return parts[0][0]?.toUpperCase() ?? "?";
    }

    const firstInitial = parts[0][0]?.toUpperCase() ?? "";
    const lastInitial = parts[parts.length - 1][0]?.toUpperCase() ?? "";
    return `${firstInitial}${lastInitial}`;
  };

  const fallbackInitials = getInitials(name);

  // Tailwind classes built dynamically
  const sizeClass = `w-${size} h-${size}`;
  const textSizeClass = size >= 12 ? "text-xl" : "text-lg";

  return (
    <div className="flex items-center space-x-3">
      {imgError || !image ? (
        <div
          className={`${sizeClass} rounded-full bg-secondary text-primary-content flex items-center justify-center font-bold ${textSizeClass}`}
        >
          {fallbackInitials}
        </div>
      ) : (
        <div className="avatar">
          <div className={`${sizeClass} rounded-full`}>
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={image} alt={name} onError={() => setImgError(true)} />
          </div>
        </div>
      )}
      {showName && <p>{name}</p>}
    </div>
  );
};

export default AvatarCell;
