import React, { useEffect, useState } from "react";
import {
  UserSessionProvider,
  useUserSession,
} from "@/app/contexts/UserSessionProvider";
import {
  Squares2X2Icon,
  RectangleGroupIcon,
  ArrowsRightLeftIcon,
  CircleStackIcon,
} from "@heroicons/react/24/outline";
import { getDataOverview } from "@/app/lib/user_services";

const DataOverviewWidget = () => {
  const { user } = useUserSession();
  const [stats, setStats] = useState<{
    projects: number;
    records: number;
    tags: number;
    connections: number;
  } | null>(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const data = await getDataOverview("1"); // Hard coding user till be get login finlaized.
        setStats({
          projects: data.projects,
          records: data.records,
          tags: data.tags,
          connections: data.connections,
        });
      } catch (error) {
        console.error("Failed to fetch project stats for user:", error);
      }
    };
    fetchStats();
  }, []);

  return (
    <div className="card-body">
      <h2 className="card-title">Data Overview</h2>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">Projects</div>
          <div className="stat-value text-secondary flex items-center">
            <Squares2X2Icon className="size-8 mr-2" />
            {stats?.projects}
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">Data Records</div>
          <div className="stat-value text-secondary flex items-center">
            <CircleStackIcon className="size-8 mr-2" />
            {stats?.records}
          </div>
        </div>
      </div>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">Classes</div>
          <div className="stat-value text-secondary flex items-center">
            <RectangleGroupIcon className="size-8 mr-2" />
            {stats?.tags}
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">Connections</div>
          <div className="stat-value text-secondary flex items-center">
            <ArrowsRightLeftIcon className="size-8 mr-2" />
            {stats?.connections}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DataOverviewWidget;
