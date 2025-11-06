"use client";
import { useLanguage } from "@/app/contexts/Language";
import { createOauthApplication } from "@/app/lib/oauth_services.client";
import { useRouter } from "next/navigation";
import { useState } from "react";

interface CreateOAuthModalProps {
    isOpen: boolean; // Indicates whether the modal is open
    onClose: () => void; // Function to call when closing the modal
    onOAuthApplicationCreated: () => void;
}

// Main CreateProject component
const CreateOAuthModal = ({
    isOpen,
    onClose,
    onOAuthApplicationCreated,
}: CreateOAuthModalProps) => {
    const { t } = useLanguage();
    const [name, setName] = useState("");
    const [callback_url, setCallbackUrl] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    // TODO: Use the react hot toast ... it uses a lot less code
    const [toastMessage, setToastMessage] = useState("");
    const [toastType, setToastType] = useState<
        "success" | "error" | "info" | null
    >(null);

    const handleSubmit = async () => {
        let data;
        if (isLoading) return;
        setIsLoading(true);
        try {
            console.log(name, callback_url)
            data = await createOauthApplication({
                name,
                callback_url,
            });


            setName("");
            setCallbackUrl("");

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
            {/* Render the modal dialog if isOpen is true */}
            {isOpen && (
                <dialog className="modal modal-open">
                    <div className="modal-box max-w-lg">
                        <h3 className="font-bold text-lg mb-4 text-base-content">
                            {t.translations.CREATE_OAUTH_APPLICATION}
                        </h3>
                        <form
                            method="dialog"
                            className="flex flex-col gap-4"
                            onSubmit={(e) => {
                                e.preventDefault();
                                handleSubmit();
                            }}
                        >
                            <input
                                type="text"
                                placeholder={t.translations.NAME}
                                className="input input-bordered input-primary bg-base-100 text-base-content placeholder:text-base-content/40 w-full"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                required
                            />
                            <textarea
                                placeholder={t.translations.CALLBACK_URL} // Placeholder for project description
                                className="textarea textarea-bordered textarea-primary bg-base-100 text-base-content placeholder:text-base-content/40 min-h-[100px] w-full"
                                value={callback_url}
                                onChange={(e) => setCallbackUrl(e.target.value)}
                            />

                            {/* Modal Actions */}
                            <div className="modal-action mt-6">
                                <button
                                    type="button"
                                    className="btn btn-ghost"
                                    onClick={onClose}
                                >
                                    {t.translations.CANCEL}
                                </button>
                                <button type="submit" disabled={isLoading} aria-busy={isLoading} className="btn btn-primary">
                                    {isLoading ? (
                                        <>
                                            <span className="spinner" aria-hidden="true" />
                                        </>
                                    ) : (t.translations.CREATE)}
                                </button>
                            </div>
                        </form>
                    </div>
                </dialog>
            )}
        </>
    );
};

export default CreateOAuthModal;
