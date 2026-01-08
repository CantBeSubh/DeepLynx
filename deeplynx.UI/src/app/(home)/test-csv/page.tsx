"use client";

import CsvTemplateDownload from "../components/CsvTemplateDownload";

export default function TestCsvPage() {
  return (
    <div className="p-10">
      <h1 className="text-2xl font-bold mb-4">Test CSV Template Download</h1>
      <p className="mb-4 text-base-content/70">
        Click the button below to download a sample CSV template.
      </p>
      <CsvTemplateDownload />
    </div>
  );
}
