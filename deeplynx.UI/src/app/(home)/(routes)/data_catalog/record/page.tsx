// app/(home)/(routes)/data_catalog/record/page.tsx
import { notFound } from "next/navigation";
import RecordViewClient from "./RecordViewClient";
import { getRecord } from "@/app/lib/record_services"; // server-only

type PageProps = {
  searchParams: Promise<{ recordId?: string; projectId?: string }>;
};

export default async function Page({ searchParams }: PageProps) {
  const params = await searchParams;
  const recordId = params.recordId ? Number(params.recordId) : NaN;
  const projectId = params.projectId ? Number(params.projectId) : NaN;

  if (!Number.isFinite(recordId) || !Number.isFinite(projectId)) {
    return notFound();
  }

  const record = await getRecord(projectId, recordId);
  if (!record) {
    return notFound();
  }

  await new Promise((r) => setTimeout(r, 1200));
  return (
    <RecordViewClient
      initialRecord={record}
      projectId={projectId}
      recordId={recordId}
    />
  );
}
