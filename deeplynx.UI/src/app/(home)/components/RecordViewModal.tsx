import React from 'react';
import PropertyTable from '../(routes)/data_catalog/record/PropertyTable';
import { Column, FileViewerTableRow } from '../types/types';
import GenericTable from './GenericTable';

interface RecordViewModalProps {
  isOpen: boolean;
  onClose: () => void;
  record: FileViewerTableRow;
  relatedRecords: RelatedRecord[]
  tags: { id: string; name: string}[]
}

interface RelatedRecord {
  relationship: string;
  id: string;
  class: string;
  name: string;
  actions: React.JSX.Element;
}

const RecordViewModal: React.FC<RecordViewModalProps> = ({ isOpen, onClose, record, relatedRecords, tags }) => {
  if (!isOpen || !record) return null;

  // Prepare system properties and additional properties for the PropertyTable
  const systemPropertiesRows = [
    { label: "Record Name", value: record.name },
    { label: "Record Description", value: record.description },
    { label: "Data Source Name", value: record.dataSourceName },
    { label: "Uri", value: record.dataSourceName },
    { label: "Class Name", value: record.dataSourceName },
    { label: "OriginalID", value: record.originalId },
    { label: "Created By", value: record.createdBy },
    { label: "Created At", value: formatDate(record.createdAt) },
    { label: "Modified By", value: record.modifiedBy },
    { label: "Modified At", value: formatDate(record.modifiedAt) },
  ];

  const relatedRecordsColumns: Column<RelatedRecord>[] = [
      { header: "Relationship", data: "relationship" },
      { header: "ID", data: "id" },
      { header: "Class", data: "class" },
      { header: "Name", data: "name" },
      { header: "Actions", data: "actions", sortable: false },
    ];

  const parsedProperties = JSON.parse(record.properties!);
  const additionalPropertiesRows = parsedProperties
    ? Object.keys(parsedProperties).map((key) => {
        const value = parsedProperties[key as keyof object];
        return {
          label: key,
          value: typeof value === "object" ? JSON.stringify(value) : String(value),
        };
      })
    : [];

  return (
    <dialog className="modal modal-open">
      <div className="modal-box max-w-lg">
        <h3 className="font-bold text-lg mb-4 text-neutral">Record Details</h3>
        
        <div>
          <h4 className="font-semibold">System Properties:</h4>
          <PropertyTable title="System Properties" rows={systemPropertiesRows} />
        </div>

        <div>
          <h4 className="font-semibold">Additional Properties:</h4>
          <PropertyTable title="Additional Properties" rows={additionalPropertiesRows} />
        </div>

        <div>
          <h4 className="font-semibold">Tags:</h4>
          <ul>
            {tags && tags.map((tag: { id: string; name: string }) => (
              <li key={tag.id}>{tag.name}</li>
            ))}
          </ul>
        </div>

        <div>
            <GenericTable
                columns={relatedRecordsColumns}
                data={relatedRecords}
                title="Related Records"
                bordered
                searchBar={false}
                enablePagination={false}
                actionButtons={false}
            />
        </div>
        <div className="modal-action">
          <button className="btn" onClick={onClose}>Close</button>
        </div>
      </div>
    </dialog>
  );
};

function formatDate(date: string | null | undefined): string {
  if (!date) return "N/A";
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

export default RecordViewModal;