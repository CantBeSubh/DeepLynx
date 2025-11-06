"use client";
import { useLanguage } from "@/app/contexts/Language";
import { createOauthApplication } from "@/app/lib/oauth_services.client";
import { useState } from "react";

interface CreateOAuthModalProps {
    isOpen: boolean;
    onClose: () => void;
    onOAuthApplicationCreated: () => void;
}

const CreateOAuthModal = ({
    isOpen,
    onClose,
    onOAuthApplicationCreated,
}: CreateOAuthModalProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState("");
    const [callbackUrl, setCallbackUrl] = useState("");
    const [description, setDescription] = useState("");
    const [baseUrl, setBaseUrl] = useState("");
    const [appOwnerEmail, setAppOwnerEmail] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [toastMessage, setToastMessage] = useState("");
    const [toastType, setToastType] = useState<
        "success" | "error" | "info" | null
    >(null);

    const handleSubmit = async () => {
        if (isLoading) return;
        setIsLoading(true);
        try {
            await createOauthApplication({
                name,
                callbackUrl,
                description,
                baseUrl,
                appOwnerEmail
            });

            setName("");
            setCallbackUrl("");
            setDescription("");
            setBaseUrl("");
            setAppOwnerEmail("");

            setTimeout(() => {
                onOAuthApplicationCreated();
                setToastMessage("");
                setToastType(null);
                onClose();
            }, 1000);
        } catch (error) {
            console.error("Failed to create OAuth Application", error);
            setToastType("error");
            setToastMessage("Failed to create OAuth Application");

            setTimeout(() => {
                setToastMessage("");
                setToastType(null);
            }, 2000);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <>
            {/* Toast Message */}
            {toastMessage && toastType && (
                <div className="toast toast-top toast-end z-50">
                    <div className={`alert alert-${toastType}`}>
                        <span>{toastMessage}</span>
                    </div>
                </div>
            )}
            {isOpen && (
                <dialog className="modal modal-open">
                    <div className="modal-box max-w-lg">
                        <h3 className="font-bold text-lg mb-4 text-base-content">
                            {t.translations.CREATE_OAUTH_APPLICATION}
                        </h3>

                        <div className="flex flex-col gap-4">
                            <input
                                type="text"
                                placeholder={t.translations.NAME}
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                required
                            />
                            <input
                                placeholder={t.translations.CALLBACK_URL}
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={callbackUrl}
                                onChange={(e) => setCallbackUrl(e.target.value)}
                            />
                            <textarea
                                placeholder={t.translations.DESCRIPTION}
                                className="textarea textarea-bordered textarea-primary bg-base-100 text-base-content placeholder:text-base-content/40 min-h-[100px] w-full"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                            />
                            <input
                                placeholder={t.translations.BASE_URL}
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={baseUrl}
                                onChange={(e) => setBaseUrl(e.target.value)}
                            />
                            <input
                                placeholder={t.translations.APP_OWNER_EMAIL}
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={appOwnerEmail}
                                onChange={(e) => setAppOwnerEmail(e.target.value)}
                            />
                        </div>

                        <div className="modal-action mt-6">
                            <button
                                type="button"
                                className="btn btn-ghost"
                                onClick={onClose}
                            >
                                {t.translations.CANCEL}
                            </button>
                            <button
                                type="button"
                                disabled={isLoading}
                                aria-busy={isLoading}
                                className="btn btn-primary"
                                onClick={handleSubmit}
                            >
                                {isLoading ? (
                                    <span className="spinner" aria-hidden="true" />
                                ) : (
                                    t.translations.CREATE
                                )}
                            </button>
                        </div>
                    </div>
                </dialog>
            )}
        </>
    );
};

export default CreateOAuthModal;