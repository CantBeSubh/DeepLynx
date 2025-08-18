"use client";

import { useState } from "react";
import { Tag } from "../types/types";
import { translations } from "@/app/lib/translations";
import React from "react";

const ExpandableTagsCell = ({ tags }: { tags: Tag[] }) => {
  const locale = "en";
  const t = translations[locale];
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
          {t.translations.SEE_MORE}
        </button>
      )}
    </div>
  );
};

export default ExpandableTagsCell;
