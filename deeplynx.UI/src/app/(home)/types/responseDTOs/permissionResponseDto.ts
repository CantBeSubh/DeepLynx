export type GroupResponseDto = {
  id: number|string;
  name: string;
  description?: string | null;
  action:string;
  resource?:string|null;
  isHardcoded:boolean;
  labelId?:number|string;
  lastUpdatedAt?: Date;
  lastUpdatedBy?: string | null;
  isArchived:boolean;
  projectId?: number | string;
  organizationId?: number | string;
}