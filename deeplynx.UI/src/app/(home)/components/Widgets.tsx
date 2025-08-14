"use client";

import React, { useState, ReactNode, useEffect } from "react";
import { Reorder } from "framer-motion";
import DataOverviewWidget from "./WidgetCards/DataOverviewWidget";
import LinksWidget from "./WidgetCards/LinksWidget";
import GraphWidget from "./WidgetCards/GraphWidget";
import RecentActivityWidget from "./WidgetCards/RecentActivity";
import ProjectOverviewWidget from "./WidgetCards/ProjectOverview";
import TeamMembersWidget from "./WidgetCards/TeamMembers";

export type WidgetType = "DataOverview" | "Links" | "Graph" | "RecentActivity" | "ProjectOverview" | "TeamMembers";
interface WidgetCardProps {
  widgets: WidgetType[];
  canCustomize: boolean;
  onSave: (widgets: WidgetType[]) => void;
  onCancel: () => void;
}

const WidgetCard: React.FC<WidgetCardProps> = ({ widgets, canCustomize, onSave, onCancel }) => {
  const [currentWidgets, setCurrentWidgets] = useState<WidgetType[]>(widgets);

  const renderWidgets = (widget: WidgetType) => {
    switch (widget) {
      case "DataOverview":
        return <DataOverviewWidget />;

      case "Links":
        return <LinksWidget />;

      case "Graph":
        return <GraphWidget />;

      case "RecentActivity":
        return <RecentActivityWidget />;

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

  return (
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
          className={`card card-border bg-base-100 w-auto ${canCustomize ? "cursor-grab" : "cursor-default"}`}
          style={{ pointerEvents: canCustomize ? 'auto' : 'none' }}
        >
          {renderWidgets(widget)}
        </Reorder.Item>
      ))}
    </Reorder.Group>
  );
};

export default WidgetCard;
