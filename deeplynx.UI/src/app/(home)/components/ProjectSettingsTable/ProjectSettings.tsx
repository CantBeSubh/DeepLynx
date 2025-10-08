"use client";

import React, { useState, useEffect, use, useCallback } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import Tabs from "../Tabs";
import AddProjectMember from "../../components/ProjectSettingsTable/ProjectModals/ProjectMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
// import DataSourceTable from '././ProjectTables/DataSourceTable';
// import ObjectStorageTable from '././ProjectTables/ObjectStorageTable';
// import MemberSearchBar from './MemberSearchBar';
import { useRouter, useSearchParams } from "next/navigation";
import { PlusIcon } from "@heroicons/react/24/outline";
import ProjectDropdown from '../ProjectDropdown';
import ProjectDropdownSingleSelect from '../ProjectDropdownSingleSelect';
import { ProjectMembersTable, ProjectsList, UserResponseDto, RoleResponseDto, MyRolesTable } from '../../types/types';
import { getAllUsers } from '@/app/lib/user_services.client';
import { getProjectMembers } from '@/app/lib/projects_services.client';
import { getAllRoles } from '@/app/lib/role_services.client';

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
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(
    initialProject?.id || null
  );
  const [projectMembers, setProjectMembers] = useState<ProjectMembersTable[]>([]);
  const [projectRoles, setProjectRoles] = useState<MyRolesTable[]>([]);
  //changes to ProjectResponseDto DTO && RoleResponseDto

  const [roles, setRoles] = useState([]);

  //Getting Project Roles
  useEffect(() => {
    const fetchRoles = async () => {
      const rolesData = await getAllRoles(Number(selectedProjectId));
      setRoles(rolesData);
    };
    fetchRoles();
  }, [selectedProjectId]);

  //Getting Project Members
  useEffect(() => {
    if (!selectedProjectId) return;
    (async () => {
      try {
        const users = await getProjectMembers(Number(selectedProjectId));
        setProjectMembers(users);
      } catch (err) {
        console.error(err);
      }
    })();
  }, [selectedProjectId]);

  //Refreshing Members Table
  const refreshMembers = async () => {
    if (selectedProjectId) {
      const users = await getProjectMembers(Number(selectedProjectId));
      setProjectMembers(users);
    }
  };

  //Tab Data for Project Settings Tables
  const tabData = [
    {
      label: "Members",
      content: (
        <MembersTable
          data={projectMembers}
          projectId={selectedProjectId}
          roles={roles}
        />
      ),
    },
    {
      label: "Roles",
      content: (
        <RolesTable
          id={selectedProjectId}
          data={projectRoles}
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

  //Function for adding roles
  const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
    event.preventDefault();
    if (activeTab === "Roles") {
      router.push("/project_settings/project_roles");
    } else if (activeTab === "Members") {
      setAddProjectMemberModal(true);
    }
  };

  //Function for changing a project from dropdown
  const handleProjectChange = useCallback((newProjectId: string) => {
    setSelectedProjectId(newProjectId);
  }, []);

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
        projectId={Number(selectedProjectId)}
        isOpen={addProjectMemberModal}
        onClose={() => setAddProjectMemberModal(false)}
        onMemberAdded={refreshMembers}
      />
    </div>
  );
};

export default ProjectSettings;