"use client";

import { useLanguage } from "@/app/contexts/Language";
import { parseCsvFile } from "@/app/lib/client_service/csv_parser";
import { validateCsvRecords } from "@/app/lib/validate_records";
import type {
  ParsedCsvRow,
  ValidationResult,
} from "@/app/(home)/types/bulk_upload_types";
import type {
  DataSourceResponseDto,
  ProjectResponseDto,
} from "@/app/(home)/types/responseDTOs";
import toast from "react-hot-toast";
import CsvTemplateDownload from "../../components/CsvTemplateDownload";
import ValidationResults from "./ValidationResults";
import { InformationCircleIcon } from "@heroicons/react/24/outline";

type BackendError = {
  message: string;
  type: "validation" | "not_found" | "permission" | "general";
  suggestion?: string;
};

interface BulkUploadSectionProps {
  // CSV State
  csvFile: File | null;
  setCsvFile: (file: File | null) => void;
  isParsing: boolean;
  setIsParsing: (parsing: boolean) => void;
  setParsedCsvData: (data: ParsedCsvRow[]) => void;
  csvParseErrors: string[];
  setCsvParseErrors: (errors: string[]) => void;

  // Validation State
  validationResult: ValidationResult | null;
  setValidationResult: (result: ValidationResult | null) => void;
  isValidating: boolean;
  setIsValidating: (validating: boolean) => void;

  // Upload State
  isUploading: boolean;
  setShowUploadConfirm: (show: boolean) => void;
  backendErrors: BackendError[];
  setBackendErrors: (errors: BackendError[]) => void;
  showPreview: boolean;
  setShowPreview: (show: boolean) => void;
  uploadProgress: number;

  // Project Resources
  projectId: string;
  dataSourceId: string;
  organizationId?: number;

  // For confirmation modal
  projects: ProjectResponseDto[];
  dataSources: DataSourceResponseDto[];
}

export default function BulkUploadSection(props: BulkUploadSectionProps) {
  const { t } = useLanguage();

  const handleCsvUpload = async (file: File) => {
    props.setCsvFile(file);
    props.setIsParsing(true);
    props.setParsedCsvData([]);
    props.setCsvParseErrors([]);
    props.setValidationResult(null);
    props.setBackendErrors([]);

    try {
      const parseResult = await parseCsvFile(file);

      if (parseResult.success) {
        props.setParsedCsvData(parseResult.data);
        toast.success(
          `Successfully parsed ${parseResult.data.length} rows from CSV`
        );

        if (!props.projectId || !props.dataSourceId || !props.organizationId) {
          toast.error("Please select project and data source first");
          props.setIsParsing(false);
          return;
        }

        props.setIsValidating(true);

        try {
          const validationResult = validateCsvRecords(
            parseResult.data,
            props.projectId,
            props.dataSourceId,
            props.organizationId
          );

          props.setValidationResult(validationResult);

          if (validationResult.isValid) {
            toast.success(
              `All ${validationResult.validCount} records validated successfully!`
            );
          } else {
            toast.error(
              `Validation failed: ${validationResult.invalidCount} of ${validationResult.totalRows} records have errors`
            );
          }
        } catch (error) {
          console.error("Validation error:", error);
          toast.error("Error validating records");
        } finally {
          props.setIsValidating(false);
        }
      } else {
        props.setCsvParseErrors(parseResult.errors);
        toast.error("Failed to parse CSV file");
      }
    } catch (error) {
      console.error("Error parsing CSV:", error);
      props.setCsvParseErrors(["Unexpected error while parsing CSV file"]);
      toast.error("Error parsing CSV file");
    } finally {
      props.setIsParsing(false);
    }
  };

  return (
    <>
      {/* Info Box */}
      <div className="bg-info/10 p-6 rounded-lg space-y-4 border border-info/20">
        <div className="flex items-start gap-3">
          <InformationCircleIcon className="size-6" />
          <div>
            <h3 className="font-semibold text-base-content">
              {t.translations.BULK_METADATA_UPLOAD || "Bulk Metadata Upload"}
            </h3>
            <p className="text-sm text-base-content/70 mt-1">
              {t.translations.BULK_METADATA_INSTRUCTIONS ||
                "Create multiple records at once by uploading a CSV file with metadata."}
            </p>
          </div>
        </div>

        {/* Steps */}
        <div className="flex flex-col gap-3">
          {/* Step 1: Download Template */}
          <label className="label flex-col items-start">
            <span className="label-text font-semibold">
              Step 1: Download Template
            </span>
            <CsvTemplateDownload />
          </label>

          {/* Step 2: Upload CSV */}
          <div>
            <label className="label flex-col items-start">
              <span className="label-text font-semibold">
                Step 2: Upload Your CSV
              </span>
              <input
                type="file"
                accept=".csv"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) handleCsvUpload(file);
                }}
                className="file-input file-input-bordered file-input-primary w-full max-w-xs"
                disabled={
                  props.isParsing ||
                  props.isValidating ||
                  !props.projectId ||
                  !props.dataSourceId
                }
              />
            </label>
            {props.csvFile && (
              <div className="mt-2 text-sm text-base-content/70 flex items-center gap-2">
                <span>
                  Selected:{" "}
                  <span className="font-semibold">{props.csvFile.name}</span>
                </span>
                {(!props.projectId || !props.dataSourceId) && (
                  <div className="badge badge-warning badge-sm">
                    Select project and data source first
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Validation Results */}
      {props.csvFile && <ValidationResults {...props} />}
    </>
  );
}
