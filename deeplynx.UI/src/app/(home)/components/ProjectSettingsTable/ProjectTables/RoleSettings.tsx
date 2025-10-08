// import React, { useState, useEffect } from 'react';
// import Tabs from "../../Tabs";
// import { useLanguage } from "@/app/contexts/Language";
// import RoleName from './RoleName';
// import RolePermissions from './RolePermissions';
// import { useRouter, useSearchParams } from "next/navigation";

// interface RoleSettingsProps {
//   id?: string | string[];
// }

// const RoleSettings = ({ id }: RoleSettingsProps) => {
//   const { t } = useLanguage();
//   const [activeTab, setActiveTab] = useState("Settings");
//   const router = useRouter();
//   const searchParams = useSearchParams();

//   const handleTabChange = (label: string) => {
//     setActiveTab(label);
//   };

//   const toPermissionsTab = () => {
//     setActiveTab("Permissions");
//   };

//   const onCancel = () => {
//     router.push(`/project_settings?tab=Roles`);
//   };

//   const onSave = () => {
//     // TODO Add logic to save role changes
//     router.push(`/project_settings?tab=Roles`);
//   };

//   // Effect to read roleId from query and perform any necessary logic
//   useEffect(() => {
//     const roleId = searchParams.get('roleId');
//     if (roleId) {
//       // Fetch the role data using the roleId if needed
//     }
//   }, [searchParams]);

//   const tabData = [
//     {
//       label: "Settings",
//       content: (
//         <RoleName
//           toPermissionsTab={toPermissionsTab}
//           onCancel={onCancel}
//         />
//       ),
//     },
//     {
//       label: "Permissions",
//       content: (
//         <RolePermissions
//           onCancel={onCancel}
//           onSave={onSave}
//         />
//       ),
//     },
//   ];

//   return (
//     <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
//       <div className="card-body">
//         <div className="flex justify-between items-start">
//           <h2 className="card-title">{t.translations.ROLE_SETTINGS}</h2>
//           {/* TODO Differentiate the name of the role instead of just role settings */}
//         </div>
//         <div className="w-full">
//           {/* <Tabs
//             tabs={tabData}
//             className="tabs tabs-border"
//             onTabChange={handleTabChange}
//             activeTab={activeTab}
//           /> */}

//           <RoleName
//             toPermissionsTab={toPermissionsTab}
//             onCancel={onCancel}
//           />

//           <RolePermissions
//             onCancel={onCancel}
//             onSave={onSave}
//           />

//         </div>
//       </div>
//     </div>
//   );
// };

// export default RoleSettings;

// import React, { useState, useEffect } from 'react';
// import { useLanguage } from "@/app/contexts/Language";
// import RoleDetails from './RoleDetails';
// import { useRouter, useSearchParams } from "next/navigation";

// interface RoleSettingsProps {
//   id?: string | string[];
// }

// const RoleSettings = ({ id }: RoleSettingsProps) => {
//   const { t } = useLanguage();
//   const router = useRouter();
//   const searchParams = useSearchParams();

//   const onCancel = () => {
//     router.push(`/project_settings?tab=Roles`);
//   };

//   const onSave = () => {
//     // TODO Add logic to save role changes
//     router.push(`/project_settings?tab=Roles`);
//   };

//   // Effect to read roleId from query and perform any necessary logic
//   useEffect(() => {
//     const roleId = searchParams.get('roleId');
//     if (roleId) {
//       // Fetch the role data using the roleId if needed
//     }
//   }, [searchParams]);

//   return (
//     <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
//       <div className="card-body">
//         <div className="w-full">
//           <RoleDetails
//             onCancel={onCancel}
//             onSave={onSave}
//           />
//         </div>
//       </div>
//     </div>
//   );
// };

// export default RoleSettings;

import React, { useState, useEffect } from 'react';
import { useLanguage } from "@/app/contexts/Language";
import RoleDetails from './RoleDetails';
import { useRouter, useSearchParams } from "next/navigation";
import { getAllRoles, updateRole } from "@/app/lib/role_services.client";
import { RoleResponseDto, PermissionResponseDto } from "../../../types/types";

interface RoleSettingsProps {
  id?: string | string[];
}

const RoleSettings = ({ id }: RoleSettingsProps) => {
  const { t } = useLanguage();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [role, setRole] = useState<RoleResponseDto | null>(null);
  const [permissions, setPermissions] = useState<PermissionResponseDto[]>([]);

  const onCancel = () => {
    router.push(`/project/${id}/project_settings?tab=Roles`);
  };

  const onSave = async () => {
  if (role) {
    try {
      await updateRole(role.id, {
        name: role.name,
        description: role.description,
      });
      const updatedRoles = await getAllRoles();
      setRole(updatedRoles.find((updatedRole: { id: number; }) => updatedRole.id === role.id) || null);
      router.push(`/project/${id}/project_settings?tab=Roles`);
    } catch (error) {
      console.error("Error updating role:", error);
    }
  }
};

  // Fetch role and permissions data on component mount
  useEffect(() => {
    const roleId = searchParams.get('roleId');
    if (roleId) {
      // Fetch the role data using the roleId
      // Update state with fetched data
      // Example API call to get role data
      getAllRoles(/* pass necessary params */)
        .then((data) => {
          setRole(data.find((r: RoleResponseDto) => r.id === Number(roleId)) || null);
          // Assuming another API call to get permissions based on roleId
          // Example: getPermissions(roleId).then(setPermissions);
        })
        .catch((error) => {
          console.error("Error fetching role data:", error);
        });
    }
  }, [searchParams]);

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="w-full">
          {role && (
            <RoleDetails
              role={role}
              setRole={setRole}
              permissions={permissions}
              setPermissions={setPermissions}
              onCancel={onCancel}
              onSave={onSave}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default RoleSettings;