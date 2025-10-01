// "use client";

// import React, { useState, useEffect } from "react";
// import { useLanguage } from "@/app/contexts/Language";
// import { systemUsers, systemGroups, systemOrgs } from "../../dummy_data/data";
// import Tabs from "../Tabs";
// import { useRouter, useSearchParams } from "next/navigation";
// // import MemberSearchBar from "../ProjectSettingsTable/MemberSearchBar";
// import { PlusIcon } from "@heroicons/react/24/outline";
// import UsersTable from "./MemberTables/UsersTable";
// import GroupsTable from "./MemberTables/GroupsTable";
// import OrganizationsTable from "./MemberTables/OrganizationsTable";
// import AddSysUser from "./MemberModals/AddSysUser";
// import AddGroup from "./MemberModals/AddGroup";
// import AddOrg from "./MemberModals/AddOrg";

// interface ProjectSettingsProps {
//   className?: string;
// }

// const MemberSettings = ({ className }: ProjectSettingsProps) => {
//   const { t } = useLanguage();
//   const [addSysUserModal, setAddSysUserModal] = useState(false);
//   const [addGroupModal, setAddGroupModal] = useState(false);
//   const [addOrgModal, setAddOrgModal] = useState(false);
//   const [activeTab, setActiveTab] = useState("Members");
//   const router = useRouter();
//   const searchParams = useSearchParams();

//   const tabData = [
//     {
//       label: "Users",
//       content: <UsersTable />,
//     },
//     {
//       label: "Groups",
//       content: <GroupsTable data={systemGroups} />,
//     },
//     {
//       label: "Organizations",
//       content: <OrganizationsTable data={systemOrgs} />,
//     },
//   ];

//   const handleTabChange = (label: string) => {
//     setActiveTab(label);
//   };

//   const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
//     event.preventDefault();
//     if (activeTab === "Users") {
//       setAddSysUserModal(true);
//     } else if (activeTab === "Groups") {
//       setAddGroupModal(true);
//     } else if (activeTab === "Organizations") {
//       setAddOrgModal(true);
//     }
//   };

//   // Effect to set the active tab from the query parameter
//   useEffect(() => {
//     const tab = searchParams.get("tab");
//     if (tab) {
//       setActiveTab(tab);
//     }
//   }, [searchParams]);

//   return (
//     <div className="bg-base-100 text-accent-content rounded-xl shadow-md card">
//       <div className="card-body">
//         <div className="flex justify-between items-start">
//           <h2 className="card-title">{t.translations.MEMBER_MANAGEMENT}</h2>
//           <div className="flex space-x-4">
//             <button
//               onClick={handleAddButtonClick}
//               className="btn btn-secondary text-white"
//             >
//               <PlusIcon className="size-6" />
//               {activeTab === "Users"
//                 ? t.translations.USER
//                 : activeTab === "Groups"
//                 ? t.translations.GROUP
//                 : activeTab === "Organizations"
//                 ? t.translations.ORGANIZATION
//                 : t.translations.USER}
//             </button>
//             {/* TODO POST FY REQUIREMENT
//             <div className="flex flex-col">
//               {activeTab === "Users" && <MemberSearchBar />}
//             </div> */}
//           </div>
//         </div>
//         <Tabs
//           tabs={tabData}
//           className="tabs tabs-border"
//           onTabChange={handleTabChange}
//           activeTab={activeTab}
//         />
//       </div>

//     <AddSysUser
//         isOpen={addSysUserModal}
//         onClose={() => setAddSysUserModal(false)}
//     />

//     <AddGroup
//         isOpen={addGroupModal}
//         onClose={() => setAddGroupModal(false)}
//     />
//     <AddOrg
//         isOpen={addOrgModal}
//         onClose={() => setAddOrgModal(false)}
//     />

//     </div>
//   );
// };

// export default MemberSettings;

// "use client";

// import React, { useState, useEffect } from "react";
// import { useLanguage } from "@/app/contexts/Language";
// import Tabs from "../Tabs";
// import { useRouter, useSearchParams } from "next/navigation";
// import { PlusIcon } from "@heroicons/react/24/outline";
// import UsersTable from "./MemberTables/UsersTable";
// import GroupsTable from "./MemberTables/GroupsTable";
// import OrganizationsTable from "./MemberTables/OrganizationsTable";
// import AddSysUser from "./MemberModals/AddSysUser";
// import AddGroup from "./MemberModals/AddGroup";
// import AddOrg from "./MemberModals/AddOrg";
// import { systemOrgs } from "../../dummy_data/data";
// import { createGroup } from "@/app/lib/group_services.client";

// interface ProjectSettingsProps {
//   className?: string;
// }

// const MemberSettings = ({ className }: ProjectSettingsProps) => {
//   const { t } = useLanguage();
//   const [addSysUserModal, setAddSysUserModal] = useState(false);
//   const [addGroupModal, setAddGroupModal] = useState(false);
//   const [addOrgModal, setAddOrgModal] = useState(false);
//   const [activeTab, setActiveTab] = useState("Users");
//   const router = useRouter();
//   const searchParams = useSearchParams();
//   const [groups, setGroups] = useState([]); // Initialize with an empty array or fetch data as needed

//   const tabData = [
//     {
//       label: "Users",
//       content: <UsersTable />,
//     },
//     {
//       label: "Groups",
//       content: <GroupsTable data={groups} />, // Pass groups data to GroupsTable
//     },
//     {
//       label: "Organizations",
//       content: <OrganizationsTable data={systemOrgs} />,
//     },
//   ];

//   // Handle tab changes
//   const handleTabChange = (label: string) => {
//     setActiveTab(label);
//   };

//   // Handle button click to open modals
//   const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
//     event.preventDefault();
//     if (activeTab === "Users") {
//       setAddSysUserModal(true);
//     } else if (activeTab === "Groups") {
//       setAddGroupModal(true);
//     } else if (activeTab === "Organizations") {
//       setAddOrgModal(true);
//     }
//   };

//   // Function to handle adding a new group via API
//   const handleAddGroup = async (groupName: string, description: string) => {
//     try {
//       const newGroup = await createGroup(groupName, description); // Call the createGroup service
//       setGroups((prevGroups) => [...prevGroups, newGroup]); // Update state with the new group
//     } catch (error) {
//       console.error('Error adding group:', error);
//       // Optionally show an error message to the user
//     }
//   };

//   // Effect to set the active tab from the query parameter
//   useEffect(() => {
//     const tab = searchParams.get("tab") || "Users"; // Default to "Users"
//     setActiveTab(tab);
//   }, [searchParams]);

//   return (
//     <div className="bg-base-100 text-accent-content rounded-xl shadow-md card">
//       <div className="card-body">
//         <div className="flex justify-between items-start">
//           <h2 className="card-title">{t.translations.MEMBER_MANAGEMENT}</h2>
//           <div className="flex space-x-4">
//             <button
//               onClick={handleAddButtonClick}
//               className="btn btn-secondary text-white"
//             >
//               <PlusIcon className="size-6" />
//               {activeTab === "Users"
//                 ? t.translations.USER
//                 : activeTab === "Groups"
//                 ? t.translations.GROUP
//                 : activeTab === "Organizations"
//                 ? t.translations.ORGANIZATION
//                 : t.translations.USER}
//             </button>
//           </div>
//         </div>
//         <Tabs
//           tabs={tabData}
//           className="tabs tabs-border"
//           onTabChange={handleTabChange}
//           activeTab={activeTab}
//         />
//       </div>

//       <AddSysUser
//         isOpen={addSysUserModal}
//         onClose={() => setAddSysUserModal(false)}
//       />

//       <AddGroup
//         isOpen={addGroupModal}
//         onClose={() => setAddGroupModal(false)}
//         onAddGroup={handleAddGroup} // Pass the add group handler
//       />
//       <AddOrg
//         isOpen={addOrgModal}
//         onClose={() => setAddOrgModal(false)}
//       />
//     </div>
//   );
// };

// export default MemberSettings;

"use client";

import React, { useState, useEffect } from "react";
import { useLanguage } from "@/app/contexts/Language";
import Tabs from "../Tabs";
import { useRouter, useSearchParams } from "next/navigation";
import { PlusIcon } from "@heroicons/react/24/outline";
import UsersTable from "./MemberTables/UsersTable";
import GroupsTable from "./MemberTables/GroupsTable";
import OrganizationsTable from "./MemberTables/OrganizationsTable";
import AddSysUser from "./MemberModals/AddSysUser";
import AddGroup from "./MemberModals/AddGroup";
import AddOrg from "./MemberModals/AddOrg";
import { SystemGroupsTable } from "@/app/(home)/types/types";
import { systemOrgs } from "../../dummy_data/data";
import { createGroup, Group } from "@/app/lib/group_services.client";

interface ProjectSettingsProps {
  className?: string;
}

const MemberSettings = ({ className }: ProjectSettingsProps) => {
  const { t } = useLanguage();
  const [addSysUserModal, setAddSysUserModal] = useState(false);
  const [addGroupModal, setAddGroupModal] = useState(false);
  const [addOrgModal, setAddOrgModal] = useState(false);
  const [activeTab, setActiveTab] = useState("Users");
  const searchParams = useSearchParams();

  // Explicitly define the type of groups
  const [groups, setGroups] = useState<Group[]>([]);

  const tabData = [
    {
      label: "Users",
      content: <UsersTable />,
    },
    {
      label: "Groups",
      content: <GroupsTable data={groups} />,
    },
    {
      label: "Organizations",
      content: <OrganizationsTable data={systemOrgs} />,
    },
  ];

  // Handle tab changes
  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  // Handle button click to open modals
  const handleAddButtonClick = (event: React.MouseEvent<HTMLElement>) => {
    event.preventDefault();
    if (activeTab === "Users") {
      setAddSysUserModal(true);
    } else if (activeTab === "Groups") {
      setAddGroupModal(true);
    } else if (activeTab === "Organizations") {
      setAddOrgModal(true);
    }
  };

  // Function to handle adding a new group via API
//   const handleAddGroup = async (groupName: string, description?: string) => {
//     try {
//       const newGroup = await createGroup({ name: groupName, description });
//       setGroups((prevGroups) => [...prevGroups, newGroup]); // Update state with the new group
//     } catch (error) {
//       console.error('Error adding group:', error);
//     }
//   };

  const handleAddGroup = async (groupName: string, description?: string | null) => {
  try {
    const newGroup = await createGroup({ name: groupName, description });
    setGroups((prevGroups) => [...prevGroups, newGroup]);
  } catch (error) {
    console.error('Error adding group:', error);
  }
};

  // Effect to set the active tab from the query parameter
  useEffect(() => {
    const tab = searchParams.get("tab") || "Users";
    setActiveTab(tab);
  }, [searchParams]);

  return (
    <div className="bg-base-100 text-accent-content rounded-xl shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
          <h2 className="card-title">{t.translations.MEMBER_MANAGEMENT}</h2>
          <div className="flex space-x-4">
            <button
              onClick={handleAddButtonClick}
              className="btn btn-secondary text-white"
            >
              <PlusIcon className="size-6" />
              {activeTab === "Users"
                ? t.translations.USER
                : activeTab === "Groups"
                ? t.translations.GROUP
                : activeTab === "Organizations"
                ? t.translations.ORGANIZATION
                : t.translations.USER}
            </button>
          </div>
        </div>
        <Tabs
          tabs={tabData}
          className="tabs tabs-border"
          onTabChange={handleTabChange}
          activeTab={activeTab}
        />
      </div>

      <AddSysUser
        isOpen={addSysUserModal}
        onClose={() => setAddSysUserModal(false)}
      />

      <AddGroup
        isOpen={addGroupModal}
        onClose={() => setAddGroupModal(false)}
        onAddGroup={handleAddGroup}
      />
      <AddOrg
        isOpen={addOrgModal}
        onClose={() => setAddOrgModal(false)}
      />
    </div>
  );
};

export default MemberSettings;