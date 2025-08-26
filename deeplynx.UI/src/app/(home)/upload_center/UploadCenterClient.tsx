"use client";

import React, { useEffect, useMemo, useState } from "react";
import DropUpload from "../components/DropUpload";
import NewFileUploadCard from "../components/NewFileUploadCard";
import RecentUploadsCard from "../components/RecentUploadsCard";
import SelectedFilesCard from "../components/SelectedFilesCard";
import { ExistingFile, RecentUpload, UploadType } from "../types/upload";
import FileDetailsCard from "../components/FileDetailCard";
import { translations } from "@/app/lib/translations";

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
  const t = translations["en"];
  const [multi, setMulti] = useState(false);
  const [showMultiFileWarning, setShowMultiFileWarning] = useState(false);
  const [uploadType, setUploadType] = useState<UploadType>("");
  const [targetFileId, setTargetFileId] = useState("");
  const [destination, setDestination] = useState("");
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);

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

  useEffect(() => {
    if (!needsTarget) setTargetFileId("");
  }, [needsTarget]);

  const removeAt = (idx: number) =>
    setSelectedFiles((prev) => prev.filter((_, i) => i !== idx));
  const clearAll = () => setSelectedFiles([]);
  const handleUpload = () =>
    alert(`Uploading ${selectedFiles.length} file(s)…`);

  // inside component state/logic
  const isMultiAllowed = uploadType === "new";

  // If user switches away from "new", force multi off
  useEffect(() => {
    if (!isMultiAllowed && multi) setMulti(false);
  }, [isMultiAllowed, multi]);

  // Use this so DropUpload never gets multiple=true unless allowed
  const effectiveMultiple = isMultiAllowed ? multi : false;

  return (
    <div>
      <div className="flex items-center bg-base-200/40 py-2 pl-12">
        <h1 className="text-2xl font-bold text-info-content">
          {t.translations.UPLOAD_CENTER}
        </h1>
      </div>

      <div className="flex lg:flex-grow justify-between gap-8 p-10 lg:p-20">
        {/* LEFT */}
        <div className="w-full lg:w-3/5">
          <h2>{t.translations.START_UPLOAD_BY_CHOOSING_TYPE}</h2>
          <div className="p-4 space-y-4">
            <fieldset>
              <label className="label cursor-pointer justify-start gap-3">
                <span className="label-text text-sm">
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
              {!isMultiAllowed && (
                <p id="multi-files-hint" className="text-xs opacity-60 mt-1">
                  {t.translations.MULTI_FILES_ONLY_AVAILABLE}.
                </p>
              )}
            </fieldset>

            <fieldset>
              <label className="label">
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
              <label className="label">
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

        {/* RIGHT */}
        <div className="lg:w-2/5">
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
        </div>
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
