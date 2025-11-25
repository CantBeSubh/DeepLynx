// src/app/lib/file_upload_services.client.ts

import { RecordResponseDto } from "@/app/(home)/types/responseDTOs";
import { UploadFileArgs } from "@/app/(home)/types/types";
import api from "./api";



export async function uploadFile({
  organizationId,
  projectId,
  file,
  dataSourceId,      // optional per docs
  objectStorageId,   // optional per docs
  name,
  description,
  properties,        // object or string? if object, we stringify below
  tags,              // array? we stringify below unless your API expects tags[i].name format
  originalId,
  classId,
}: UploadFileArgs) {
  if (!organizationId || !projectId || !file) {
    throw new Error("organizationId, projectId, and file are required");
  }

  const form = new FormData();
  // include filename for broader adapter compatibility
  form.append("file", file as any, (file as any).name ?? "upload.bin");

  if (name) form.append("name", name);
  if (description) form.append("description", description);

  if (properties) {
    // backend expects a string? stringify objects
    form.append(
      "properties",
      typeof properties === "string" ? properties : JSON.stringify(properties)
    );
  }

  if (tags && tags.length > 0) {
    // same note: if backend expects structured fields, adjust accordingly
    form.append("tags", JSON.stringify(tags));
  }

  if (originalId) form.append("originalId", originalId);
  if (classId != null) form.append("classId", String(classId));

  const params: Record<string, number> = {};
  if (dataSourceId != null) params.dataSourceId = Number(dataSourceId);
  if (objectStorageId != null) params.objectStorageId = Number(objectStorageId);

  const { data } = await api.post<RecordResponseDto>(
    `/organizations/${organizationId}/projects/${projectId}/files`,
    form,
    { params }
  );

  return data;
}

export async function uploadFilesBatch(
  args: Omit<UploadFileArgs, "file"> & { files: File[] }
) {
  const { files, ...rest } = args;
  return Promise.allSettled(files.map((file) => uploadFile({ ...rest, file })));
}
