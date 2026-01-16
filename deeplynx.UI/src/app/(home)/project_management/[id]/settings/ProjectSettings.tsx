// src/app/(home)/project_management/[id]/settings/ProjectSettings.tsx
"use client";

import { useState, useEffect } from "react";
import toast from "react-hot-toast";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  archiveProject,
  getProjectLogoUrl,
  removeProjectLogo,
  uploadProjectLogo,
} from "@/app/lib/client_service/projects_services.client";
import ArchiveDelete from "@/app/(home)/components/ArchiveDelete";
import { ProjectResponseDto } from "@/app/(home)/types/responseDTOs";

interface ProjectSettingsProps {
  project: ProjectResponseDto | null;
}

const ProjectSettings = ({ project }: ProjectSettingsProps) => {
  const { clearProject } = useProjectSession();
  const { organization } = useOrganizationSession();

  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isCheckingLogo, setIsCheckingLogo] = useState(true);

  // Load existing logo on mount
  useEffect(() => {
    const loadExistingLogo = async () => {
      if (!project?.id) {
        setIsCheckingLogo(false);
        return;
      }

      try {
        setIsCheckingLogo(true);
        const logoUrl = await getProjectLogoUrl(project.id as number);

        if (logoUrl) {
          setLogoPreview(logoUrl);
        }
      } catch (error) {
        console.error("Error checking for existing logo:", error);
      } finally {
        setIsCheckingLogo(false);
      }
    };

    loadExistingLogo();
  }, [project?.id]);

  const handleLogoChange = (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;

    const file = fileList[0];

    // Validate file type
    if (!file.type.startsWith("image/")) {
      toast.error("Please upload a valid image file (PNG recommended).");
      return;
    }

    // Validate file size (max 5MB)
    const maxSize = 5 * 1024 * 1024; // 5MB in bytes
    if (file.size > maxSize) {
      toast.error("File size must be less than 5MB");
      return;
    }

    setLogoFile(file);
    const previewUrl = URL.createObjectURL(file);
    setLogoPreview(previewUrl);
  };

  const handleUploadLogo = async () => {
    if (!organization?.organizationId || !project?.id || !logoFile) {
      toast.error("No file selected");
      return;
    }

    try {
      setIsUploading(true);

      const result = await uploadProjectLogo({
        organizationId: organization.organizationId as number,
        projectId: project.id as number,
        file: logoFile,
      });

      // Add timestamp to force browser to reload the image
      setLogoPreview(`${result.logoUrl}?t=${Date.now()}`);
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
    if (!organization?.organizationId || !project?.id) return;

    // Show confirmation dialog
    if (!confirm("Are you sure you want to remove the project logo?")) {
      return;
    }

    try {
      await removeProjectLogo({
        organizationId: organization.organizationId as number,
        projectId: project.id as number,
      });

      setLogoFile(null);
      setLogoPreview(null);
      toast.success("Logo removed successfully!");
    } catch (error) {
      console.error("Failed to remove logo:", error);
      toast.error("Failed to remove logo");
    }
  };

  const handleCancelSelection = async () => {
    setLogoFile(null);

    // Restore previous logo if it exists
    if (project?.id) {
      const logoUrl = await getProjectLogoUrl(project.id as number);
      setLogoPreview(logoUrl);
    } else {
      setLogoPreview(null);
    }
  };

  if (isCheckingLogo) {
    return (
      <div className="p-6 flex items-center justify-center min-h-[400px]">
        <span className="loading loading-spinner loading-lg"></span>
      </div>
    );
  }

  if (!project) {
    return (
      <div className="p-6">
        <div className="alert alert-warning">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            className="stroke-current shrink-0 h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
          <span>No project selected</span>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="mx-auto space-y-6">
        {/* Page Header */}
        <div className="border-b border-base-300 pb-4">
          <h2 className="text-2xl font-bold text-base-content">
            Project Settings
          </h2>
          <p className="text-base-content/70 text-sm mt-1">
            Configure branding and manage your project
          </p>
        </div>

        {/* Logo Section */}
        <div className="card bg-base-100 border border-primary/40 shadow-sm max-w-[40%]">
          <div className="card-body">
            <h3 className="card-title text-lg mb-4">Project Logo</h3>

            <div className="flex items-start gap-6 mb-6">
              {/* Logo Preview */}
              <div className="avatar">
                <div className="w-32 h-32 rounded-xl bg-base-200 flex items-center justify-center overflow-hidden border-2 border-base-300">
                  {logoPreview ? (
                    <img
                      src={logoPreview}
                      alt="Project Logo"
                      className="object-contain w-full h-full p-2"
                      onError={() => {
                        // If image fails to load, clear preview
                        setLogoPreview(null);
                      }}
                    />
                  ) : (
                    <div className="text-center p-4">
                      <span className="text-base-content/40 text-sm">
                        No Logo
                      </span>
                    </div>
                  )}
                </div>
              </div>

              {/* Logo Controls */}
              <div className="flex flex-col gap-3 flex-1">
                <div>
                  <span className="font-semibold text-lg block">
                    {project?.name || "Project"}
                  </span>
                  <span className="text-sm text-base-content/60">
                    Project Logo
                  </span>
                </div>

                <div className="flex flex-wrap gap-2">
                  <label className="btn btn-sm btn-primary">
                    {logoFile ? "Change Logo" : "Select Logo"}
                    <input
                      type="file"
                      accept=".png,.jpg,.jpeg,.svg,.webp"
                      className="hidden"
                      onChange={(e) => handleLogoChange(e.target.files)}
                    />
                  </label>

                  {logoFile && (
                    <>
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

                      <button
                        type="button"
                        className="btn btn-sm btn-ghost"
                        onClick={handleCancelSelection}
                        disabled={isUploading}
                      >
                        Cancel
                      </button>
                    </>
                  )}

                  {logoPreview && !logoFile && (
                    <button
                      type="button"
                      className="btn btn-sm btn-error btn-outline"
                      onClick={handleRemoveLogo}
                    >
                      Remove Logo
                    </button>
                  )}
                </div>

                {logoFile && (
                  <div className="alert alert-info">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                      className="stroke-current shrink-0 w-6 h-6"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth="2"
                        d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                      ></path>
                    </svg>
                    <span className="text-sm">
                      Click "Upload" to save your changes
                    </span>
                  </div>
                )}

                <div className="text-xs text-base-content/60 bg-base-200 p-3 rounded-lg">
                  <p className="font-semibold mb-1">Logo Guidelines:</p>
                  <ul className="list-disc list-inside space-y-1">
                    <li>Replaces the folder icon next to the project name</li>
                    <li>Recommended: PNG with transparent background</li>
                    <li>Optimal size: 256×256 pixels</li>
                    <li>Maximum file size: 5MB</li>
                    <li>Supported formats: PNG, JPG, SVG, WebP</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Archive Project Section */}
        <div className="mt-8">
          <ArchiveDelete
            actionType="archive"
            itemType="Project"
            itemName={project?.name || ""}
            onConfirm={async () => {
              if (organization && project) {
                await archiveProject(
                  organization.organizationId as number,
                  project.id as number,
                  true
                );
              }
              clearProject();
              window.location.href = "/";
            }}
          />
        </div>
      </div>
    </div>
  );
};

export default ProjectSettings;
