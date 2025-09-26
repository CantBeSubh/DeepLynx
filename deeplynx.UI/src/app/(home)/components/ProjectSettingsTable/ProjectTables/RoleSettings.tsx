import React, { useState, useEffect } from 'react';
import Tabs from "../../Tabs";
import { useLanguage } from "@/app/contexts/Language";
import SettingsTab from './RoleName';
import PermissionsTab from './RolePermissions';
import { useRouter, useSearchParams } from "next/navigation";

interface RoleSettingsProps {
  className?: string;
}

const RoleSettings = ({ className }: RoleSettingsProps) => {
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState("Settings");
  const router = useRouter();
  const searchParams = useSearchParams();

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

   const toPermissionsTab = () => {
    setActiveTab("Permissions");
  };

  const onCancel = () => {
    router.push("/project_settings?tab=Roles");
  };

  const onSave = () => {
    // TODO Add logic to save role changes
    router.push("/project_settings?tab=Roles");
  };

   // Effect to read roleId from query and perform any necessary logic
  useEffect(() => {
    const roleId = searchParams.get('roleId');
    if (roleId) {
      console.log(`Editing role with ID: ${roleId}`);
      // Fetch the role data using the roleId if needed
    }
  }, [searchParams]);

  const tabData = [
    { label: "Settings", content: <SettingsTab toPermissionsTab={toPermissionsTab} onCancel={onCancel}/> },
    { label: "Permissions", content: <PermissionsTab onCancel={onCancel} onSave={onSave}/> },
  ];

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
      <div className="card-body">
        <div className="flex justify-between items-start">
          <h2 className="card-title">{t.translations.ROLE_SETTINGS}</h2>
          {/* TODO Differentiate the name of the role instead of just role settings */}
        </div>
        <div className="w-full">
          <Tabs
            tabs={tabData}
            className="tabs tabs-border"
            onTabChange={handleTabChange}
            activeTab={activeTab}
          />
        </div>
      </div>
    </div>
  );
};

export default RoleSettings;