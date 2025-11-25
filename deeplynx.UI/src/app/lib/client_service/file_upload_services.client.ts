// src/app/lib/file_upload_services.client.ts

import api from "./client_service/api";
import { UploadFileArgs } from "../(home)/types/types";
import { RecordResponseDto } from "../(home)/types/responseDTOs";

export async function uploadFile({
  organizationId,
  projectId,
  dataSourceId,
  objectStorageId,
  file,
  name,
  description,
  properties,
  tags,
  originalId,
  classId,
}: UploadFileArgs) {
  if (!organizationId || !projectId || !dataSourceId || !objectStorageId || !file) {
    throw new Error("organizationId, projectId, dataSourceId, objectStorageId, and file are required");
  }

  const form = new FormData();
  form.append("file", file);

  // Add metadata to FormData if provided
  if (name) {
    form.append("name", name);
  }

  if (description) {
    form.append("description", description);
  }

  if (properties) {
    form.append("properties", JSON.stringify(properties));
  }

  if (tags && tags.length > 0) {
    form.append("tags", JSON.stringify(tags));
  }

  if (originalId) {
    form.append("originalId", originalId);
  }

  if (classId) {
    form.append("classId", String(classId));
  }

  const { data } = await api.post<RecordResponseDto>(
    `/organizations/${organizationId}/projects/${projectId}/files`,
    form,
    {
      params: {
        dataSourceId: Number(dataSourceId),
        objectStorageId: Number(objectStorageId),
      },
    }
  );

  return data;
}

export async function uploadFilesBatch(
  args: Omit<UploadFileArgs, "file"> & { files: File[] }
) {
  const { files, ...rest } = args;
  return Promise.allSettled(files.map((file) => uploadFile({ ...rest, file })));
}