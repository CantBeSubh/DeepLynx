import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { updateUser } from "@/app/lib/user_services.client";
import { updateOrganization } from "@/app/lib/organization_services.client";

interface EditOrganizationProps {
    isOpen: boolean;
    onClose: () => void;
    organizationId: number;
    organizationName: string;
    organizationDescription: string;
    onOrganizationUpdated: () => void;
}

// Main EditSysUser component
const EditOrganization = ({ isOpen, onClose, organizationId, organizationName, organizationDescription, onOrganizationUpdated }: EditOrganizationProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState(organizationName);
    const [description, setDescription] = useState(organizationDescription)

    useEffect(() => {
        if (isOpen) {
            setName(organizationName);
            setDescription(organizationDescription);
        }
    }, [isOpen, organizationName, organizationDescription]);

    const handleUpdate = async () => {
        try {
            await updateOrganization(organizationId, { name, description });
            onOrganizationUpdated();
        } catch (error) {
            console.error("Error updating organization:", error);
            alert("An error occurred while updating the organization.");
        }

        onClose();
    };

    return (
        <>
            {isOpen && (
                <dialog className="modal modal-open">
                    <div className="modal-box max-w-lg">
                        <h3 className="font-bold text-lg mb-4 text-neutral">
                            {t.translations.EDIT_ORGANIZATION}
                        </h3>
                        <label className="font-semibold text-sm text-neutral">
                            {t.translations.NAME}
                        </label>
                        <input
                            type="text"
                            placeholder="Name"
                            className="input input-primary w-full"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            required
                        />
                        <label className="font-semibold text-sm text-neutral">
                            {t.translations.DESCRIPTION}
                        </label>
                        <input
                            type="text"
                            placeholder="Description"
                            className="input input-primary w-full"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            required
                        />
                        <div className="modal-action">
                            <button type="button" className="btn" onClick={onClose}>
                                {t.translations.CANCEL}
                            </button>
                            <button type="submit" className="btn btn-primary" onClick={handleUpdate}>
                                {t.translations.SAVE}
                            </button>
                        </div>
                    </div>
                </dialog>
            )}
        </>
    );
};

export default EditOrganization;