// src/app/(home)/components/AddTagModal.tsx
"use client";

import React, { useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import type { Tag } from "@/app/(home)/types/types";
import { createTag } from "@/app/lib/tag_services.client";
import { TagResponseDto } from "@/app/(home)/types/responseDTOs";

type Props = {
  isOpen: boolean;
  onClose: () => void;
  projectId: number;
  onTagCreated?: (newTag: TagResponseDto) => void;
};

const AddTagModal: React.FC<Props> = ({
  isOpen,
  onClose,
  projectId,
  onTagCreated,
}) => {
  const { t } = useLanguage();
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Required fields
  const [name, setName] = useState("");

  const resetForm = () => {
    setName("");
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

    const payload: Tag = {
      name,
    };

    try {
      const newTag = await createTag(projectId, payload); // Capture the returned tag

      // Call the callback with the newly created tag
      if (onTagCreated && newTag) {
        onTagCreated(newTag);
      }

      resetForm();
      onClose();
    } catch (error) {
      console.error("Error creating tag:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <dialog className={`modal ${isOpen ? "modal-open" : ""}`}>
      <div className="modal-box">
        <h3 className="text-base-content font-bold text-lg mb-4">
          {t.translations.ADD_A_TAG}
        </h3>
        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <input
            type="text"
            className="input input-primary w-full"
            placeholder={t.translations.NAME}
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
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
