import React from "react";

interface LargeSearchBarProps {
  placeholder?: string;
  className?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const LargeSearchBar: React.FC<LargeSearchBarProps> = ({
  placeholder = "Search...",
  className = "",
  onChange,
}) => {
  return (
    <div className={`relative w-full max-w-3xl ${className}`}>
      <svg
        className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-base-content"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth="2"
          d="M21 21l-4.35-4.35m1.35-5.15a7 7 0 1 1-14 0 7 7 0 0 1 14 0z"
        />
      </svg>
      <input
        type="text"
        placeholder={placeholder}
        className="w-full pl-12 pr-4 py-2 rounded-full border border-base-300 bg-base-100 shadow-sm focus:outline-none focus:ring-2 focus:ring-primary"
        onChange={onChange}
      />
    </div>
  );
};

export default LargeSearchBar;
