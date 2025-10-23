"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import Tabs from "../Tabs";
import AddProjectMember from "../../components/ProjectSettingsTable/ProjectModals/ProjectMemberModal";
import MembersTable from '././ProjectTables/MembersTable';
import RolesTable from '././ProjectTables/RolesTable';
import { useRouter, useSearchParams } from "next/navigation";
import { PlusIcon } from "@heroicons/react/24/outline";
import ProjectDropdownSingleSelect from '../ProjectDropdownSingleSelect';
import { getProjectMembers } from '@/app/lib/projects_services.client';
import { getAllRoles } from '@/app/lib/role_services.client';
import { ProjectResponseDto, RoleResponseDto, ProjectMembersDto } from '../../types/responseDTOs';
import ProjectSettingsMemberSkeleton from '../skeletons/projectsettingsmemberskeleton';
import { Role } from '@/app/(home)/types/types';

interface ProjectSettingsProps {
  projects: ProjectResponseDto[];
  initialProject: ProjectResponseDto | null;
}

const ProjectSettings = ({
  projects,
  initialProject,
}: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addProjectMemberModal, setAddProjectMemberModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Members");
  const router = useRouter();
  const searchParams = useSearchParams();
  // const [project, setProject] = useState<ProjectResponseDto[] | null>(initialProject);
  const [selectedProjectId, setSelectedProjectId] = useState<string | number | null>(
    initialProject?.id ?? null
  );
  const [projectMembers, setProjectMembers] = useState<ProjectMembersDto[]>([]);
  const [projectRoles, setProjectRoles] = useState<RoleResponseDto[]>([]);
  const [isMembersLoading, setIsMembersLoading] = useState(true);

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

//   //normalize roles
//   const rolesForTable: Role[] = roleResponseDtos.map(r => ({
//   ...r,
//   description: r.description ?? null, // ensure string|null (never undefined)
// }));

const memberContent = isMembersLoading || selectedProjectId == null ? <ProjectSettingsMemberSkeleton/>:
<MembersTable
  data={projectMembers}
  projectId={selectedProjectId == null
      ? null
      : String(selectedProjectId)}
  roles={roles}
/>;

  //Tab Data for Project Settings Tables
  const tabData = [
    {
      label: "Members",
      content: (
        memberContent
      ),
    },
    {
      label: "Roles",
      content: (
        <RolesTable
          id={selectedProjectId == null ? undefined : String(selectedProjectId)}
          data={roles}
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
      router.push(`/project/${selectedProjectId}/project_settings/project_roles/new_role`);
    } else if (activeTab === "Members") {
      setAddProjectMemberModal(true);
    }
  };

  //Function for changing a project from dropdown
  const handleProjectChange = useCallback((newProjectId: string) => {
    setSelectedProjectId(newProjectId);
  }, []);

  // Effect to set the active tab from the query parameter
  const tab = searchParams.get('tab');
  useEffect(() => {
    if (tab) {
      setActiveTab(tab);
    }
  }, [tab]);

  return (
    <div className="">
      <div className="">
        <div className="">
          <div className="flex justify-between items-center bg-base-200/40 pl-12 py-2">
            <div className="">
              <h1 className="text-2xl font-bold text-info-content">
                {t.translations.PROJECT_SETTINGS}
              </h1>

              <ProjectDropdownSingleSelect
                projects={projects}
                onSelectionChange={handleProjectChange}
                defaultSelectedId={selectedProjectId === undefined
                  || selectedProjectId === null
                  ? undefined
                  : String(selectedProjectId)}
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