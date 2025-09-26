// src/app/(home)/components/AddRecordModal.tsx
"use client";

import React, { useEffect, useMemo, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import {
    createRecord,
    type CreateRecordPayload,
} from "@/app/lib/record_services.client";
import type { ProjectsList } from "@/app/(home)/types/types";
import {
    getAllDataSources,
    type DataSourceDTO,
} from "@/app/lib/data_source_services.client";
import toast from "react-hot-toast";
import { isAxiosError } from "axios";

type JsonValue = Record<string, unknown>;

type Props = {
    isOpen: boolean;
    onClose: () => void;
};

const AddTagModal: React.FC<Props> = ({
    isOpen,
    onClose,
}) => {
    const { t } = useLanguage();

    // Project/Data Source
    // const initialProjectId = useMemo(
    //     () => (initialProjects.length ? Number(initialProjects[0].id) : undefined),
    //     [initialProjects]
    // );
    // const [selectedProjectId, setSelectedProjectId] = useState<
    //     number | undefined
    // >(initialProjectId);

    const [dataSources, setDataSources] = useState<DataSourceDTO[]>([]);
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
        // setSelectedProjectId(undefined);
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

        // if (selectedProjectId === undefined) {
        //     toast.error(t.translations.PLEASE_SELECT_A_PROJECT);
        //     setIsSubmitting(false);
        //     return;
        // }
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

        if (objectStorageIdNum !== undefined)
            payload.object_storage_id = objectStorageIdNum;
        if (uri.trim()) payload.uri = uri.trim();
        if (classNameOpt.trim()) payload.class_name = classNameOpt.trim();
        if (tags?.length) payload.tags = tags;
        if (sensitivity_labels?.length)
            payload.sensitivity_labels = sensitivity_labels;

        // try {
        //     await createRecord(selectedProjectId, payload, {
        //         dataSourceId: selectedDataSourceId,
        //     });
        //     toast.success(t.translations.RECORD_CREATED_SECCESSFULLY);
        //     resetForm();
        //     onClose();
        // } catch (error) {
        //     console.error("Error creating record:", error);
        //     toast.error(t.translations.FAILED_TO_CREATE_RECORD);
        // } finally {
        //     setIsSubmitting(false);
        // }
    };

    // useEffect(() => {
    //     if (!selectedProjectId) {
    //         setDataSources([]);
    //         setSelectedDataSourceId(undefined);
    //         setDsError(null);
    //         return;
    //     }

    //     let cancelled = false;
    //     (async () => {
    //         try {
    //             setDsLoading(true);
    //             setDsError(null);
    //             setSelectedDataSourceId(undefined);
    //             const list = await getAllDataSources(selectedProjectId);
    //             if (!cancelled) setDataSources(list ?? []);
    //         } catch (err: unknown) {
    //             const fallback = t.translations.FAILED_TO_LOAD_DATA_SOURCE;
    //             let message = fallback;

    //             if (isAxiosError(err)) {
    //                 const data = err.response?.data as unknown;

    //                 if (typeof data === "string") {
    //                     message = data;
    //                 } else if (
    //                     data &&
    //                     typeof data === "object" &&
    //                     "message" in data &&
    //                     typeof (data as { message?: string }).message === "string"
    //                 ) {
    //                     message = (data as { message?: string }).message!;
    //                 }
    //             }

    //             if (!cancelled) setDsError(message);
    //         } finally {
    //             if (!cancelled) setDsLoading(false);
    //         }
    //     })();

    //     return () => {
    //         cancelled = true;
    //     };
    // }, [selectedProjectId, t.translations]);

    return (
        <dialog className={`modal ${isOpen ? "modal-open" : ""}`}>
            <div className="modal-box">
                <h3 className="text-base-content font-bold text-lg mb-4">
                    {t.translations.ADD_A_TAG}
                </h3>

                <form className="flex flex-col gap-4" onSubmit={handleSubmit}>


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

export default AddTagModal;
