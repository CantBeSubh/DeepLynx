// src/app/(home)/upload_center/UploadCenterClient.tsx
"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useCallback, useEffect, useMemo, useState } from "react";
import DropUpload from "../components/DropUpload";
import FileDetailsCard from "../components/FileDetailCard";
import NewFileUploadCard, {
  FileMetadata,
} from "../components/NewFileUploadCard";
import RecentUploadsCard from "../components/RecentUploadsCard";
import SelectedFilesCard from "../components/SelectedFilesCard";
import { ExistingFile, RecentUpload, UploadType } from "../types/upload";
import { getAllProjects } from "@/app/lib/projects_services.client";
import {
  DataSourceDTO,
  getAllDataSources,
} from "@/app/lib/data_source_services.client";
import {
  getAllObjectStorages,
  ObjectStorageDTO,
} from "@/app/lib/object_storage_services.client";
import {
  uploadFile,
  uploadFilesBatch,
} from "@/app/lib/file_upload_services.client";
import toast from "react-hot-toast";
import { ProjectResponseDto } from "../types/responseDTOs";

type Props = {
  initialAvailableFiles: ExistingFile[];
  initialRecentUploads: RecentUpload[];
  uploadText: string;
};

export default function UploadCenterClient({
  initialAvailableFiles,
  initialRecentUploads,
  uploadText,
}: Props) {
  const { t } = useLanguage();
  const [multi, setMulti] = useState(false);
  const [showMultiFileWarning, setShowMultiFileWarning] = useState(false);
  const [uploadType, setUploadType] = useState<UploadType>("new");
  const [targetFileId, setTargetFileId] = useState("");
  const [destination, setDestination] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const showRightPanel = selectedFiles.length > 0;
  const [projects, setProjects] = useState<ProjectResponseDto[]>([]);
  const [objectStorage, setObjectstorage] = useState<ObjectStorageDTO[]>([]);
  const [dataSources, setDataSources] = useState<DataSourceDTO[]>([]);
  const [projectId, setProjectId] = useState<string>("");
  const [dataSourceId, setDataSourceId] = useState<string>("");
  const [objectStorageId, setObjectstorageId] = useState<string>("");
  const [filesMetadata, setFilesMetadata] = useState<
    Record<number, FileMetadata>
  >({});
  const [isLoadingDataSources, setIsLoadingDataSources] = useState(false);
  const [isLoadingObjectStorage, setIsLoadingObjectStorage] = useState(false);

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
    if (
      !projectId ||
      !dataSourceId ||
      !objectStorageId ||
      selectedFiles.length === 0
    ) {
      toast.error(
        "Select project, data source, object storage, and at least one file."
      );
      return;
    }

    try {
      if (selectedFiles.length === 1) {
        const file = selectedFiles[0];
        const metadata = filesMetadata[0];

        await uploadFile({
          projectId,
          dataSourceId,
          objectStorageId,
          file,
          name: metadata?.name || file.name,
          description: metadata?.description || "",
        });
        toast.success("File uploaded!");
      } else {
        const uploadPromises = selectedFiles.map(async (file, index) => {
          const metadata = filesMetadata[index];

          return uploadFile({
            projectId,
            dataSourceId,
            objectStorageId,
            file,
            name: metadata?.name || file.name,
            description: metadata?.description || "",
          });
        });

        const results = await Promise.allSettled(uploadPromises);
        const ok = results.filter((r) => r.status === "fulfilled").length;
        const fail = results.length - ok;
        toast.success(
          `Uploaded ${ok} file(s)${fail ? ` • ${fail} failed` : ""}`
        );
      }
      resetForm({ keepProject: true });
    } catch (error) {
      toast.error("Upload failed. See console for details.");
      console.error(error);
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
        const objectStorage = await getAllObjectStorages(projectId);
        const storageData = objectStorage?.data ?? objectStorage;
        setObjectstorage(storageData);
        if (storageData.length === 1) {
          setObjectstorageId(String(storageData[0].id));
        }
        setIsLoadingObjectStorage(false);
      } catch (error) {
        console.error("Error fetching object storage:", error);
        setObjectstorage([]);
        setIsLoadingObjectStorage(false);
      }
    })();
  }, [projectId]);

  useEffect(() => {
    (async () => {
      try {
        const data = await getAllProjects();
        setProjects(data);
        if (data.length === 1) {
          setProjectId(String(data[0].id));
        }
      } catch (error) {
        console.error(error);
      }
    })();
  }, []);

  return (
    <div>
      <div className="flex items-center bg-base-200/40 py-2 pl-12">
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
          <h2>{t.translations.START_UPLOAD_BY_CHOOSING_TYPE}</h2>
          <div className="p-4 space-y-4">
            <fieldset className="space-x-4">
              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">Select a project</span>
                <select
                  value={projectId}
                  onChange={(e) => setProjectId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                >
                  <option value="" disabled>
                    {t.translations.PROJECT}
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
                  required
                  disabled={!projectId || isLoadingObjectStorage}
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
            <fieldset>
              <label className="label text-base-content font-bold">
                {t.translations.UPLOADING}
                <select
                  value={uploadType}
                  onChange={(e) => setUploadType(e.target.value as UploadType)}
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
          </div>
        </div>

        {/* RIGHT — render only when needed */}
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
