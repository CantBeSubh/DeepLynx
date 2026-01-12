// src/app/(home)/upload_center/UploadCenterClient.tsx
"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { getAllDataSources } from "@/app/lib/client_service/data_source_services.client";
import {
  uploadFile,
  uploadFilesBatch,
} from "@/app/lib/client_service/file_upload_services.client";
import { getAllObjectStorages } from "@/app/lib/client_service/object_storage_services.client";
import { getAllProjects } from "@/app/lib/client_service/projects_services.client";
import { useCallback, useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import DropUpload from "../components/DropUpload";
import FileDetailsCard from "../components/FileDetailCard";
import NewFileUploadCard from "../components/NewFileUploadCard";
import SelectedFilesCard from "../components/SelectedFilesCard";
import CsvTemplateDownload from "../components/CsvTemplateDownload";
import { parseCsvFile } from "@/app/lib/client_service/csv_parser";
import { ParsedCsvRow } from "../types/bulk_upload_types";
import { validateCsvRecords } from "../../lib/validate_records";
import { ValidationResult } from "../types/bulk_upload_types";
import { uploadBulkMetadata } from "@/app/lib/client_service/metadata_service.client";
import {
  DataSourceResponseDto,
  ObjectStorageResponseDto,
  ProjectResponseDto,
} from "../types/responseDTOs";
import {
  ExistingFile,
  FileMetadata,
  RecentUpload,
  UploadType,
} from "../types/types";
import { parseBackendErrors } from "@/app/lib/error_parser";

type Props = {
  initialAvailableFiles: ExistingFile[];
  initialRecentUploads: RecentUpload[];
  uploadText: string;
};

type UploadMode = "file" | "bulk";

export default function UploadCenterClient({ initialAvailableFiles }: Props) {
  const { t } = useLanguage();
  const { organization } = useOrganizationSession();
  const [multi, setMulti] = useState(false);
  const [showMultiFileWarning, setShowMultiFileWarning] = useState(false);
  const [uploadType, setUploadType] = useState<UploadType>("new");
  const [targetFileId, setTargetFileId] = useState("");
  const [destination, setDestination] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [projects, setProjects] = useState<ProjectResponseDto[]>([]);
  const [objectStorage, setObjectstorage] = useState<
    ObjectStorageResponseDto[]
  >([]);
  const [dataSources, setDataSources] = useState<DataSourceResponseDto[]>([]);
  const [projectId, setProjectId] = useState<string>("");
  const [dataSourceId, setDataSourceId] = useState<string>("");
  const [objectStorageId, setObjectstorageId] = useState<string>("");
  const [filesMetadata, setFilesMetadata] = useState<
    Record<number, FileMetadata>
  >({});
  const [isLoadingDataSources, setIsLoadingDataSources] = useState(false);
  const [isLoadingObjectStorage, setIsLoadingObjectStorage] = useState(false);
  const [isLoadingProjects, setIsLoadingProjects] = useState(false);
  const [uploadMode, setUploadMode] = useState<UploadMode>("file");
  const [csvFile, setCsvFile] = useState<File | null>(null);
  const showRightPanel = selectedFiles.length > 0 && uploadMode === "file";
  const [parsedCsvData, setParsedCsvData] = useState<ParsedCsvRow[]>([]);
  const [csvParseErrors, setCsvParseErrors] = useState<string[]>([]);
  const [isParsing, setIsParsing] = useState(false);
  const [validationResult, setValidationResult] =
    useState<ValidationResult | null>(null);
  const [isValidating, setIsValidating] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [showUploadConfirm, setShowUploadConfirm] = useState(false);
  const [backendErrors, setBackendErrors] = useState<
    Array<{
      message: string;
      type: "validation" | "not_found" | "permission" | "general";
      suggestion?: string;
    }>
  >([]);

  const handleMetadataChange = useCallback(
    (fileIndex: number, metadata: FileMetadata) => {
      setFilesMetadata((prev) => ({ ...prev, [fileIndex]: metadata }));
    },
    []
  );

  const needsTarget = uploadType === "version" || uploadType === "properties";
  const availableFiles = useMemo(
    () => initialAvailableFiles,
    [initialAvailableFiles]
  );
  const selectedTarget = useMemo(
    () => availableFiles.find((f) => f.id === targetFileId) ?? null,
    [availableFiles, targetFileId]
  );

  const [dropKey, setDropKey] = useState(0);

  const resetForm = (opts?: { keepProject?: boolean }) => {
    const keepProject = opts?.keepProject ?? true;

    setSelectedFiles([]);
    setUploadType("new");
    setDestination("");
    setTargetFileId("");
    setMulti(false);
    setFilesMetadata({});

    if (!keepProject) {
      setProjectId("");
      setDataSources([]);
      setObjectstorage([]);
    }
    setDataSourceId("");
    setObjectstorageId("");

    setDropKey((k) => k + 1);
  };

  const canUpload =
    selectedFiles.length > 0 &&
    !!projectId &&
    !!dataSourceId &&
    !!objectStorageId &&
    (!needsTarget || !!targetFileId);

  useEffect(() => {
    if (!needsTarget) setTargetFileId("");
  }, [needsTarget]);

  const removeAt = (idx: number) =>
    setSelectedFiles((prev) => prev.filter((_, i) => i !== idx));
  const clearAll = () => setSelectedFiles([]);

  const handleUpload = async () => {
    if (!projectId || selectedFiles.length === 0) {
      toast.error("Select a project and at least one file.");
      return;
    }
    try {
      if (selectedFiles.length === 1) {
        const file = selectedFiles[0];
        const metadata = filesMetadata[0] ?? {};
        await uploadFile({
          organizationId: organization?.organizationId as number,
          projectId,
          dataSourceId, // may be undefined
          objectStorageId, // may be undefined
          file,
          name: metadata.name || file.name,
          description: metadata.description || "",
          // properties: metadata.properties, // if you collect extra JSON
          // tags: metadata.tags, // if you collect tags
        });
        toast.success("File uploaded!");
      } else {
        // Use your batch helper (returns Promise.allSettled)
        const results = await uploadFilesBatch({
          organizationId: organization?.organizationId as number,
          projectId,
          dataSourceId,
          objectStorageId,
          files: selectedFiles,
          // optional shared metadata defaults
        });

        const ok = results.filter((r) => r.status === "fulfilled").length;
        const fail = results.length - ok;
        toast.success(
          `Uploaded ${ok} file(s)${fail ? ` • ${fail} failed` : ""}`
        );
        if (fail) console.warn("Batch upload failures:", results);
      }
      resetForm({ keepProject: true });
    } catch (err) {
      console.error(err);
      toast.error("Upload failed. See console for details.");
    }
  };

  const isMultiAllowed = uploadType === "new";

  useEffect(() => {
    if (!isMultiAllowed && multi) setMulti(false);
  }, [isMultiAllowed, multi]);

  useEffect(() => {
    if (!projectId) {
      setDataSources([]);
      setObjectstorage([]);
      setDataSourceId("");
      setObjectstorageId("");
      setIsLoadingDataSources(false);
      setIsLoadingObjectStorage(false);
      return;
    }

    setIsLoadingDataSources(true);
    setIsLoadingObjectStorage(true);

    setDataSourceId("");
    setObjectstorageId("");

    (async () => {
      try {
        const dataSource = await getAllDataSources(Number(projectId));
        setDataSources(dataSource);
        if (dataSource.length === 1) {
          setDataSourceId(String(dataSource[0].id));
        }
        setIsLoadingDataSources(false);
      } catch (error) {
        console.error("Error fetching data sources:", error);
        setDataSources([]);
        setIsLoadingDataSources(false);
      }

      try {
        const objectStorage = await getAllObjectStorages(
          organization?.organizationId as number,
          Number(projectId)
        );
        setObjectstorage(objectStorage);
        if (objectStorage.length === 1) {
          setObjectstorageId(String(objectStorage[0].id));
        }
        setIsLoadingObjectStorage(false);
      } catch (error) {
        console.error("Error fetching object storage:", error);
        setObjectstorage([]);
        setIsLoadingObjectStorage(false);
      }
    })();
  }, [organization?.organizationId, projectId]);

  // Memoize the fetch projects function
  const fetchProjects = useCallback(async () => {
    if (!organization) {
      setProjects([]);
      setProjectId("");
      setIsLoadingProjects(false);
      return;
    }

    setIsLoadingProjects(true);

    try {
      const data = await getAllProjects(
        organization.organizationId as number,
        true
      );
      setProjects(data);
      if (data.length === 1) {
        setProjectId(String(data[0].id));
      }
    } catch (error) {
      console.error("Error fetching projects:", error);
      setProjects([]);
    } finally {
      setIsLoadingProjects(false);
    }
  }, [organization]);

  const handleBulkUpload = async () => {
    if (!validationResult || !validationResult.isValid) {
      toast.error("Please fix validation errors before uploading");
      return;
    }

    if (!projectId || !dataSourceId) {
      toast.error("Please select project and data source");
      return;
    }

    if (!organization?.organizationId) {
      toast.error("Organization not found");
      return;
    }

    setIsUploading(true);
    setBackendErrors([]); // Clear previous backend errors

    try {
      // Upload to API
      await uploadBulkMetadata(
        organization.organizationId as number,
        Number(projectId),
        Number(dataSourceId),
        validationResult.validRecords
      );

      toast.success(
        `Successfully uploaded ${validationResult.validCount} records!`
      );

      // Reset form
      setCsvFile(null);
      setParsedCsvData([]);
      setValidationResult(null);
      setCsvParseErrors([]);
      setBackendErrors([]);

      // Reset file input
      const fileInput = document.querySelector(
        'input[type="file"]'
      ) as HTMLInputElement;
      if (fileInput) fileInput.value = "";
    } catch (error: any) {
      console.error("Upload error:", error);

      // Extract detailed error information
      let errorMessages: string[] = [];

      if (error.response?.data) {
        const data = error.response.data;

        // Check for various error formats the backend might return
        if (data.errors && Array.isArray(data.errors)) {
          errorMessages = data.errors.map((err: any) =>
            typeof err === "string" ? err : err.message || JSON.stringify(err)
          );
        } else if (data.error) {
          errorMessages = [
            typeof data.error === "string"
              ? data.error
              : data.error.message || JSON.stringify(data.error),
          ];
        } else if (data.message) {
          errorMessages = [data.message];
        } else {
          errorMessages = [JSON.stringify(data)];
        }
      } else {
        errorMessages = [error.message || "Unknown error occurred"];
      }

      // Parse and clean up the error messages
      const parsedErrors = parseBackendErrors(errorMessages);
      setBackendErrors(parsedErrors); // Now storing parsed error objects

      toast.error("Upload failed. Please check the error details below.");
    } finally {
      setIsUploading(false);
    }
  };

  // Fetch projects filtered by organization
  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-6">
        <h1 className="text-2xl font-bold text-base-content">
          {t.translations.UPLOAD_CENTER}
        </h1>
      </div>

      <div
        className={`flex gap-8 p-10 lg:p-20 ${
          showRightPanel ? "justify-between" : "justify-center"
        }`}
      >
        {/* LEFT */}
        <div
          className={`w-full lg:w-3/5 ${
            showRightPanel ? "" : "max-w-5xl mx-auto"
          }`}
        >
          {/* UPLOAD MODE TOGGLE */}
          <div className="mb-6">
            <label className="label">
              <span className="label-text font-bold text-base-content">
                {t.translations.UPLOAD_MODE || "Upload Mode"}
              </span>
            </label>
            <div className="btn-group">
              <button
                type="button"
                className={`btn btn-sm mr-5 ${
                  uploadMode === "file" ? "btn-primary" : "btn-ghost"
                }`}
                onClick={() => {
                  setUploadMode("file");
                  setCsvFile(null);
                }}
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth={1.5}
                  stroke="currentColor"
                  className="w-4 h-4"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"
                  />
                </svg>
                {t.translations.FILE_UPLOAD || "File Upload"}
              </button>
              <button
                type="button"
                className={`btn btn-sm ${
                  uploadMode === "bulk" ? "btn-primary" : "btn-ghost"
                }`}
                onClick={() => {
                  setUploadMode("bulk");
                  setSelectedFiles([]);
                  resetForm({ keepProject: true });
                }}
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth={1.5}
                  stroke="currentColor"
                  className="w-4 h-4"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M3.375 19.5h17.25m-17.25 0a1.125 1.125 0 01-1.125-1.125M3.375 19.5h7.5c.621 0 1.125-.504 1.125-1.125m-9.75 0V5.625m0 12.75v-1.5c0-.621.504-1.125 1.125-1.125m18.375 2.625V5.625m0 12.75c0 .621-.504 1.125-1.125 1.125m1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125m0 3.75h-7.5A1.125 1.125 0 0112 18.375m9.75-12.75c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125m19.5 0v1.5c0 .621-.504 1.125-1.125 1.125M2.25 5.625v1.5c0 .621.504 1.125 1.125 1.125m0 0h17.25m-17.25 0h7.5c.621 0 1.125.504 1.125 1.125M3.375 8.25c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125m17.25-3.75c.621 0 1.125.504 1.125 1.125v1.5c0 .621-.504 1.125-1.125 1.125m-17.25 0V18m0-7.5c0-.621.504-1.125 1.125-1.125h15.75c.621 0 1.125.504 1.125 1.125m1.5-1.5V18m-18.75 0h17.25"
                  />
                </svg>
                {t.translations.BULK_METADATA || "Bulk Metadata"}
              </button>
            </div>
          </div>

          {/* PROJECT, DATA SOURCE, OBJECT STORAGE SELECTORS (Always visible) */}
          <div className="p-4 space-y-4">
            <fieldset className="space-x-4">
              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">
                  Select a project
                  {isLoadingProjects && (
                    <span className="loading loading-spinner loading-xs ml-2"></span>
                  )}
                </span>
                <select
                  value={projectId}
                  onChange={(e) => setProjectId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                  disabled={!organization || isLoadingProjects}
                >
                  <option value="" disabled>
                    {!organization
                      ? "Select an organization first"
                      : isLoadingProjects
                      ? "Loading projects..."
                      : t.translations.PROJECT}
                  </option>
                  {projects.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">
                  {t.translations.DATA_SOURCE}
                  {isLoadingDataSources && (
                    <span className="loading loading-spinner loading-xs ml-2"></span>
                  )}
                </span>
                <select
                  value={dataSourceId}
                  onChange={(e) => setDataSourceId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                  disabled={!projectId || isLoadingDataSources}
                >
                  <option value="" disabled>
                    {!projectId
                      ? "Select a project first"
                      : isLoadingDataSources
                      ? "Loading data sources..."
                      : "Data Sources"}
                  </option>
                  {dataSources.map((d) => (
                    <option key={d.id} value={String(d.id)}>
                      {d.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">
                  {t.translations.STORAGE_DESTINATION}
                  {isLoadingObjectStorage && (
                    <span className="loading loading-spinner loading-xs ml-2"></span>
                  )}
                </span>
                <select
                  value={objectStorageId}
                  onChange={(e) => setObjectstorageId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required={uploadMode === "file"}
                  disabled={
                    !projectId ||
                    isLoadingObjectStorage ||
                    uploadMode === "bulk"
                  }
                >
                  <option value="" disabled>
                    {!projectId
                      ? "Select a project first"
                      : isLoadingObjectStorage
                      ? "Loading object storages..."
                      : "Object storages"}
                  </option>
                  {objectStorage.map((object) => (
                    <option key={object.id} value={String(object.id)}>
                      {object.name}
                    </option>
                  ))}
                </select>
              </label>
            </fieldset>

            {/* CONDITIONAL RENDERING BASED ON UPLOAD MODE */}
            {uploadMode === "file" ? (
              // SINGLE FILE UPLOAD MODE (existing UI)
              <>
                <fieldset>
                  <label className="label text-base-content font-bold">
                    {t.translations.UPLOADING}
                    <select
                      value={uploadType}
                      onChange={(e) =>
                        setUploadType(e.target.value as UploadType)
                      }
                      className="select select-info select-sm mt-2"
                      required
                    >
                      <option value="new">{t.translations.NEW_FILE}</option>
                      {/* Future options can be added here */}
                    </select>
                    {needsTarget && (
                      <select
                        value={targetFileId}
                        onChange={(e) => setTargetFileId(e.target.value)}
                        className="select select-info select-sm mt-2"
                        required
                      >
                        <option value="" disabled>
                          {t.translations.SELECT_EXISTING_FILE}
                        </option>
                        {availableFiles.map((f) => (
                          <option key={f.id} value={f.id}>
                            {f.name}
                          </option>
                        ))}
                      </select>
                    )}
                  </label>
                </fieldset>

                <fieldset>
                  <label className="label cursor-pointer justify-start gap-3">
                    <span className="label-text text-xs">
                      {t.translations.UPLOAD_MULTIPLE_FILES}
                    </span>
                    <input
                      type="checkbox"
                      checked={multi}
                      disabled={!isMultiAllowed}
                      onChange={(e) => {
                        if (!isMultiAllowed) return;
                        const checked = e.target.checked;
                        if (!checked && selectedFiles.length > 1) {
                          setShowMultiFileWarning(true);
                          return;
                        }
                        setMulti(checked);
                      }}
                      className="toggle toggle-secondary"
                      aria-describedby="multi-files-hint"
                    />
                  </label>
                </fieldset>

                {(multi || selectedFiles.length === 0) && (
                  <DropUpload
                    key={dropKey}
                    multiple={multi}
                    files={selectedFiles}
                    onFilesChange={setSelectedFiles}
                    disabled={!uploadType || (needsTarget && !targetFileId)}
                  />
                )}

                {selectedFiles.length >= 1 &&
                  selectedFiles.map((file, index) => (
                    <NewFileUploadCard
                      key={index}
                      defaultName={file.name}
                      uploadType={uploadType}
                      fileIndex={index}
                      onMetadataChange={handleMetadataChange}
                    />
                  ))}
              </>
            ) : (
              // BULK CSV UPLOAD MODE
              <>
                <div className="bg-info/10 p-6 rounded-lg space-y-4 border border-info/20">
                  <div className="flex items-start gap-3">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                      strokeWidth={1.5}
                      stroke="currentColor"
                      className="w-6 h-6 text-info flex-shrink-0 mt-0.5"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"
                      />
                    </svg>
                    <div>
                      <h3 className="font-semibold text-base-content">
                        {t.translations.BULK_METADATA_UPLOAD ||
                          "Bulk Metadata Upload"}
                      </h3>
                      <p className="text-sm text-base-content/70 mt-1">
                        {t.translations.BULK_METADATA_INSTRUCTIONS ||
                          "Create multiple records at once by uploading a CSV file with metadata. No actual files are uploaded - only record metadata is created."}
                      </p>
                    </div>
                  </div>

                  <div className="flex flex-col gap-3">
                    <label className="label flex-col items-start">
                      <span className="label-text font-semibold">
                        Step 1: Download Template
                      </span>
                      <CsvTemplateDownload />
                    </label>

                    <div>
                      <label className="label flex-col items-start">
                        <span className="label-text font-semibold">
                          Step 2: Upload Your CSV
                        </span>

                        <input
                          type="file"
                          accept=".csv"
                          onChange={async (e) => {
                            const file = e.target.files?.[0];
                            if (file) {
                              setCsvFile(file);
                              setIsParsing(true);
                              setParsedCsvData([]);
                              setCsvParseErrors([]);
                              setValidationResult(null);
                              setBackendErrors([]);

                              try {
                                // Step 1: Parse CSV
                                const parseResult = await parseCsvFile(file);

                                if (parseResult.success) {
                                  setParsedCsvData(parseResult.data);
                                  toast.success(
                                    `Successfully parsed ${parseResult.data.length} rows from CSV`
                                  );

                                  // Step 2: Validate parsed data
                                  if (
                                    !projectId ||
                                    !dataSourceId ||
                                    !organization?.organizationId
                                  ) {
                                    toast.error(
                                      "Please select project, data source, and object storage first"
                                    );
                                    setIsParsing(false);
                                    return;
                                  }

                                  setIsValidating(true);

                                  try {
                                    const validationResult = validateCsvRecords(
                                      parseResult.data,
                                      projectId,
                                      dataSourceId,
                                      organization.organizationId as number
                                    );

                                    setValidationResult(validationResult);

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
                                    setIsValidating(false);
                                  }
                                } else {
                                  setCsvParseErrors(parseResult.errors);
                                  toast.error("Failed to parse CSV file");
                                }
                              } catch (error) {
                                console.error("Error parsing CSV:", error);
                                setCsvParseErrors([
                                  "Unexpected error while parsing CSV file",
                                ]);
                                toast.error("Error parsing CSV file");
                              } finally {
                                setIsParsing(false);
                              }
                            }
                          }}
                          className="file-input file-input-bordered file-input-primary w-full max-w-xs"
                          disabled={
                            isParsing ||
                            isValidating ||
                            !projectId ||
                            !dataSourceId
                          }
                        />
                      </label>
                      {csvFile && (
                        <div className="mt-2 text-sm text-base-content/70 flex items-center gap-2">
                          <span>
                            Selected:{" "}
                            <span className="font-semibold">
                              {csvFile.name}
                            </span>
                          </span>
                          {(!projectId || !dataSourceId) && (
                            <div className="badge badge-warning badge-sm">
                              Select project and data source first
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                </div>

                {/* VALIDATION RESULTS */}
                {csvFile && (
                  <div className="mt-4 space-y-4">
                    {/* Parsing Status */}
                    {isParsing && (
                      <div className="alert alert-info">
                        <span className="loading loading-spinner loading-sm"></span>
                        <span>Parsing CSV file...</span>
                      </div>
                    )}

                    {/* Parsing Errors */}
                    {!isParsing && csvParseErrors.length > 0 && (
                      <div className="alert alert-error">
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          className="stroke-current shrink-0 h-6 w-6"
                          fill="none"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth="2"
                            d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
                          />
                        </svg>
                        <div>
                          <h3 className="font-bold">CSV Parsing Errors</h3>
                          <ul className="list-disc list-inside text-sm">
                            {csvParseErrors.map((error, idx) => (
                              <li key={idx}>{error}</li>
                            ))}
                          </ul>
                        </div>
                      </div>
                    )}

                    {/* Validating Status */}
                    {isValidating && (
                      <div className="alert alert-info">
                        <span className="loading loading-spinner loading-sm"></span>
                        <span>Validating records...</span>
                      </div>
                    )}

                    {/* Validation Results - Success */}
                    {!isParsing &&
                      !isValidating &&
                      validationResult &&
                      validationResult.isValid && (
                        <div className="space-y-3">
                          <div className="alert alert-success">
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              className="stroke-current shrink-0 h-6 w-6"
                              fill="none"
                              viewBox="0 0 24 24"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth="2"
                                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                              />
                            </svg>
                            <div className="w-full">
                              <h3 className="font-bold">
                                Validation Successful!
                              </h3>
                              <p className="text-sm">
                                All {validationResult.validCount} records are
                                valid and ready to upload.
                              </p>
                            </div>
                          </div>

                          {/* Backend Validation Errors (from API) */}
                          {!isParsing &&
                            !isValidating &&
                            backendErrors.length > 0 && (
                              <div className="alert alert-error">
                                <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  className="stroke-current shrink-0 h-6 w-6"
                                  fill="none"
                                  viewBox="0 0 24 24"
                                >
                                  <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth="2"
                                    d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
                                  />
                                </svg>
                                <div className="w-full">
                                  <h3 className="font-bold">Upload Failed</h3>
                                  <p className="text-sm mb-3">
                                    The server rejected the upload. Please fix
                                    the following issues:
                                  </p>

                                  <div className="space-y-3">
                                    {backendErrors.map((error, idx) => (
                                      <div
                                        key={idx}
                                        className="bg-base-100 p-3 rounded"
                                      >
                                        <div className="flex items-start gap-2 mb-2">
                                          {error.type === "not_found" && (
                                            <span className="badge badge-warning badge-sm">
                                              Not Found
                                            </span>
                                          )}
                                          {error.type === "validation" && (
                                            <span className="badge badge-error badge-sm">
                                              Validation
                                            </span>
                                          )}
                                          {error.type === "permission" && (
                                            <span className="badge badge-error badge-sm">
                                              Permission
                                            </span>
                                          )}
                                          {error.type === "general" && (
                                            <span className="badge badge-neutral badge-sm">
                                              Error
                                            </span>
                                          )}
                                        </div>

                                        <p className="text-sm font-semibold text-error mb-1">
                                          {error.message}
                                        </p>

                                        {error.suggestion && (
                                          <p className="text-sm text-base-content/70 italic">
                                            {error.suggestion}
                                          </p>
                                        )}
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              </div>
                            )}

                          {/* Upload Button */}
                          <div className="flex justify-end">
                            <button
                              onClick={() => setShowUploadConfirm(true)}
                              disabled={isUploading}
                              className="btn btn-primary gap-2"
                              type="button"
                            >
                              {isUploading ? (
                                <>
                                  <span className="loading loading-spinner loading-sm"></span>
                                  Uploading...
                                </>
                              ) : (
                                <>
                                  <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    strokeWidth={1.5}
                                    stroke="currentColor"
                                    className="w-5 h-5"
                                  >
                                    <path
                                      strokeLinecap="round"
                                      strokeLinejoin="round"
                                      d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5"
                                    />
                                  </svg>
                                  Upload {validationResult.validCount} Records
                                </>
                              )}
                            </button>
                          </div>
                        </div>
                      )}

                    {/* Validation Results - Errors */}
                    {!isParsing &&
                      !isValidating &&
                      validationResult &&
                      !validationResult.isValid && (
                        <div className="space-y-3">
                          {/* Summary Alert */}
                          <div className="alert alert-warning">
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              className="stroke-current shrink-0 h-6 w-6"
                              fill="none"
                              viewBox="0 0 24 24"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth="2"
                                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                              />
                            </svg>
                            <div className="w-full">
                              <h3 className="font-bold">
                                Validation Errors Found
                              </h3>
                              <p className="text-sm mb-2">
                                {validationResult.invalidCount} of{" "}
                                {validationResult.totalRows} records have
                                errors. Please fix the errors below and
                                re-upload.
                              </p>
                              <div className="text-sm">
                                <strong>Valid:</strong>{" "}
                                {validationResult.validCount} |{" "}
                                <strong>Invalid:</strong>{" "}
                                {validationResult.invalidCount}
                              </div>
                            </div>
                          </div>

                          {/* Detailed Errors */}
                          <div className="bg-base-200/50 rounded-lg p-4">
                            <h4 className="font-semibold mb-3 text-base-content">
                              Error Details:
                            </h4>
                            <div className="space-y-2 max-h-96 overflow-y-auto">
                              {validationResult.errors.map((error, idx) => (
                                <div
                                  key={idx}
                                  className="bg-base-100 p-3 rounded border-l-4 border-error"
                                >
                                  <div className="flex items-start gap-2">
                                    <span className="badge badge-error badge-sm">
                                      Row {error.row}
                                    </span>
                                    <div className="flex-1">
                                      <p className="font-semibold text-sm text-base-content">
                                        {error.recordName}
                                      </p>
                                      <ul className="list-disc list-inside text-sm text-base-content/70 mt-1 space-y-1">
                                        {error.errors.map((err, errIdx) => (
                                          <li key={errIdx}>{err}</li>
                                        ))}
                                      </ul>
                                    </div>
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        </div>
                      )}
                  </div>
                )}
              </>
            )}
          </div>
          {/* Upload Confirmation Modal */}
          {showUploadConfirm && validationResult && (
            <div className="modal modal-open">
              <div className="modal-box">
                <h3 className="font-bold text-lg">Confirm Bulk Upload</h3>
                <p className="py-4">
                  You are about to upload{" "}
                  <span className="font-bold">
                    {validationResult.validCount} records
                  </span>{" "}
                  to the system.
                </p>
                <div className="bg-base-200 p-3 rounded text-sm space-y-1">
                  <p>
                    <strong>Project:</strong>{" "}
                    {projects.find((p) => p.id === Number(projectId))?.name}
                  </p>
                  <p>
                    <strong>Data Source:</strong>{" "}
                    {
                      dataSources.find((d) => d.id === Number(dataSourceId))
                        ?.name
                    }
                  </p>
                </div>
                <p className="text-sm text-base-content/70 mt-4">
                  This action cannot be undone. Are you sure you want to
                  proceed?
                </p>
                <div className="modal-action">
                  <button
                    className="btn btn-ghost"
                    onClick={() => setShowUploadConfirm(false)}
                    disabled={isUploading}
                  >
                    Cancel
                  </button>
                  <button
                    className="btn btn-primary"
                    onClick={() => {
                      setShowUploadConfirm(false);
                      handleBulkUpload();
                    }}
                    disabled={isUploading}
                  >
                    {isUploading ? (
                      <>
                        <span className="loading loading-spinner loading-sm"></span>
                        Uploading...
                      </>
                    ) : (
                      "Confirm Upload"
                    )}
                  </button>
                </div>
              </div>
              <div
                className="modal-backdrop"
                onClick={() => !isUploading && setShowUploadConfirm(false)}
              />
            </div>
          )}
        </div>

        {/* RIGHT PANEL - only show in single file mode */}
        {showRightPanel && (
          <div className="lg:w-2/5">
            <FileDetailsCard
              needsTarget={needsTarget}
              selectedTarget={selectedTarget}
            />
            <SelectedFilesCard
              files={selectedFiles}
              onRemoveAt={removeAt}
              onClear={clearAll}
              onUpload={handleUpload}
              canUpload={canUpload}
            />
          </div>
        )}
      </div>

      {/* MULTI FILE WARNING MODAL (unchanged) */}
      {showMultiFileWarning && (
        <div className="modal modal-open">
          <div className="modal-box">
            <h3 className="font-bold text-lg">
              {t.translations.CANT_SWITCH_TO_SINGLE_FILE}
            </h3>
            <p className="py-2">{t.translations.MULTI_FILE_WARNING}</p>
            <div className="modal-action">
              <button
                className="btn btn-secondary"
                onClick={() => setShowMultiFileWarning(false)}
              >
                {t.translations.OKAY}
              </button>
            </div>
          </div>
          <div
            className="modal-backdrop"
            onClick={() => setShowMultiFileWarning(false)}
          />
        </div>
      )}
    </div>
  );
}
