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
const EditOrganizataion = ({ isOpen, onClose, organizationId, organizationName, organizationDescription, onOrganizationUpdated }: EditOrganizationProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState(organizationName);
    const [description, setDescription] = useState(organizationDescription)

    useEffect(() => {
        if (isOpen) {
            setName(organizationName);
        }
    }, [isOpen, organizationName]);

    const handleUpdate = async (e: React.FormEvent) => {
        try {
            await updateOrganization(organizationId, { name, description });
            onOrganizationUpdated();
            alert("Organization updated successfully!");
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
                        <form className="flex flex-col gap-4" onSubmit={handleUpdate}>
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
                                <button type="submit" className="btn btn-primary">
                                    {t.translations.SAVE}
                                </button>
                            </div>
                        </form>
                    </div>
                </dialog>
            )}
        </>
    );
};

export default EditOrganizataion;