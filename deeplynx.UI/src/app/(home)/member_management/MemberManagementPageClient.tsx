"use client";

import React from "react";
import { peopleData } from "../dummy_data/data";
import AvatarCell from "../components/Avatar";
import { MoonIcon, SunIcon } from "@heroicons/react/24/outline";
import { useLanguage } from "@/app/contexts/Language";
import ThemeToggle from "../components/ThemeToggle";

const MemberManagementClient = () => {
  const { lang, setLang, t } = useLanguage(); // <-- t comes from translations[lang]
  const jason = peopleData.find((p) => p.name === "Jason");

  return (
    <div className="p-20">
      <div className="flex items-center">
      </div>
    </div>
  );
};

export default MemberManagementClient;