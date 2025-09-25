"use client";

import React, { useState } from 'react';
import Tabs from "../../Tabs";
import { useLanguage } from "@/app/contexts/Language";

interface RoleSettingsProps {
  className?: string;
  isOpen: boolean;
  onClose: () => void;
}

const RoleSettings = ({ className }: RoleSettingsProps) => {
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState("Settings");

  const handleTabChange = (label: string) => {
    setActiveTab(label);
  };

  const tabData = [
    {
      label: "Settings",
      content: (""),
    },
    {
      label: "Permissions",
      content: (""),
    },
  ];

  return (
    <div className="bg-base-100 text-accent-content rounded-xl p-0 shadow-md card">
        <div className="card-body">
            <div className="flex justify-between items-start">
                <h2 className="card-title">{t.translations.ROLE_SETTINGS}</h2>
            </div>
            <div className="w-full">
            <Tabs
                tabs={tabData}
                className="tabs tabs-border"
                onTabChange={handleTabChange}
            />
                <h2 className="font-bold text-lg mb-4 mt-3 text-neutral">
                {t.translations.ADD_NEW_ROLE}
                </h2>
                {/* Form for adding a new member and selecting their role*/}
                <form method="dialog" className="flex flex-col gap-4 w-full">
                    <input
                    type="text"
                    placeholder="Role name"
                    className="input input-primary w-full"
                    />
                    <input
                    type="text"
                    placeholder="Description"
                    className="input input-primary w-full"
                    />
                </form>
                {/* Modal Action Buttons */}
                <div className="modal-action">
                    {/* <button className="btn" onClick={onClose}> */}
                    <button className="btn">
                        {t.translations.CANCEL}
                    </button>
                    {/* <button className="btn btn-primary" onClick={onClose}> */}
                    <button className="btn btn-primary">
                        {t.translations.NEXT}
                    </button>
                </div>
            </div>
        </div>
    </div>
  );
};

export default RoleSettings;