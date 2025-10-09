export type GroupResponseDto = {
  id: number|string;
  name: string;
  description?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  organizationId: number | string;
}