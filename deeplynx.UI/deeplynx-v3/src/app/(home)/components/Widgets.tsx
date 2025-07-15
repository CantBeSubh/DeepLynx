"use client";

import React, { useState, ReactNode } from "react";
import { Reorder } from "framer-motion";
import DataOverviewWidget from "./WidgetCards.tsx/DataOverviewWidget";
import LinksWidget from "./WidgetCards.tsx/LinksWidget";
import GraphWidget from "./WidgetCards.tsx/GraphWidget";
import RecentActivityWidget from "./WidgetCards.tsx/RecentActivity";
import ProjectOverviewWidget from "./WidgetCards.tsx/ProjectOverview";
import TeamMembersWidget from "./WidgetCards.tsx/TeamMember";

export type WidgetType = "DataOverview" | "Links" | "Graph" | "RecentActivity" | "ProjectOverview" | "TeamMembers";
interface WidgetCardProps {
  widgets: WidgetType[];
}

const WidgetCard: React.FC<WidgetCardProps> = ({ widgets }) => {
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

  return (
    <Reorder.Group
      axis="y"
      onReorder={setCurrentWidgets}
      values={currentWidgets}
      className="space-y-4"
    >
      {currentWidgets.map((widget) => (
        <Reorder.Item
          key={widget}
          value={widget}
          className="card card-border bg-base-100 w-auto cursor-grab"
        >
          {renderWidgets(widget)}
        </Reorder.Item>
      ))}
    </Reorder.Group>
  );
};

export default WidgetCard;
