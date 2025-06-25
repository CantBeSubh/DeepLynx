"use client";

import React, { useState } from "react";

interface AvatarCellProps {
  name: string;
  image?: string;
}

const AvatarCell: React.FC<AvatarCellProps> = ({ name, image }) => {
  const [imgError, setImgError] = useState(false);
  const fallbackLetter = name.charAt(0).toUpperCase();

  return (
    <div className="flex items-center space-x-3">
      {imgError || !image ? (
        <div className="w-10 h-10 rounded-full bg-secondary text-primary-content flex items-center justify-center font-bold text-lg">
          {fallbackLetter}
        </div>
      ) : (
        <div className="avatar">
          <div className="w-10 rounded-full">
            <img src={image} alt={name} onError={() => setImgError(true)} />
          </div>
        </div>
      )}
      <p>{name}</p>
    </div>
  );
};

export default AvatarCell;
