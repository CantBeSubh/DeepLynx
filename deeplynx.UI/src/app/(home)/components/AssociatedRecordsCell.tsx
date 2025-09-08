"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useState } from "react";

const AssociatedRecordsCell = ({
  records,
}: {
  records: string[] | undefined;
}) => {
  const { t } = useLanguage();
  const [expanded, setExpanded] = useState(false);
  const recordsToShow = expanded ? records : records?.slice(0, 3) || [];

  return (
    <div className="flex flex-col gap-2">
      {recordsToShow?.map((rec, i) => (
        <a key={i} className="text-blue-600 underline text-sm">
          {rec}
        </a>
      ))}
      {records && records.length > 3 && !expanded && (
        <button
          className="text-sm flex badge text-blue-600 ml-2"
          onClick={() => setExpanded(true)}
        >
          {t.translations.SEE_MORE}
        </button>
      )}
    </div>
  );
};

export default AssociatedRecordsCell;
