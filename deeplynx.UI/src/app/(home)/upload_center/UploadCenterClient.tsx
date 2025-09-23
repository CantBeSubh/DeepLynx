// src/app/(home)/upload_center/UploadCenterClient.tsx

"use client";

import { useLanguage } from "@/app/contexts/Language";
import { useEffect, useMemo, useState } from "react";
import DropUpload from "../components/DropUpload";
import FileDetailsCard from "../components/FileDetailCard";
import NewFileUploadCard from "../components/NewFileUploadCard";
import RecentUploadsCard from "../components/RecentUploadsCard";
import SelectedFilesCard from "../components/SelectedFilesCard";
import { ExistingFile, RecentUpload, UploadType } from "../types/upload";
import { getAllProjects } from "@/app/lib/projects_services.client";
import type { ProjectDTO } from "@/app/lib/projects_services.server";
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
  const [uploadType, setUploadType] = useState<UploadType>("");
  const [targetFileId, setTargetFileId] = useState("");
  const [destination, setDestination] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const showRightPanel = selectedFiles.length > 0;
  const [projects, setProjects] = useState<ProjectDTO[]>([]);
  const [objectStorage, setObjectstorage] = useState<ObjectStorageDTO[]>([]);
  const [dataSources, setDataSources] = useState<DataSourceDTO[]>([]);
  const [projectId, setProjectId] = useState<string>("");
  const [dataSourceId, setDataSourceId] = useState<string>("");
  const [objectStorageId, setObjectstorageId] = useState<string>("");

  const needsTarget = uploadType === "version" || uploadType === "properties";
  const availableFiles = useMemo(
    () => initialAvailableFiles,
    [initialAvailableFiles]
  );
  const recentUploads = useMemo(
    () => initialRecentUploads,
    [initialRecentUploads]
  );
  const selectedTarget = useMemo(
    () => availableFiles.find((f) => f.id === targetFileId) ?? null,
    [availableFiles, targetFileId]
  );

  const [dropKey, setDropKey] = useState(0); // force-remount DropUpload to clear its <input type="file" />

  const resetForm = (opts?: { keepProject?: boolean }) => {
    const keepProject = opts?.keepProject ?? true;

    setSelectedFiles([]);
    setUploadType("");
    setDestination("");
    setTargetFileId("");
    setMulti(false);

    if (!keepProject) {
      setProjectId("");
      setDataSources([]);
      setObjectstorage([]);
    }
    setDataSourceId("");
    setObjectstorageId("");

    // bump key so DropUpload clears its native file input
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
        await uploadFile({
          projectId,
          dataSourceId,
          objectStorageId,
          file: selectedFiles[0],
        });
        toast.success("File uploaded!");
      } else {
        const results = await uploadFilesBatch({
          projectId,
          dataSourceId,
          objectStorageId,
          files: selectedFiles,
        });
        const ok = results.filter((r) => r.status === "fulfilled").length;
        const fail = results.length - ok;
        toast.success(
          `Uploaded ${ok} file(s)${fail ? ` • ${fail} failed` : ""}`
        );
      }

      // 👉 reset everything (but keep project by default)
      resetForm({ keepProject: true });
    } catch (e: any) {
      const msg =
        e?.response?.data ??
        e?.response?.statusText ??
        e?.message ??
        "Upload failed. See console for details.";
      toast.error(String(msg));
      console.error(e);
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
      return;
    }

    (async () => {
      try {
        const ds = await getAllDataSources(Number(projectId));
        const os = await getAllObjectStorages(projectId);
        setDataSources(ds);
        setObjectstorage(os?.data ?? os);
        setDataSourceId("");
        setObjectstorageId("");
      } catch (error) {
        console.error(error);
        setDataSources([]);
        setObjectstorage([]);
        setDataSourceId("");
        setObjectstorageId("");
      }
    })();
  }, [projectId]);

  useEffect(() => {
    (async () => {
      try {
        const data = await getAllProjects();
        setProjects(data);
      } catch (error) {
        console.error(error);
      }
    })();
  }, []);

  useEffect(() => {
    if (!projectId) {
      setDataSources([]);
      return;
    }

    (async () => {
      try {
        const dataSources = await getAllDataSources(Number(projectId));
        const objectStorageResponse = await getAllObjectStorages(projectId);
        setDataSources(dataSources);
        setObjectstorage(objectStorageResponse.data);
      } catch (error) {
        console.error(error);
        setDataSources([]);
      }
    })();
  }, [projectId]);

  // Use this so DropUpload never gets multiple=true unless allowed
  const effectiveMultiple = isMultiAllowed ? multi : false;

  return (
    <div>
      <div className="flex items-center bg-base-200/40 py-2 pl-12">
        <h1 className="text-2xl font-bold text-base-content">
          {t.translations.UPLOAD_CENTER}
        </h1>
      </div>

      {/* <div className="flex lg:flex-grow justify-between gap-8 p-10 lg:p-20"> */}
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
                    Project
                  </option>
                  {projects.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">Data Source</span>
                <select
                  value={dataSourceId}
                  onChange={(e) => setDataSourceId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                  disabled={!projectId}
                >
                  <option value="" disabled>
                    {projectId ? "Data Sources" : "Select a project first"}
                  </option>
                  {dataSources.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name}
                    </option>
                  ))}
                </select>
              </label>
              <label className="label flex-col items-start text-base-content font-bold">
                <span className="label-text mb-1">Object Storage</span>
                <select
                  value={objectStorageId}
                  onChange={(e) => setObjectstorageId(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                  disabled={!projectId}
                >
                  <option value="" disabled>
                    {projectId ? "Object storages" : "Select a project first"}
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
              {/* {!isMultiAllowed && (
                <p id="multi-files-hint" className="text-xs opacity-60 mt-1">
                  {t.translations.MULTI_FILES_ONLY_AVAILABLE}.
                </p>
              )} */}
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
                  <option value="" disabled>
                    {t.translations.CHOOSE_A_TYPE}
                  </option>
                  <option value="new">{t.translations.NEW_FILE}</option>
                  <option value="version" disabled={multi}>
                    {t.translations.NEW_VERSION_OF}
                  </option>
                  <option value="properties" disabled={multi}>
                    {t.translations.PROPERTIES_FOR}
                  </option>
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
              <label className="label text-base-content font-bold">
                {t.translations.TO_DESTINATION}
                <select
                  value={destination}
                  onChange={(e) => setDestination(e.target.value)}
                  className="select select-info select-sm mt-2"
                  required
                >
                  <option value="" disabled>
                    {t.translations.CHOOSE_A_DESTINATION}
                  </option>
                  <option value="nexus">{t.translations.NEXUS_DEFAULT}</option>
                  <option value="remote-db">{t.translations.REMOTE_DB}</option>
                  <option value="onsite-db">{t.translations.ONSITE_DB}</option>
                </select>
              </label>
            </fieldset>

            {(multi || selectedFiles.length === 0) && (
              <DropUpload
                key={dropKey}
                multiple={multi}
                files={selectedFiles}
                onFilesChange={setSelectedFiles}
                disabled={
                  !uploadType || !destination || (needsTarget && !targetFileId)
                }
              />
            )}

            {selectedFiles.length >= 1 &&
              selectedFiles.map((file, index) => (
                <NewFileUploadCard
                  key={index}
                  defaultName={file.name}
                  uploadType={uploadType}
                />
              ))}
          </div>
        </div>

        {/* RIGHT — render only when needed */}
        {showRightPanel && (
          <div className="lg:w-2/5">
            {/* Keep these so the functionality “when something is uploaded” appears */}
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
        {/* <div className="lg:w-2/5">
          {selectedFiles.length === 0 &&
            (uploadType === "" || uploadType === "new") && (
              <RecentUploadsCard
                uploads={recentUploads}
                uploadText={uploadText}
              />
            )}

          <FileDetailsCard
            needsTarget={needsTarget}
            selectedTarget={selectedTarget}
          />

          <SelectedFilesCard
            files={selectedFiles}
            onRemoveAt={removeAt}
            onClear={clearAll}
            onUpload={handleUpload}
          />
        </div> */}
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
