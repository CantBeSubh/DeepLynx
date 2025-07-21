"use client";

import React, { useState } from "react";
import Image from "next/image";
import AddMember from "@/app/(home)/components/WidgetCards/WidgetCardModals/AddMemberModal";
import { ChevronRightIcon, ChevronLeftIcon } from "@heroicons/react/24/outline";
import { PlusCircleIcon } from "@heroicons/react/24/solid";

interface Person {
  id: number;
  name: string;
  image: string;
  nickname: string;
  visibility: string;
  role: string;
}
interface AvatarCarouselProps {
  avatars: string[];
}

const AvatarCarousel: React.FC<AvatarCarouselProps> = ({ avatars }) => {
  const [addMemberModal, setAddMemberModal] = useState(false);
  // Ratio to account for the size of the avatars, plus icon, and arrows
  const avatarsPerPage = Math.floor(
    (window.innerWidth * 0.3 - (24 + 24 + 24 + 32.5)) / 50
  );
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
  const endIdx = startIdx + avatarsPerPage - 1;
  const currentAvatars = avatars.slice(startIdx, endIdx);

  return (
    <div className="flex items-center justify-center w-full">
      <div className="flex items-center justify-between round-box pt-4 pb-4 w-full">
        <div className="flex items-center space-x-2 w-full">
          {/* Left Button */}
          <div className="flex items-center">
            <button onClick={handlePrev} className="flex-shrink-0">
              <ChevronLeftIcon className="size-6" />
            </button>
          </div>

          {/* Avatar Icons */}
          <div className="flex items-center justify-center space-x-3">
            <p className="text-base-300 mb-2"></p>
            {currentAvatars.map((avatar, i) => (
              <div key={i} className="avatar inline-block">
                <div className="w-12 rounded-full">
                  <Image src={avatar} alt="avatar" width="300" height="300" />
                </div>
              </div>
            ))}
            <button onClick={() => setAddMemberModal(true)}>
              <PlusCircleIcon className="w-10 h-10 text-secondary" />
            </button>
          </div>

          {/* Right Button */}
          <div className="flex items-center">
            <button onClick={handleNext} className="flex-shrink-0">
              <ChevronRightIcon className="size-6" />
            </button>
          </div>
        </div>
      </div>

      {/* Create Link Modal */}
      <AddMember
        isOpen={addMemberModal}
        onClose={() => setAddMemberModal(false)}
      />
    </div>
  );
};

export default AvatarCarousel;
