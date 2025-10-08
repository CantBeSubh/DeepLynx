export type ProjectDTO = {
  id: number | string;
  name: string;
  description?: string | null;
  abbreviation ?: string | null;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  organizationId: number | string;
};