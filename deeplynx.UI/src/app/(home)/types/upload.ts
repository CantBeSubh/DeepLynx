export type UploadType = "new" | "version" | "properties" | "";

export type ExistingFile = {
  id: string;
  name: string;
  alias?: string;
  description?: string;
  lastUpdate?: string;
  updatedBy?: string;
  tags?: string[];
  dataSource?: string;
  propertiesSources?: string;
  timeSeries?: boolean;
  image?: string;
};

export type RecentUploadIcon = "arrows" | "link" | "stack";

export type RecentUpload = {
  id: string;
  name: string;
  avatar: string;
  file: string;
  icon: RecentUploadIcon;
};
