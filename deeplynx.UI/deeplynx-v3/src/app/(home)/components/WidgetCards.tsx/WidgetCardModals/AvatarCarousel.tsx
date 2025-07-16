import React, { useState } from 'react';
import {ChevronDownIcon, ChevronRightIcon, ChevronLeftIcon, PlusCircleIcon} from "@heroicons/react/24/outline";

interface AvatarCarouselProps {
    avatars: string[];
}

const AvatarCarousel: React.FC<AvatarCarouselProps> = ({ avatars }) => {
    const avatarsPerPage = 9;
    // const totalAvatars = 30;
    const [currentPage, setCurrentPage] = useState(0);

    const totalPages = Math.ceil(avatars.length / avatarsPerPage);

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

    const startIdx = currentPage * avatarsPerPage;
    const endIdx = startIdx + avatarsPerPage;
    const currentAvatars = avatars.slice(startIdx, endIdx);

    return (
        <div className="flex items-center justify-center w-full">
            <div className="flex items-center justify-between round-box pt-4 pb-4 w-full">
                <div className="flex items-center space-x-2 w-full">
                    {/* Left Button */}
                    <div className="flex items-center">
                        <button onClick={handlePrev} className="flex-shrink-0">
                            <ChevronLeftIcon className="size-6 mr-4" />
                        </button>
                    </div>

                    {/* Avatar Icons */}
                    <div className="flex items-center justify-center space-x-3">
                        <p className="text-base-300 mb-2"></p>
                        {currentAvatars.map((avatar, i) => (
                            <div key={i} className="avatar inline-block">
                                <div className="w-12 rounded-full">
                                    <img
                                        src={avatar}
                                        alt="avatar"
                                    />
                                </div>
                            </div>
                        ))}
                        <button className="">
                            <PlusIcon />
                        </button>
                    </div>

                    {/* Right Button */}
                    <div className="flex items-center">
                        <button onClick={handleNext} className="flex-shrink-0">
                            <ChevronRightIcon className="size-6 ml-4" />
                        </button>
                    </div>
                </div>
            </div>
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
    className="w-9 h-9 rounded-full"
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

export default AvatarCarousel;