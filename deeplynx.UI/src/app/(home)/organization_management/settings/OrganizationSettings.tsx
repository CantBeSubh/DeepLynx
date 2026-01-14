// src/app/(home)/organization_management/settings/OrganizationSettings.tsx
"use client";

import { useState, useEffect } from "react";
import toast from "react-hot-toast";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  getOrganizationLogoUrl,
  uploadOrganizationLogo,
  removeOrganizationLogo,
} from "@/app/lib/client_service/organization_services.client";

const OrganizationSettings = () => {
  const { organization } = useOrganizationSession();

  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  // Load existing logo on mount
  useEffect(() => {
    if (organization?.organizationId) {
      const logoUrl = getOrganizationLogoUrl(
        organization.organizationId as number
      );
      // We'll verify if the file exists in Phase 2
      setLogoPreview(logoUrl);
    }
  }, [organization?.organizationId]);

  const handleLogoChange = (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;

    const file = fileList[0];

    // Validate file type
    if (!file.type.startsWith("image/")) {
      toast.error("Please upload a valid image file (PNG recommended).");
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      toast.error("File size must be less than 5MB");
      return;
    }

    setLogoFile(file);
    const previewUrl = URL.createObjectURL(file);
    setLogoPreview(previewUrl);
  };

  const handleUploadLogo = async () => {
    if (!organization?.organizationId || !logoFile) {
      toast.error("No file selected");
      return;
    }

    try {
      setIsUploading(true);

      const result = await uploadOrganizationLogo({
        organizationId: organization.organizationId as number,
        file: logoFile,
      });

      setLogoPreview(result.logoUrl);
      setLogoFile(null);
      toast.success("Logo uploaded successfully!");
    } catch (error) {
      console.error("Failed to upload logo:", error);
      toast.error(
        error instanceof Error ? error.message : "Failed to upload logo"
      );
    } finally {
      setIsUploading(false);
    }
  };

  const handleRemoveLogo = async () => {
    if (!organization?.organizationId) return;

    try {
      await removeOrganizationLogo({
        organizationId: organization.organizationId as number,
      });

      setLogoFile(null);
      setLogoPreview(null);
      toast.success("Logo removed successfully!");
    } catch (error) {
      console.error("Failed to remove logo:", error);
      toast.error("Failed to remove logo");
    }
  };

  return (
    <div className="p-6">
      <div className="max-w-4xl mx-auto">
        <h2 className="text-2xl font-bold text-base-content mb-6">
          Organization Settings
        </h2>

        <div className="card bg-base-100 border border-primary/40 shadow-sm">
          <div className="card-body">
            <h3 className="card-title text-lg mb-4">Organization Logo</h3>

            <div className="flex items-center gap-4 mb-6">
              <div className="avatar">
                <div className="w-24 h-24 rounded-xl bg-base-200 flex items-center justify-center overflow-hidden">
                  {logoPreview ? (
                    <img
                      src={logoPreview}
                      alt="Organization Logo"
                      className="object-cover w-full h-full"
                    />
                  ) : (
                    <span className="text-base-content/40 text-sm">
                      No Logo
                    </span>
                  )}
                </div>
              </div>

              <div className="flex flex-col gap-2 flex-1">
                <span className="font-semibold text-lg">
                  {organization?.organizationName || "Organization"}
                </span>

                <div className="flex gap-2">
                  <label className="btn btn-sm btn-primary">
                    Select Logo
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/svg+xml"
                      className="hidden"
                      onChange={(e) => handleLogoChange(e.target.files)}
                    />
                  </label>

                  {logoFile && (
                    <button
                      type="button"
                      className="btn btn-sm btn-success"
                      onClick={handleUploadLogo}
                      disabled={isUploading}
                    >
                      {isUploading && (
                        <span className="loading loading-spinner loading-xs" />
                      )}
                      Upload
                    </button>
                  )}

                  {logoPreview && !logoFile && (
                    <button
                      type="button"
                      className="btn btn-sm btn-ghost"
                      onClick={handleRemoveLogo}
                    >
                      Remove
                    </button>
                  )}
                </div>

                <p className="text-xs text-base-content/60">
                  Appears next to the organization name in the header.
                  Recommended: PNG, 256×256, transparent background. Max size:
                  5MB.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrganizationSettings;
