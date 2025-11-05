"use client";

import React from "react";
import Tabs from "../components/Tabs";
import UsersTable from "./users/UsersTable";
import Groups from "./groups/Groups";
import Organizations from "./organizations/Organizations";

const MemberManagementClient = () => {
  const tabs = [
    {
      label: "Users",
      content: <UsersTable />,
    },
    {
      label: "Groups",
      content: <Groups />,
    },
    {
      label: "Organizations",
      content: <Organizations />,
    },
  ];

  return (
    <div>
      <div className="bg-base-200/40 pl-12 p-4">
        <h1 className="text-2xl font-bold text-base-content">
          Member Management
        </h1>
      </div>
      <div className="p-6">
        <Tabs tabs={tabs} activeTab={""} />
      </div>
    </div>
  );
};

export default MemberManagementClient;
