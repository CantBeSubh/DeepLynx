import React, { useEffect, useState } from "react";
import {
  LinkIcon,
  RectangleGroupIcon,
  ArrowsRightLeftIcon,
  CircleStackIcon,
} from "@heroicons/react/24/outline";
import { getProjectStats } from "@/app/lib/projects_services";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";

const ProjectOverviewWidget = () => {
  const { project, hasLoaded } = useProjectSession();
  const [stats, setStats] = useState<{
    classes: number;
    records: number;
    connections: number;
    linkedSources?: number;
  } | null>(null);

  useEffect(() => {
    const fetchStats = async (projectId: string) => {
      try {
        const data = await getProjectStats(projectId);
        setStats({
          classes: data.classes,
          records: data.records,
          connections: data.datasources,
        });
      } catch (error) {
        console.error("Failed to fetch project stats:", error);
      }
    };
    if (project?.projectId) fetchStats(project.projectId);
  }, [project?.projectId]);

  return (
    <div className="card-body">
      <h2 className="card-title">Project Overview</h2>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">Linked Sources</div>
          <div className="stat-value text-secondary flex items-center">
            <LinkIcon className="size-7 mr-2" />
            {stats?.linkedSources}
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">Records</div>
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
            {stats?.classes}
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

export default ProjectOverviewWidget;
