export type TagResponseDto = {
  id: number;
  name: string;
  projectId: number;
  lastUpdatedAt?: string | null;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  createdBy?: string | null;
  createdAt: string; // ISO date string from backend
  archivedAt?: string | null;
};