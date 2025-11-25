// src/app/(home)/components/AddRecordModal.tsx
"use client";

import React, { useEffect, useMemo, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";

import { DataSourceResponseDto } from "../types/responseDTOs";
import toast from "react-hot-toast";
import { isAxiosError } from "axios";
import { ProjectResponseDto } from "../types/responseDTOs";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { CreateRecordRequestDto } from "../types/requestDTOs";
import { getAllDataSources } from "@/app/lib/client_service/data_source_services.client";
import { createRecord } from "@/app/lib/client_service/record_services.client";
import { CreateRecordPayload } from "../types/types";
type JsonValue = Record<string, unknown>;

type Props = {
  isOpen: boolean;
  onClose: () => void;
  initialProjects: ProjectResponseDto[] | { id: string; name: string }[];
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

  const [dataSources, setDataSources] = useState<DataSourceResponseDto[]>([]);
  const [selectedDataSourceId, setSelectedDataSourceId] = useState<
    number | undefined
  >();
  const [dsLoading, setDsLoading] = useState(false);
  const [dsError, setDsError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Required fields
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [abbreviation, setAbbreviation] = useState("");
  const [propertiesText, setPropertiesText] = useState("");
  const [propertiesError, setPropertiesError] = useState<string | null>(null);
  const { organization, hasLoaded } = useOrganizationSession();

  // Optional fields
  const [objectStorageId, setObjectStorageId] = useState<string>("");
  const [classId, setClassId] = useState<number | undefined>();
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
      if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
        setPropertiesError(t.translations.MUST_BE_SINGLE_JSON_OBJECT);
      } else {
        setPropertiesError(null);
      }
    } catch {
      setPropertiesError(t.translations.INVALID_JASON);
    }
  };

  const resetForm = () => {
    // selections
    setSelectedProjectId(undefined);
    setDataSources([]);
    setSelectedDataSourceId(undefined);
    setDsError(null);

    // required
    setName("");
    setDescription("");
    setAbbreviation("");
    setPropertiesText("");
    setPropertiesError(null);

    // optional
    setObjectStorageId("");
    setClassId(undefined);
    setUri("");
    setClassNameOpt("");
    setTagsText("");
    setLabelsText("");

    setIsSubmitting(false);
  };

  const handleClose = () => {
    resetForm();
    onClose();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting) return;
    setIsSubmitting(true);

    if (selectedProjectId === undefined) {
      toast.error(t.translations.PLEASE_SELECT_A_PROJECT);
      setIsSubmitting(false);
      return;
    }
    if (selectedDataSourceId === undefined) {
      toast.error(t.translations.PLEASE_SELECT_A_DATA_SOURCE);
      setIsSubmitting(false);
      return;
    }

    let props: JsonValue;
    try {
      const parsed = JSON.parse(propertiesText);

      // If user pasted an array with one object, be helpful and take the first one.
      if (Array.isArray(parsed)) {
        if (
          parsed.length === 1 &&
          typeof parsed[0] === "object" &&
          parsed[0] !== null
        ) {
          props = parsed[0] as JsonValue;
        } else {
          throw new Error(t.translations.MUST_BE_SINGLE_JSON_OBJECT);
        }
      } else if (parsed && typeof parsed === "object") {
        props = parsed as JsonValue;
      } else {
        throw new Error(t.translations.MUST_BE_SINGLE_JSON_OBJECT);
      }
      setPropertiesError(null);
    } catch (err) {
      setPropertiesError(err instanceof Error ? err.message : "Invalid JSON.");
      setIsSubmitting(false);
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
      setIsSubmitting(false);
      return;
    }

    const tags = tagsText.trim() ? parseCommaList(tagsText) : undefined;
    const sensitivity_labels = labelsText.trim()
      ? parseCommaList(labelsText)
      : undefined;

    const payload: CreateRecordPayload = {
      name,
      description,
      original_id: abbreviation,
      properties: props, // now guaranteed to be a single object
      class_id: classId,
    };
    const dto: CreateRecordRequestDto = {
      name,
      description,
      original_id: abbreviation,
      properties: JSON.stringify(props),
      class_id: classId,
    };

    if (objectStorageIdNum !== undefined)
      payload.object_storage_id = objectStorageIdNum;
    if (uri.trim()) payload.uri = uri.trim();
    if (classNameOpt.trim()) payload.class_name = classNameOpt.trim();
    if (tags?.length) payload.tags = tags;
    if (sensitivity_labels?.length)
      payload.sensitivity_labels = sensitivity_labels;

    try {
      await createRecord(
        organization!.organizationId as number,
        selectedProjectId,
        selectedDataSourceId,
        dto
      );
      toast.success(t.translations.RECORD_CREATED_SECCESSFULLY);
      resetForm();
      onClose();
    } catch (error) {
      console.error("Error creating record:", error);
      toast.error(t.translations.FAILED_TO_CREATE_RECORD);
    } finally {
      setIsSubmitting(false);
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
        const list = await getAllDataSources(
          organization?.organizationId as number,
          selectedProjectId
        );
        if (!cancelled) setDataSources(list ?? []);
      } catch (err: unknown) {
        const fallback = t.translations.FAILED_TO_LOAD_DATA_SOURCE;
        let message = fallback;

        if (isAxiosError(err)) {
          const data = err.response?.data as unknown;

          if (typeof data === "string") {
            message = data;
          } else if (
            data &&
            typeof data === "object" &&
            "message" in data &&
            typeof (data as { message?: string }).message === "string"
          ) {
            message = (data as { message?: string }).message!;
          }
        }

        if (!cancelled) setDsError(message);
      } finally {
        if (!cancelled) setDsLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [selectedProjectId, t.translations]);

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
                  {dsLoading
                    ? t.translations.LOADING
                    : t.translations.SELECT_A_DATA_SOURCE}
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
                onChange={(e) =>
                  setClassId(
                    e.target.value === "" ? undefined : Number(e.target.value)
                  )
                }
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
            <button type="button" className="btn" onClick={handleClose}>
              {t.translations.CANCEL}
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? t.translations.SAVING : t.translations.SAVE}
            </button>
          </div>
        </form>
      </div>
    </dialog>
  );
};

export default AddRecordModal;
