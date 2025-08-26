"use client";

import { translations } from "@/app/lib/translations";
import { MagnifyingGlassIcon, XMarkIcon } from "@heroicons/react/24/outline";
import React, { useRef, useState } from "react";

// Define the props for the translations component
interface Filter {
  id: number;
  term: string;
}
interface LargeSearchBarProps {
  placeholder?: string;
  className?: string;
  onChange?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onEnter?: (value: string) => void;
  value?: string;
  activeFilters?: Filter[];
  onRemoveFilter?: (id: number) => void;
  onClearAll?: () => void;
  resultCount?: number;
  showResultsMessage?: boolean;
}

const LargeSearchBar: React.FC<LargeSearchBarProps> = ({
  placeholder = "Search...", // Default placeholder text
  className = "", // Default className
  onChange,
  onEnter,
  value,
  activeFilters = [],
  onRemoveFilter,
  onClearAll,
  resultCount,
  showResultsMessage,
}) => {
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];
  const [internalValue, setInternalValue] = useState<string>("");
  const inputRef = useRef<HTMLInputElement>(null);

  // Check if the component is controlled or uncontrolled
  const isControlled = value !== undefined && onChange !== undefined;
  const currentValue = isControlled ? value : internalValue;

  // Clear the input field
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
    <div className={`${className}`}>
      <div className="relative">
        <MagnifyingGlassIcon className="absolute left-4 top-5 transform -translate-y-1/2 w-5 h-5 text-neutral size-6" />
        <input
          type="text"
          placeholder={placeholder}
          className="w-full pl-12 pr-4 py-2 rounded-full border border-base-300 bg-base-100 shadow-sm focus:outline-none focus:ring-2 focus:ring-primary text-neutral"
          onChange={
            isControlled ? onChange : (e) => setInternalValue(e.target.value)
          }
          onKeyDown={(e) => {
            if (e.key === "Enter" && onEnter) {
              onEnter((e.target as HTMLInputElement).value);
            }
          }}
          value={currentValue}
          ref={inputRef}
        />
        {/* Clear button - only show when there's text */}
        {currentValue && (
          <button
            type="button"
            onClick={handleClear}
            className="absolute right-4 top-5 transform -translate-y-1/2 text-base-content opacity-70 hover:opacity-100"
            aria-label="Clear search"
          >
            <XMarkIcon className="size-6" />
          </button>
        )}
        <div className="text-right mt-1">
          <a
            href="/data_catalog/query_builder"
            className="text-sm underline text-secondary hover:underline"
          >
            {t.translations.ADITIONAL_FILTERS}
          </a>
        </div>
      </div>

      {/* Filter Chips */}
      {activeFilters.length > 0 && (
        <div className="flex flex-wrap items-center gap-2 mt-3 ml-1">
          {activeFilters.map((filter, index) => (
            <div
              key={index}
              className="bg-base-200 rounded-full px-3 py-1 flex items-center gap-2 text-sm"
            >
              <span>Filtered by: {filter.term}</span>
              {onRemoveFilter && (
                <button
                  className="hover:text-error"
                  onClick={() => onRemoveFilter(filter.id)}
                >
                  <XMarkIcon className="size-4" />
                </button>
              )}
            </div>
          ))}
          {activeFilters.length > 1 && onClearAll && (
            <button
              className="text-sm hover:underline ml-2"
              onClick={onClearAll}
            >
              {t.translations.CLEAR_ALL}
            </button>
          )}
        </div>
      )}
      {/* Results message */}
      {showResultsMessage && (
        <div className="mt-4 ml-1">
          {activeFilters.length > 0 && resultCount === 0 ? (
            <p>{t.translations.NO_RESULTS_FOUND}</p>
          ) : (
            <div className="border-b border-base-200">
              <h2>
                {t.translations.FOUND} {resultCount} {t.translations.MATCHES}
              </h2>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default LargeSearchBar;
