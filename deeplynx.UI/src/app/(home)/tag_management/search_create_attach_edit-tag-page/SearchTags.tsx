import React, { useState } from "react";
import SimpleFilterInput from "../../components/SimpleFilterComponent";
import { TagResponseDto } from "../../types/responseDTOs";

interface Props {
  loading: boolean;
  error: string | null;
  filteredTags: TagResponseDto[];
  tags: TagResponseDto[];
}

const SearchTags = ({ loading, error, filteredTags, tags }: Props) => {
  const [searchQuery, setSearchQuery] = useState("");
  return (
    <div>
      <h3 className="font-bold mb-4">Search Tags</h3>
      <SimpleFilterInput
        placeholder="Filter tags..."
        value={searchQuery}
        onChange={setSearchQuery}
      />
      <div className="mt-4">
        {loading && <p>Loading tags ...</p>}
        {error && <p className="text-error flex justify-center">{error}</p>}
        {!loading && filteredTags.length === 0 && tags.length === 0 && (
          <p className="text-base-300">No Tags found</p>
        )}
        {!loading && filteredTags.length === 0 && tags.length > 0 && (
          <p className="text-base-300">No tags match your search</p>
        )}
        {!loading && filteredTags.length > 0 && (
          <div className="space-y-2">
            <p className="text-sm font-semibold">
              Tags ({filteredTags.length}):
            </p>
            <ul className="space-y-1">
              {filteredTags.map((tag, index) => (
                <li key={tag.id || index} className="px-3 py-1">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-primary"
                  />
                  <span className="badge ml-2">
                    {tag.name || JSON.stringify(tag)}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchTags;
