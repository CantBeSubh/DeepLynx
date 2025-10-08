export type DataSourceResponseDto = {
  id: number;
  name: string;
  description: string | null;
  default:boolean;
  abbreviation: string | null;
  type: string | null;
  baseuri: string | null;
  config: Record<string, unknown> | null; // object | null
  projectid: number;
  lastUpdatedAt: string | null; // RFC 3339 or null
  lastUpdatedBy: string | null;  
  isArchived:boolean;
  createdby: string | null;
  createdat: string;          // RFC 3339 date-time
  archivedat: string | null;  // RFC 3339 or null
};