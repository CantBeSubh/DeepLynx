export type RelatedRecordsRequestDto = {
    recordId?: number;
    isOrigin?: boolean;
    page?: number;
    pageSize?: number;
    hideArchived?: boolean;
}