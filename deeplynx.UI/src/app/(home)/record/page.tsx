// app/(home)/(routes)/data_catalog/record/page.tsx
import RecordViewClient from "./RecordViewClient";
import { getRecordServer } from "@/app/lib/record_services.server";

export const dynamic = "force-dynamic"; // optional if behind auth

type PageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

export default async function Page({ searchParams }: PageProps) {
  const params = await searchParams;
  const recordId = typeof params.recordId === "string" ? params.recordId : "";
  const projectId =
    typeof params.projectId === "string" ? params.projectId : "";

  if (!recordId || !projectId) {
    // you can render a simple error / notFound()
    return <div className="p-4">Missing recordId or projectId</div>;
  }

  return (
    <RecordViewClient
      projectId={Number(projectId)}
      recordId={Number(recordId)}
    />
  );
}
