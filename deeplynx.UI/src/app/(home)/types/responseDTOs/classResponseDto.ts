export type ClassResponseDto = {
  id: number;
  name: string;
  description: string | null;
  uuid: string | null;
  projectid: number;
  lastUpdatedAt: string | null;
  lastUpdatedBy: string | null;
  isArchived:boolean;
  archivedat: string | null;
  createdby: string | null;
  createdat: string;
};