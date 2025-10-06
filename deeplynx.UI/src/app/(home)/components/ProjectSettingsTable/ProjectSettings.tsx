"use client";

import React, { useState, useEffect, use } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import { projectMembers, defaultRoles } from "../../dummy_data/data";
import Tabs from "../Tabs";
import AddProjectMember from "@/app/(home)/components/ProjectSettingsTable/ProjectModals/ProjectMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
// import DataSourceTable from '././ProjectTables/DataSourceTable';
// import ObjectStorageTable from '././ProjectTables/ObjectStorageTable';
// import MemberSearchBar from './MemberSearchBar';
import { useRouter, useSearchParams } from "next/navigation";
import { PlusIcon } from "@heroicons/react/24/outline";
import ProjectDropdown from '../ProjectDropdown';
import ProjectDropdownSingleSelect from '../ProjectDropdownSingleSelect';
import { ProjectsList, UserResponseDto } from '../../types/types';
import { getAllUsers } from '@/app/lib/user_services.client';

interface ProjectSettingsProps {
  projects: ProjectsList[];
  initialProject: ProjectsList | null;
}

const ProjectSettings = ({
  projects,
  initialProject,
}: ProjectSettingsProps) => {
  // const [selectedProjects, setSelectedProjects] = useState<string[]>(
  //   initialSelectedProjects
  // );
  const { t } = useLanguage();
  const [addProjectMemberModal, setAddProjectMemberModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Members");
  const router = useRouter();
  const searchParams = useSearchParams();
  const [project, setProject] = useState<ProjectsList | null>(initialProject);
  const [selectedProjectId, setSelectedProjectId] = useState(
    initialProject ? initialProject.id : null
  );
  // const [projectMembers, setProjectMembers] = useState<UserResponseDto>();

  useEffect(() => {
    (async () => {
      try {
        // const users = await getAllUsers();
        // setProjectMembers(users);
      } catch (err) {
        console.error(err);
      }
    })();
  }, []);

  const tabData = [
    {
      label: "Members",
      content: (
        <MembersTable
          data={projectMembers}
        />
      ),
    },
    {
      label: "Roles",
      content: (
        <RolesTable
          id={selectedProjectId}
          data={defaultRoles}
        />
      ),
    },
    //  TODO POST FY: ADD BACK DATA SOURCE / OBJ STORAGE
    // {
    //   label: "Data Source",
    //   content: (
    //     <DataSourceTable
    //       data={mySavedSearches}
    //     />
    //   ),
    // },
    // {
    //   label: "Object Storage",
    //   content: (
    //     <ObjectStorageTable
    //       data={mySavedSearches}
    //     />
    //   ),
    // },
  ];

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
    event.preventDefault();
    if (activeTab === "Roles") {
      router.push("/project_settings/project_roles");
    } else if (activeTab === "Members") {
      setAddProjectMemberModal(true);
    }
  };

  const handleProjectChange = (newProjectId: string) => {
    setSelectedProjectId(newProjectId);
  };

  // Effect to set the active tab from the query parameter
  useEffect(() => {
    const tab = searchParams.get('tab');
    if (tab) {
      setActiveTab(tab);
    }
  }, [searchParams]);

  return (
    <div className="">
      <div className="">
        <div className="">
          <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
            <div>
              <h1 className="text-2xl font-bold text-info-content">
                {t.translations.PROJECT_SETTINGS}
              </h1>

              <ProjectDropdownSingleSelect
                projects={projects}
                onSelectionChange={handleProjectChange}
                defaultSelectedId={initialProject?.id || ""}
              />
            </div>
          </div>
          <div className="flex justify-end space-x-4 pt-4">
            <button
              onClick={handleAddButtonClick}
              className="btn btn-secondary text-white"
            >
              <PlusIcon className="size-6" />
              {activeTab === "Members" ? t.translations.MEMBER : t.translations.ROLE}
            </button>
            <div className="flex flex-col">
              {/* TODO POST FY
    {activeTab === "Members" && <MemberSearchBar />} */}
            </div>
          </div>
        </div>
        <Tabs
          tabs={tabData}
          className="tabs tabs-border"
          onTabChange={handleTabChange}
          activeTab={activeTab}
        />
      </div>

      <AddProjectMember
        isOpen={addProjectMemberModal}
        onClose={() => setAddProjectMemberModal(false)}
      />
    </div>
  );
};

export default ProjectSettings;