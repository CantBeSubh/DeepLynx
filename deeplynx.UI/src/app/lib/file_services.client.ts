// lib/file_services.client.ts
"use client";

import api from "./api";
import axios from "axios";

const MIME_EXT: Record<string, string> = {
  "application/pdf": "pdf",
  "application/zip": "zip",
  "image/png": "png",
  "image/jpeg": "jpg",
  "image/gif": "gif",
  "text/plain": "txt",
};

function parseFilenameFromCD(cd?: string): string | undefined {
  if (!cd) return;
  const match =
    cd.match(/filename\*?=(?:UTF-8''|")?([^";]+)/i) ??
    cd.match(/filename="?([^"]+)"?/i);
  return match?.[1] ? decodeURIComponent(match[1]) : undefined;
}

function hasExtension(name: string): boolean {
  return /\.[A-Za-z0-9]{2,8}$/.test(name);
}

function sanitizeFilename(name: string): string {
  return name.replace(/[<>:"/\\|?*\x00-\x1F]/g, "_");
}

export async function downloadFile(
  projectId: number,
  recordId: number,
  recordName?: string
): Promise<void> {
  let url: string | null = null;
  try {
    const res = await api.get(
      `/projects/${projectId}/files/DownloadFile/${recordId}`,
      { responseType: "blob" }
    );

    const blob = res.data as Blob;

    // 1) Server-provided name (best)
    // 2) Caller-provided name
    // 3) Fallback "file"
    let filename =
      parseFilenameFromCD(res.headers["content-disposition"]) ||
      recordName?.trim() ||
      "file";

    // Does not guess if an extension already exists
    if (!hasExtension(filename)) {
      const ext = MIME_EXT[blob.type];
      if (ext) filename += `.${ext}`;
    }

    filename = sanitizeFilename(filename);

    url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
  } catch (err: unknown) {
    if (axios.isAxiosError(err)) {
      const { response } = err;
      if (response?.data instanceof Blob) {
        try {
          const text = await response.data.text();
          console.error("Download failed:", response.status, text || err.message);
        } catch {
          console.error("Download failed:", response.status, err.message);
        }
      } else {
        console.error("Download failed:", response?.status, err.message);
      }
    } else {
      console.error("Download failed:", err);
    }
    throw err;
  } finally {
    if (url) URL.revokeObjectURL(url);
  }
}
