"use client";

import { useState } from "react";
import { Tag } from "../../types/types";

const ExpandableTagsCell = ({ tags }: { tags: Tag[] }) => {
  const [expanded, setExpanded] = useState(false);
  if (!Array.isArray(tags)) return null;
  const tagsToShow = expanded ? tags : tags.slice(0, 3);

  return (
    <div className="flex flex-wrap gap-1">
      {tagsToShow.map((tag, i) => (
        <span key={i} className="badge text-sm badge-outline">
          {tag.name}
        </span>
      ))}
      {tags.length > 3 && !expanded && (
        <button
          className="text-sm badge badge-secondary badge-outline cursor-pointer text-secondary ml-2"
          onClick={() => setExpanded(true)}
        >
          See more
        </button>
      )}
    </div>
  );
};

export default ExpandableTagsCell;
