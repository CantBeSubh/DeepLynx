"use client";
import React, { useState, ReactNode } from "react";
import { Reorder } from "framer-motion";
import DataOverviewWidget from "./WidgetCards.tsx/DataOverviewWidget";
import LinksWidget from "./WidgetCards.tsx/LinksWidget";
import GraphWidget from "./WidgetCards.tsx/GraphWidget";
import RecentActivityWidget from "./WidgetCards.tsx/RecentActivity";
import ProjectOverviewWidget from "./WidgetCards.tsx/ProjectOverview";
import TeamMembersWidget from "./WidgetCards.tsx/TeamMembers";

type WidgetType = "DataOverview" | "Links" | "Graph" | "RecentActivity" | "ProjectOverview" | "TeamMembers";

// adjust logic for adding and removing widgets
//MAKE WIDGET FOR PAGE AND PROJECT NAME
const WidgetCard = () => {
  const [widgets, setWidgets] = useState<WidgetType[]>([
    "DataOverview",
    "Graph",
    "Links",
    "RecentActivity",
    "ProjectOverview",
    "TeamMembers"
  ]);

  // const WidgetCard = () => {
  // const [widgets, setWidgets] = useState<WidgetType[]>([
  //   "DataOverview",
  //   "Graph",
  //   "Links",
  //   "RecentActivity",
  //   "ProjectOverview",
  //   "TeamMembers"
  // ]);

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
    }
  };

  // Styling for Links Widget
  return (
    <Reorder.Group
      axis="y"
      onReorder={setWidgets}
      values={widgets}
      className="space-y-4"
    >
      {widgets.map((widget) => (
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
