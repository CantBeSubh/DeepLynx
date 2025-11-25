"use client";

import { useLanguage } from "@/app/contexts/Language";
import {
  OrganizationSession,
  useOrganizationSession,
} from "@/app/contexts/OrganizationSessionProvider";
import { updateOrganization } from "@/app/lib/client_service/organization_services.client";
import { PencilIcon } from "@heroicons/react/24/outline";
import { useState } from "react";

interface OrganizationSettingsProps {
  organization: OrganizationSession;
}

const OrganizationSettings = ({ organization }: OrganizationSettingsProps) => {
  const { t } = useLanguage();
  const [isEditingName, setIsEditingName] = useState(false);
  const [editedName, setEditedName] = useState(organization.organizationName);
  const [currentOrganization, setCurrentOrganization] = useState(organization);
  const { setOrganization } = useOrganizationSession();

  const handleSaveName = async () => {
    setIsEditingName(true);
    try {
      updateOrganization(organization.organizationId as number, {
        name: editedName,
      });
      setOrganization({
        organizationName: editedName,
        organizationId: organization.organizationId,
      });
    } catch (error) {
      console.error("Failed to create organization", error);
    } finally {
      setIsEditingName(false);
    }
  };

  const handleCancelEdit = () => {
    setEditedName(organization.organizationName);
    setIsEditingName(false);
  };

  return (
    <div className="p-6">
      <div className="max-w-md space-y-6">
        <div className="shadow-md card-body rounded-md">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Organization Name
          </label>
          {isEditingName ? (
            <div className="space-y-2">
              <input
                type="text"
                value={editedName}
                onChange={(e) => setEditedName(e.target.value)}
                className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                placeholder="Enter organization name"
              />
              <div className="flex space-x-2">
                <button
                  onClick={handleSaveName}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition"
                >
                  Save
                </button>
                <button
                  onClick={handleCancelEdit}
                  className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-300 transition"
                >
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <div className="flex items-center justify-between">
              <span className="text-lg font-semibold text-gray-900">
                {organization.organizationName}
              </span>
              <PencilIcon
                className="text-primary hover:text-primary-focus size-6 cursor-pointer transition-colors"
                onClick={() => setIsEditingName(true)}
              />
            </div>
          )}

          <label
            htmlFor="org-logo"
            className="block text-sm font-medium text-gray-700 mt-4"
          >
            Organization Logo
          </label>
          <div className="flex items-center space-x-4">
            <div className="w-20 h-20 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center bg-gray-50">
              <svg
                className="w-8 h-8 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
            <label htmlFor="org-logo" className="cursor-pointer">
              <span className="w-full border border-gray-300 rounded-lg px-4 py-2.5 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition bg-gray-100 text-gray-500 cursor-not-allowed">
                Choose File
              </span>
              <input
                id="org-logo"
                type="file"
                className="hidden"
                accept="image/*"
                disabled
              />
            </label>
          </div>
          <p className="text-xs text-gray-500">PNG, JPG, GIF up to 10MB</p>
        </div>
      </div>
    </div>
  );
};

export default OrganizationSettings;
