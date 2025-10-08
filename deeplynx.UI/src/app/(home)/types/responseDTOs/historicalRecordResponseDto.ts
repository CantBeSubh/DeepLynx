export type HistoricalRecordResponseDto = {
  Id?: number;
  Uri?: string;
  Properties: string;
  OriginalId?: string;
  Name?: string;
  Description?: string;
  ClassId?: number;
  ClassName?: string;
  DataSourceId?: number;
  DataSourceName?: string;
  ObjectStorageId?: number;
  ObjectStorageName?: string;
  ProjectId: number;
  ProjectName: string;
  Tags?: string;
  LastUpdatedAt?: Date;
  LastUpdateBy?: string;
  isArchived:boolean
  CreatedBy?: string;
  CreatedAt?: Date;
  ArchivedAt?: Date;
}