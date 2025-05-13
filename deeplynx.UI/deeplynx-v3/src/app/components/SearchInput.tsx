import React from "react";

interface SearchInputProps {
  placeholder?: string;
  className?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

const SearchInput: React.FC<SearchInputProps> = ({
  placeholder,
  className,
  onChange,
}: SearchInputProps) => {
  return (
    <label
      className={`input flex items-center relative ml-2 bg-base-200 ${className}`}
    >
      <input
        type="text"
        placeholder={placeholder}
        className="input pl-10 focus:outline-none"
        onChange={onChange}
      />
      <svg
        className="absolute left-3 top-2.5 w-5 h-5 text-base-content"
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
    </label>
  );
};

export default SearchInput;
