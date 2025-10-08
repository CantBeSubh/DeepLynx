import React from "react";

export type RelatedRecordsResponseDto = {
    relatedRecordName: string;
    relatedRecordId?: number | null;
    relatedRecordProjectId?: number | null;
    relationshipName?: string | null;
    isOrigin?: boolean;
}