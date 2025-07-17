import React from "react";
import {Squares2X2Icon, RectangleGroupIcon, ArrowsRightLeftIcon, CircleStackIcon} from "@heroicons/react/24/outline";

const DataOverviewWidget = () => {
  return (
    <div className="card-body">
      <h2 className="card-title">Data Overview</h2>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">Projects</div>
          <div className="stat-value text-secondary flex items-center">
            <Squares2X2Icon
              className="size-8 mr-2"/>
            89,000
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">Data Records</div>
          <div className="stat-value text-secondary flex items-center">
            <CircleStackIcon
              className="size-8 mr-2"/>
            89,400
          </div>
        </div>
      </div>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">Classes</div>
          <div className="stat-value text-secondary flex items-center">
            <ArrowsRightLeftIcon
              className="size-8 mr-2"/>
            89,400
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">Connections</div>
          <div className="stat-value text-secondary flex items-center">
            <RectangleGroupIcon
              className="size-8 mr-2"/>
            89,400
          </div>
        </div>
      </div>
    </div>
  );
};

export default DataOverviewWidget;
