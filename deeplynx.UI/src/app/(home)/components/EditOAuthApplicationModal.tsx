import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { updateUser } from "@/app/lib/user_services.client";
import { updateOauthApplication } from "@/app/lib/oauth_services.client";

interface EditOAuthApplicationProps {
    isOpen: boolean;
    onClose: () => void;
    oAuthApplicationId: number;
    oAuthApplicationName: string;
    oAuthApplicationCallbackURL: string;
    onOAuthApplicationUpdated: () => void;
}

// Main EditSysUser component
const EditOAuthApplication = ({ isOpen, onClose, oAuthApplicationId, oAuthApplicationName, oAuthApplicationCallbackURL, onOAuthApplicationUpdated }: EditOAuthApplicationProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState(oAuthApplicationName);
    const [callbackUrl, setCallbackURL] = useState(oAuthApplicationCallbackURL)

    useEffect(() => {
        if (isOpen) {
            setName(oAuthApplicationName);
        }
    }, [isOpen, oAuthApplicationName]);

    const handleUpdate = async (e: React.FormEvent) => {
        try {
            await updateOauthApplication(oAuthApplicationId, { name, callbackUrl });
            onOAuthApplicationUpdated();
            alert("OAuthApplication updated successfully!");
        } catch (error) {
            console.error("Error updating oAuthApplication:", error);
            alert("An error occurred while updating the oAuthApplication.");
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
                                placeholder="CallbackURL"
                                className="input input-primary w-full"
                                value={callbackUrl}
                                onChange={(e) => setCallbackURL(e.target.value)}
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

export default EditOAuthApplication;