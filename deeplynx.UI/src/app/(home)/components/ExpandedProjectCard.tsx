"use client";
import React, { useEffect, useState } from "react";
import { ProjectsList } from "@/app/(home)/types/types";
import { useRouter } from "next/navigation";
import { getProjectStats } from "@/app/lib/projects_services";
import { peopleData } from "../dummy_data/data";
import Image from "next/image";
import {
  XMarkIcon,
  RectangleGroupIcon,
  CircleStackIcon,
  ArrowsRightLeftIcon,
} from "@heroicons/react/24/outline";
import { translations } from "@/app/lib/translations";

interface Props {
  project: ProjectsList;
  onClose: () => void;
}

const ExpandedProjectCard: React.FC<Props> = ({ project, onClose }) => {
  const router = useRouter();
  const locale = "en"; //We could use cookies, context, or router.locale to change language in the future
  const t = translations[locale];

  const [stats, setStats] = useState<{
    classes: number;
    records: number;
    connections: number;
  } | null>(null);

  useEffect(() => {
    if (!project.id) return;

    const fetchStats = async () => {
      try {
        const data = await getProjectStats(project.id!);
        setStats({
          classes: data.classes,
          records: data.records,
          connections: data.datasources,
        });
      } catch (error) {
        console.error("Failed to fetch project stats:", error);
      }
    };
    fetchStats();
  }, [project.id]);

  return (
    <div>
      <div className="flex justify-between items-start">
        <div>
          <h2 className="text-2xl font-bold">{project.name}</h2>
          <p className="text-sm text-base-content">{project.description}</p>
          <p className="text-sm text-base-300 mt-1 mb-2">
            {t.ExpandableTable.LAST_EDIT} {project.lastViewed}
          </p>
        </div>
        <button onClick={onClose} aria-label="Close details">
          <XMarkIcon className="size-6" />
        </button>
      </div>

      <div className="space-x-2">
        <p className="text-base-300 mb-2">{t.ExpandableTable.TEAM_MEMBERS}</p>
        {peopleData
          .slice(0, Math.floor(Math.random() * 6) + 2)
          .map((person) => (
            <div key={person.id} className="avatar inline-block">
              <div className="w-10 h-10 relative overflow-hidden rounded-full">
                <Image
                  src={person.image}
                  alt={person.name}
                  fill
                  className="object-cover"
                />
              </div>
            </div>
          ))}
      </div>

      {/* Stats */}
      {!stats ? (
        <div className="text-center text-base-content">
          {t.ExpandableTable.NO_STATS}
        </div>
      ) : (
        <div className="grid grid-cols-3 gap-4 mt-4">
          {[
            {
              title: "Classes",
              value: stats?.classes,
              Icon: RectangleGroupIcon,
            },
            {
              title: "Records",
              // title: "Data Records",
              value: stats?.records,
              Icon: CircleStackIcon,
            },
            {
              title: "Data Sources",
              // title: "Connections",
              value: stats?.connections,
              Icon: ArrowsRightLeftIcon,
            },
          ].map(({ title, value, Icon: Icon }, idx) => (
            <div key={idx} className="stat flex items-center">
              <div>
                <Icon className="size-8 text-secondary" />
              </div>
              <div>
                <div className="stat-title text-secondary">{title}</div>
                <div className="stat-value text-secondary">{value}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="flex justify-end">
        <button
          className="btn btn-secondary text-primary-content mt-4"
          onClick={() => router.push(`/project/${project.id}`)}
        >
          {t.ExpandableTable.EXPLORE}
        </button>
      </div>
    </div>
  );
};

export default ExpandedProjectCard;
