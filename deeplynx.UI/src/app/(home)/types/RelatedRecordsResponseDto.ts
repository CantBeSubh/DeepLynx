import React from "react";

export type RelatedRecordsResponseDto = {
    relatedRecordName: string;
    relatedRecordId: number;
    relatedRecordProjectId: number;
    relationshipName: string | null;
}