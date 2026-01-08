"use client";

import CsvTemplateDownload from "./components/CsvTemplateDownload";

export default function TestCsvTemplate() {
  return (
    <div className="p-10">
      <h1 className="text-2xl font-bold mb-4">Test CSV Template Download</h1>
      <CsvTemplateDownload />
    </div>
  );
}
