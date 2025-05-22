import React, { useRef, useState } from "react";

interface LargeSearchBarProps {
  placeholder?: string;
  className?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onEnter?: (value: string) => void;
  value?: string;
}

const LargeSearchBar: React.FC<LargeSearchBarProps> = ({
  placeholder = "Search...",
  className = "",
  onChange,
  onEnter,
  value,
}) => {
  const [internalValue, setInternalValue] = useState<string>("");
  const inputRef = useRef<HTMLInputElement>(null);

  const isControlled = value !== undefined;
  const currentValue = isControlled ? value : internalValue;

  const handleClear = () => {
    if (!isControlled) {
      setInternalValue("");
    }
    if (onChange) {
      const event = {
        target: { value: "" },
      } as React.ChangeEvent<HTMLInputElement>;
      onChange(event);
    }
    if (inputRef.current) {
      inputRef.current.focus();
    }
  };

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
        onKeyDown={(e) => {
          if (e.key === "Enter" && onEnter) {
            onEnter((e.target as HTMLInputElement).value);
          }
        }}
        value={value}
      />
      {/* Clear button - only show when there's text */}
      {currentValue && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-4 top-1/2 transform -translate-y-1/2 text-base-content opacity-70 hover:opacity-100"
          aria-label="Clear search"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
      )}
    </div>
  );
};

export default LargeSearchBar;
