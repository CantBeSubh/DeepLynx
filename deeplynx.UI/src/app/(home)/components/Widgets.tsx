"use client";

import React, { useState, useEffect } from "react";
import { Reorder } from "framer-motion";
import { translations } from "@/app/lib/translations";
import DataOverviewWidget from "./WidgetCards/DataOverviewWidget";
import LinksWidget from "./WidgetCards/LinksWidget";
import GraphWidget from "./WidgetCards/GraphWidget";
import RecentActivityWidget from "./WidgetCards/RecentActivity";
import ProjectOverviewWidget from "./WidgetCards/ProjectOverview";
import TeamMembersWidget from "./WidgetCards/TeamMembers";
import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import {
  Cog6ToothIcon,
  PlusIcon,
  XMarkIcon,
  DocumentCheckIcon,
} from "@heroicons/react/24/outline";

export type WidgetType =
  | "DataOverview"
  | "Links"
  | "Graph"
  | "RecentActivity"
  | "ProjectOverview"
  | "TeamMembers";

interface WidgetCardProps {
  widgets: WidgetType[];
  onSave: (widgets: WidgetType[]) => void;
  onCustomizeChange: (canCustomize: boolean) => void;
}

const WidgetCard: React.FC<WidgetCardProps> = ({
  widgets,
  onSave,
  onCustomizeChange,
}) => {
  const [currentWidgets, setCurrentWidgets] = useState<WidgetType[]>(widgets);
  const [initialWidgets, setInitialWidgets] = useState<WidgetType[]>(widgets);
  const [canCustomize, setCanCustomize] = useState<boolean>(false);
  const [widgetModal, setWidgetModal] = useState<boolean>(false);
  const locale = "en";
  const t = translations[locale];

  useEffect(() => {
    if (canCustomize) {
      setInitialWidgets(currentWidgets);
    }
    onCustomizeChange(canCustomize);
  }, [canCustomize, currentWidgets, onCustomizeChange]);

  const renderWidgets = (widget: WidgetType) => {
    switch (widget) {
      // case "DataOverview":
      //   return <DataOverviewWidget />;

      case "Links":
        return <LinksWidget />;

      case "Graph":
        return <GraphWidget />;

      // case "RecentActivity":
      //   return <RecentActivityWidget />;

      case "ProjectOverview":
        return <ProjectOverviewWidget />;

      case "TeamMembers":
        return <TeamMembersWidget />;

      default:
        return null;
    }
  };

  const handleReorder = (newOrder: WidgetType[]) => {
    if (canCustomize) {
      setCurrentWidgets(newOrder);
    }
  };

  const handleCancel = () => {
    setCurrentWidgets(initialWidgets);
    setCanCustomize(false);
  };

  const handleSave = () => {
    onSave(currentWidgets);
    setCanCustomize(false);
  };

  return (
    <div>
      <div className="flex justify-between items-center justify-end mb-4">
        {!canCustomize && (
          <button
            onClick={() => setCanCustomize(true)}
            className="btn btn-outline btn-secondary flex items-center mr-2"
          >
            <Cog6ToothIcon className="h-6 w-6" />
            {t.translations.CUSTOMIZE}
          </button>
        )}
        {canCustomize && (
          <>
            <button
              onClick={handleCancel}
              className="btn flex items-center mr-2 btn-outline btn-secondary"
            >
              <XMarkIcon className="size-6" />
              {t.translations.CANCEL}
            </button>
            <button
              onClick={handleSave}
              className="btn flex items-center mr-2 btn-secondary"
            >
              <DocumentCheckIcon className="h-6 w-6" />
              {t.translations.SAVE}
            </button>
          </>
        )}
        <button
          onClick={() => setWidgetModal(true)}
          className="btn btn-secondary text-primary-content flex items-center"
        >
          <PlusIcon className="h-6 w-6" />
          {t.translations.WIDGET}
        </button>
      </div>

      <Reorder.Group
        axis="y"
        onReorder={handleReorder}
        values={currentWidgets}
        className="space-y-4"
      >
        {currentWidgets.map((widget) => (
          <Reorder.Item
            key={widget}
            value={widget}
            className={`card card-border bg-base-100 w-auto ${
              canCustomize ? "cursor-grab" : "cursor-default"
            }`}
            style={{ pointerEvents: canCustomize ? "auto" : "none" }}
          >
            {renderWidgets(widget)}
          </Reorder.Item>
        ))}
      </Reorder.Group>

      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />
    </div>
  );
};

export default WidgetCard;
