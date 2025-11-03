import { MagnifyingGlassIcon, XMarkIcon } from "@heroicons/react/24/outline";
import React from "react";

interface SimpleFilterInputProps {
  placeholder?: string;
  value: string;
  onChange: (value: string) => void;
  className?: string;
}

const SimpleFilterInput: React.FC<SimpleFilterInputProps> = ({
  placeholder = "Filter...",
  value,
  onChange,
  className = "",
}) => {
  const handleClear = () => {
    onChange("");
  };

  return (
    <div className={`relative ${className}`}>
      <MagnifyingGlassIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-base-content/50" />

      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full pl-10 pr-10 py-2 rounded-lg border border-base-300 bg-base-100 focus:outline-none focus:ring-2 focus:ring-primary text-base-content"
      />

      {value && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-base-content/50 hover:text-base-content"
          aria-label="Clear"
        >
          <XMarkIcon className="w-5 h-5" />
        </button>
      )}
    </div>
  );
};

export default SimpleFilterInput;
