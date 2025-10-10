export type CustomQueryRequestDto = {
  connector?: string | null;
  filter: string;
  operator: string;
  value: string;
  json?: string;
  jsonKey?: string;
  jsonValue?: string;
};