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
    oAuthApplicationDescription: string;
    oAuthApplicationBaseURL: string;
    oAuthApplicationAppOwnerEmail: string;
    onOAuthApplicationUpdated: () => void;
}

// Main EditSysUser component
const EditOAuthApplication = ({ isOpen, onClose, oAuthApplicationId, oAuthApplicationName, oAuthApplicationCallbackURL, oAuthApplicationDescription, oAuthApplicationBaseURL, oAuthApplicationAppOwnerEmail, onOAuthApplicationUpdated }: EditOAuthApplicationProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState(oAuthApplicationName || "");
    const [callbackUrl, setCallbackURL] = useState(oAuthApplicationCallbackURL || "")
    const [description, setDescription] = useState(oAuthApplicationDescription || "");
    const [baseUrl, setBaseUrl] = useState(oAuthApplicationBaseURL || "");
    const [appOwnerEmail, setAppOwnerEmail] = useState(oAuthApplicationAppOwnerEmail || "");


    useEffect(() => {
        if (isOpen) {
            setName(oAuthApplicationName ?? "");
            setDescription(oAuthApplicationDescription ?? "")
            setCallbackURL(oAuthApplicationCallbackURL ?? "")
            setBaseUrl(oAuthApplicationBaseURL ?? "")
            setAppOwnerEmail(oAuthApplicationAppOwnerEmail ?? "")
        }
    }, [isOpen, oAuthApplicationName, oAuthApplicationDescription, oAuthApplicationCallbackURL, oAuthApplicationBaseURL, oAuthApplicationAppOwnerEmail]);

    const handleUpdate = async (e: React.FormEvent) => {
        try {
            await updateOauthApplication(oAuthApplicationId, { name, description, callbackUrl, baseUrl, appOwnerEmail });
            onOAuthApplicationUpdated();
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
                            {t.translations.EDIT_OAUTH_APP}
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
                                {t.translations.CALLBACK_URL}
                            </label>
                            <input
                                type="text"
                                placeholder="CallbackURL"
                                className="input input-primary w-full"
                                value={callbackUrl}
                                onChange={(e) => setCallbackURL(e.target.value)}
                                required
                            />
                            <label className="font-semibold text-sm text-neutral">
                                {t.translations.DESCRIPTION}
                            </label>
                            <textarea
                                placeholder={t.translations.DESCRIPTION} // Placeholder for project description
                                className="textarea textarea-bordered textarea-primary bg-base-100 text-base-content placeholder:text-base-content/40 min-h-[100px] w-full"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                            />
                            <label className="font-semibold text-sm text-neutral">
                                {t.translations.BASE_URL}
                            </label>
                            <input
                                placeholder={t.translations.BASE_URL} // Placeholder for project description
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={baseUrl}
                                onChange={(e) => setBaseUrl(e.target.value)}
                            />
                            <label className="font-semibold text-sm text-neutral">
                                {t.translations.APP_OWNER_EMAIL}
                            </label>
                            <input
                                placeholder={t.translations.APP_OWNER_EMAIL} // Placeholder for project description
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={appOwnerEmail}
                                onChange={(e) => setAppOwnerEmail(e.target.value)}
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