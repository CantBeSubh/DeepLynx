import { MagnifyingGlassIcon } from "@heroicons/react/24/outline";
import React from "react";

// Define the props for the SearchInput component
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
      {/* Input field */}
      <input
        type="text"
        placeholder={placeholder}
        className="input pl-10 focus:outline-none text-secondary-content"
        onChange={onChange}
      />
      {/* Search icon */}
      <MagnifyingGlassIcon className="absolute left-3 top-2.5 w-5 h-5 text-base-content size-6" />
    </label>
  );
};

export default SearchInput;
