import { useLanguage } from "@/app/contexts/Language";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { getProjectStats } from "@/app/lib/projects_services.client";
import {
  ArrowsRightLeftIcon,
  CircleStackIcon,
  LinkIcon,
  RectangleGroupIcon,
} from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";

const ProjectOverviewWidget = () => {
  const { t } = useLanguage();
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
      <h2 className="card-title">{t.translations.PROJECT_OVERVIEW}</h2>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">
            {t.translations.LINKED_SOURCES}
          </div>
          <div className="stat-value text-secondary flex items-center">
            <LinkIcon className="size-7 mr-2" />
            {stats?.linkedSources ? stats.linkedSources : 0}
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">
            {t.translations.DATA_RECORD}
          </div>
          <div className="stat-value text-secondary flex items-center">
            <CircleStackIcon className="size-8 mr-2" />
            {stats?.records ? stats.records : 0}
          </div>
        </div>
      </div>
      <div className="stats shadow">
        <div className="stat">
          <div className="stat-title text-secondary">
            {t.translations.CLASSES}
          </div>
          <div className="stat-value text-secondary flex items-center">
            <RectangleGroupIcon className="size-8 mr-2" />
            {stats?.classes ? stats.classes : 0}
          </div>
        </div>
        <div className="stat">
          <div className="stat-title text-secondary">
            {t.translations.CONNECTIONS}
          </div>
          <div className="stat-value text-secondary flex items-center">
            <ArrowsRightLeftIcon className="size-8 mr-2" />
            {stats?.connections ? stats.connections : 0}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProjectOverviewWidget;
