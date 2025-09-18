"use client";

import React, { useEffect, useMemo, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { createRecord } from "@/app/lib/record_services.client";
import type { ProjectsList } from "@/app/(home)/types/types";
import {
  getAllDataSources,
  type DataSourceDTO,
} from "@/app/lib/data_source_services.client";
import toast from "react-hot-toast";

type JsonValue = Record<string, unknown> | Record<string, unknown>[];

type Props = {
  isOpen: boolean;
  onClose: () => void;
  initialProjects: ProjectsList[];
};

const AddRecordModal: React.FC<Props> = ({
  isOpen,
  onClose,
  initialProjects,
}) => {
  const { t } = useLanguage();

  // Project/Data Source
  const initialProjectId = useMemo(
    () => (initialProjects.length ? Number(initialProjects[0].id) : undefined),
    [initialProjects]
  );
  const [selectedProjectId, setSelectedProjectId] = useState<
    number | undefined
  >(initialProjectId);

  const [dataSources, setDataSources] = useState<DataSourceDTO[]>([]);
  const [selectedDataSourceId, setSelectedDataSourceId] = useState<
    number | undefined
  >();
  const [dsLoading, setDsLoading] = useState(false);
  const [dsError, setDsError] = useState<string | null>(null);

  // Required fields
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [abbreviation, setAbbreviation] = useState("");
  const [propertiesText, setPropertiesText] = useState("");
  const [propertiesError, setPropertiesError] = useState<string | null>(null);

  // Optional fields
  const [objectStorageId, setObjectStorageId] = useState<string>("");
  const [classId, setClassId] = useState<number>();
  const [uri, setUri] = useState<string>("");
  const [classNameOpt, setClassNameOpt] = useState<string>("");
  const [tagsText, setTagsText] = useState<string>("");
  const [labelsText, setLabelsText] = useState<string>("");

  const parseCommaList = (text: string) =>
    text
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean);

  const onPropertiesChange = (val: string) => {
    setPropertiesText(val);
    try {
      const parsed = JSON.parse(val);
      if (parsed && typeof parsed === "object") {
        setPropertiesError(null);
      } else {
        setPropertiesError("Must be a JSON object or array.");
      }
    } catch {
      setPropertiesError("Invalid JSON.");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (selectedProjectId === undefined) {
      toast.error("Please select a project.");
      return;
    }
    if (selectedDataSourceId === undefined) {
      toast.error("Please select a data source.");
      return;
    }

    let props: JsonValue;
    try {
      const parsed = JSON.parse(propertiesText);
      if (!parsed || typeof parsed !== "object") throw new Error();
      props = parsed as JsonValue;
    } catch {
      setPropertiesError("Invalid JSON.");
      return;
    }

    const objectStorageIdNum =
      objectStorageId.trim() === "" ? undefined : Number(objectStorageId);
    if (
      objectStorageIdNum !== undefined &&
      (!Number.isInteger(objectStorageIdNum) ||
        Number.isNaN(objectStorageIdNum))
    ) {
      toast.error("object_storage_id must be an integer if provided.");
      return;
    }

    const tags = tagsText.trim() ? parseCommaList(tagsText) : undefined;
    const sensitivity_labels = labelsText.trim()
      ? parseCommaList(labelsText)
      : undefined;

    const payload: any = {
      name,
      description,
      original_id: abbreviation,
      class_id: classId,
      properties: props,
    };

    if (objectStorageIdNum !== undefined)
      payload.object_storage_id = objectStorageIdNum;
    if (uri.trim()) payload.uri = uri.trim();
    if (classNameOpt.trim()) payload.class_name = classNameOpt.trim();
    if (tags?.length) payload.tags = tags;
    if (sensitivity_labels?.length)
      payload.sensitivity_labels = sensitivity_labels;

    try {
      await createRecord(selectedProjectId, payload, {
        dataSourceId: selectedDataSourceId,
      });
      toast.success("Record created successfully!");
      onClose();
    } catch (error) {
      console.error("Error creating record:", error);
      toast.error("Failed to create record.");
    }
  };

  useEffect(() => {
    if (!selectedProjectId) {
      setDataSources([]);
      setSelectedDataSourceId(undefined);
      setDsError(null);
      return;
    }

    let cancelled = false;
    (async () => {
      try {
        setDsLoading(true);
        setDsError(null);
        setSelectedDataSourceId(undefined);
        const list = await getAllDataSources(selectedProjectId);
        if (!cancelled) setDataSources(list ?? []);
      } catch (err: any) {
        if (!cancelled)
          setDsError(err?.response?.data ?? "Failed to load data sources");
      } finally {
        if (!cancelled) setDsLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [selectedProjectId]);

  return (
    <dialog className={`modal ${isOpen ? "modal-open" : ""}`}>
      <div className="modal-box">
        <h3 className="text-base-content font-bold text-lg mb-4">
          {t.translations.ADD_A_RECORD}
        </h3>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          {/* Project + Data Source */}
          <div className="flex gap-3">
            <div className="w-1/2">
              <label className="label">
                <span className="label-text">
                  {t.translations.SELECT_PROJECT}
                </span>
              </label>
              <select
                className="select select-primary w-full"
                value={selectedProjectId ?? ""}
                onChange={(e) =>
                  setSelectedProjectId(
                    e.target.value === "" ? undefined : Number(e.target.value)
                  )
                }
                required
              >
                <option value="" disabled>
                  {t.translations.SELECT_PROJECT}
                </option>
                {initialProjects.map((p) => (
                  <option key={p.id} value={String(p.id)}>
                    {p.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="w-1/2">
              <label className="label">
                <span className="label-text">
                  {t.translations.SELECT_DATA_SOURCE}
                </span>
              </label>
              <select
                className="select select-primary w-full"
                value={selectedDataSourceId ?? ""}
                onChange={(e) =>
                  setSelectedDataSourceId(
                    e.target.value === "" ? undefined : Number(e.target.value)
                  )
                }
                disabled={
                  !selectedProjectId || dsLoading || dataSources.length === 0
                }
                required
              >
                <option value="" disabled>
                  {dsLoading ? "Loading..." : "Select a data source..."}
                </option>
                {dataSources.map((ds) => (
                  <option key={ds.id} value={String(ds.id)}>
                    {ds.name}
                  </option>
                ))}
              </select>
              {dsError && <p className="text-error text-sm my-1">{dsError}</p>}
            </div>
          </div>

          {/* Required */}
          <input
            type="text"
            className="input input-primary w-full"
            placeholder={t.translations.NAME}
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
          />

          <input
            type="text"
            className="input input-primary w-full"
            placeholder={t.translations.ORIGINAL_ID}
            required
            value={abbreviation}
            onChange={(e) => setAbbreviation(e.target.value)}
          />

          <textarea
            placeholder={t.translations.DESCRIPTION}
            className="textarea textarea-primary w-full"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
          />

          <textarea
            placeholder={t.translations.PROPERTIES_EXAMPLE}
            className="textarea textarea-primary w-full h-40 font-mono"
            value={propertiesText}
            onChange={(e) => onPropertiesChange(e.target.value)}
            required
          />
          {propertiesError && (
            <p className="text-error text-sm">{propertiesError}</p>
          )}

          {/* Optional */}
          <div className="collapse collapse-arrow border border-base-300 bg-base-100 rounded-lg">
            <input type="checkbox" />
            <div className="collapse-title text-md font-medium">
              {t.translations.OPTIONAL_FIELDS}
            </div>
            <div className="collapse-content flex flex-col gap-4">
              <input
                type="number"
                inputMode="numeric"
                className="input input-bordered w-full"
                placeholder="object_storage_id"
                value={objectStorageId}
                onChange={(e) => setObjectStorageId(e.target.value)}
              />

              <input
                type="number"
                inputMode="numeric"
                className="input input-bordered w-full"
                placeholder="class_id"
                value={classId ?? ""}
                onChange={(e) => setClassId(Number(e.target.value))}
              />

              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="uri"
                value={uri}
                onChange={(e) => setUri(e.target.value)}
              />

              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="class_name"
                value={classNameOpt}
                onChange={(e) => setClassNameOpt(e.target.value)}
              />

              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="tags (comma-separated)"
                value={tagsText}
                onChange={(e) => setTagsText(e.target.value)}
              />

              <input
                type="text"
                className="input input-bordered w-full"
                placeholder="sensitivity_labels (comma-separated)"
                value={labelsText}
                onChange={(e) => setLabelsText(e.target.value)}
              />
            </div>
          </div>

          <div className="modal-action">
            <button type="button" className="btn" onClick={onClose}>
              {t.translations.CANCEL}
            </button>
            <button type="submit" className="btn btn-primary">
              {t.translations.SAVE}
            </button>
          </div>
        </form>
      </div>
    </dialog>
  );
};

export default AddRecordModal;
