// src/app/(home)/project_management/[id]/ProjectManagementClient.tsx

"use client";

import React, { useState } from "react";
import Tabs from "@/app/(home)/components/Tabs";
import SideMenu from "@/app/(home)/components/SideMenu";
import {
  GroupResponseDto,
  ProjectResponseDto,
  UserResponseDto,
  RoleResponseDto,
  PermissionResponseDto,
  ProjectMemberResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { useLanguage } from "@/app/contexts/Language";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import ProjectUsersTable from "./users/ProjectUsersTable";
import ProjectRolesAndPermissions from "./roles_and_permissions/ProjectRolesAndPermissions";
import DataSources from "./data_source/DataSourcesClient";
import ProjectSecurityLabelsPanel from "./tag_management/ProjectTagAndLabelManagementClient";
import ProjectTagAndLabelManagementClient from "./tag_management/ProjectTagAndLabelManagementClient";
import { archiveProject } from "@/app/lib/client_service/projects_services.client"
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { redirect } from "next/navigation";

interface ProjectManagementProps {
  project: ProjectResponseDto | null;
  projectMembers: ProjectMemberResponseDto[];
  projectGroups: GroupResponseDto[];
  projectRoles: RoleResponseDto[];
  projectPermissions: PermissionResponseDto[];
}

const ProjectManagementClient = ({
  project,
  projectMembers,
  projectGroups,
  projectRoles,
  projectPermissions,
}: ProjectManagementProps) => {
  const [activeTab, setActiveTab] = useState("");
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const { t } = useLanguage();
  const { project: sessionProject } = useProjectSession();
  const {organization, hasLoaded } = useOrganizationSession()

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  const handleArchive = () => {
    if (organization && project){
      archiveProject(organization.organizationId as number, project.id as number, true)
    }
    setArchiveConfirm(false)
    redirect("/")
  }

  const [archiveConfirm, setArchiveConfirm] = useState(false)

  // Match SideMenu widths: w-22 (collapsed) vs w-64 (expanded)
  const contentMarginLeft = isSidebarCollapsed ? "ml-32" : "ml-72";

  const tabData = [
    {
      label: "Users",
      content: (
        <ProjectUsersTable
          members={projectMembers}
          roles={projectRoles}
          project={project}
        />
      ),
    },
    {
      label: "Roles & Permissions",
      content: (
        <ProjectRolesAndPermissions
          initialRoles={projectRoles}
          initialPermissions={projectPermissions}
          projectId={project?.id as number}
        />
      ),
    },
    {
      label: "Data Sources",
      content: <DataSources projectId={project?.id as number} />,
    },
    {
      label: "Tags & Security Labels",
      content: (
        <ProjectTagAndLabelManagementClient
          project={project as ProjectResponseDto}
          orgTagsLocked={false}
        />
      ),
    },
    {
      label: "Settings",
      content: project ? (
        <div className="space-y-2">

          <div className="border-b border-base-300 pb-4 mt-4">
            <h1 className="text-2xl font-semibold text-base-content">Project Settings</h1>
            <p className="text-sm text-base-content mt-1">
              Manage your project
            </p>
          </div>

          <div className="border border-red-200 bg-red-600/5 rounded-lg p-4 max-w-2xl">
            <div className="flex items-center gap-2">
              <svg className="w-5 h-5 text-red-600" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="m20.25 7.5-.625 10.632a2.25 2.25 0 0 1-2.247 2.118H6.622a2.25 2.25 0 0 1-2.247-2.118L3.75 7.5m8.25 3v6.75m0 0-3-3m3 3 3-3M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125Z" />
              </svg>
              <h3 className="font-semibold text-base-content">Archive Project</h3>
            </div>
            <p className="text-sm text-base-content mt-1">
              Archive this project to remove it from your active projects. Archived projects can be restored later.
            </p>
            <button
              onClick={() => setArchiveConfirm(true)}
              className="px-4 py-2 mt-4 border border-red-600 text-xs text-red-600 font-medium rounded-lg hover:bg-red-600 hover:text-white transition-colors whitespace-nowrap"
            >
              Archive Project: {sessionProject?.projectName}
            </button>
          </div>

          {archiveConfirm && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
              <div className="bg-white rounded-lg max-w-md w-full p-6 shadow-xl">
                <div className="flex items-start gap-3">
                  <div className="bg-red-100 rounded-full p-2 shrink-0">
                    <svg className="w-6 h-6 text-red-600" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z" />
                    </svg>
                  </div>
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-black">
                      Archive Project
                    </h3>
                    <p className="text-sm text-black mt-2">
                      Are you sure you want to archive this project? You'll be able to restore it later from your archived projects.
                    </p>
                  </div>
                </div>

                <div className="flex gap-3 mt-6 justify-end">
                  <button
                    onClick={() => setArchiveConfirm(false)}
                    className="px-4 py-2 text-black border border-base-300 rounded-lg font-medium hover:bg-base-200/70 hover:border-base-200/50 hover:text-white transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleArchive}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg font-medium hover:bg-red-600 transition-colors"
                  >
                    Archive Project
                  </button>
                </div>
              </div>
            </div>
          )}

        </div>
      ) : (
        <div>No project selected</div>
      ),
    },
  ];

  return (
    <>
      <div className="bg-base-200/40 pl-12 p-6">
        <h1 className="text-2xl font-bold text-base-content">
          {t.translations.PROJECT_MANAGEMENT || "Project Management"}
        </h1>
        {(project || sessionProject) && (
          <p className="text-sm text-base-content/70 mt-1">
            Managing settings for project:{" "}
            <span className="font-semibold">
              {project?.name || sessionProject?.projectName}
            </span>
          </p>
        )}
      </div>

      {/* Tabs */}
      <div className="p-2">
        <Tabs
          tabs={tabData}
          className="tabs tabs-border ml-5"
          onTabChange={handleTabChange}
          activeTab={activeTab}
        />
      </div>
    </>
  );
};

export default ProjectManagementClient;
