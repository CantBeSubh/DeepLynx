import React from "react";
import { peopleData } from "../dummy_data/data";
import AvatarCell from "../components/Avatar";

const SettingsPageClient = () => {
  const jason = peopleData.find((person) => person.name === "Jason");
  return (
    <div className="p-10">
      <div className="flex items-center">
        <AvatarCell image={jason?.image} name={jason?.name} />
        <h1 className="text-2xl font-bold text-info-content ml-3">
          Jason Kuipers
        </h1>
      </div>
    </div>
  );
};

export default SettingsPageClient;
