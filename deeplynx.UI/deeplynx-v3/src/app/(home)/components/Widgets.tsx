"use client";
import React, { useState, ReactNode } from "react";
import { Reorder } from "framer-motion";
import DataOverviewWidget from "./DataOverviewWidget";
import LinksWidget from "./LinksWidget";
import GraphWidget from "./GraphWidget";

type WidgetType = "DataOverview" | "Links" | "Graph";

// adjust logic for adding and removing widgets
const WidgetCard = () => {
  const [widgets, setWidgets] = useState<WidgetType[]>([
    "DataOverview",
    "Graph",
    "Links",
  ]);

  const renderWidgets = (widget: WidgetType) => {
    switch (widget) {
      case "DataOverview":
        return <DataOverviewWidget />;

      case "Links":
        return <LinksWidget />;

      case "Graph":
        return <GraphWidget />;
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
