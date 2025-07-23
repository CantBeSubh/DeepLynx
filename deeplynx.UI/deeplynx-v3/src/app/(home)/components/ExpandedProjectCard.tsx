"use client";
import React, { useEffect, useState } from "react";
import { ProjectsList } from "@/app/(home)/types/types";
import { useRouter } from "next/navigation";
import { getProjectStats } from "@/app/lib/projects_services";
import { peopleData } from "../dummy_data/data";
import Image from "next/image";
import { XMarkIcon } from "@heroicons/react/24/outline";

interface Props {
  project: ProjectsList;
  onClose: () => void;
}

const ExpandedProjectCard: React.FC<Props> = ({ project, onClose }) => {
  const router = useRouter();

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
            Last Edited: {project.lastViewed}
          </p>
        </div>
        <button onClick={onClose} aria-label="Close details">
          <XMarkIcon className="size-6" />
        </button>
      </div>

      <div className="space-x-2">
        <p className="text-base-300 mb-2">Team Members:</p>
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
        <div className="text-center text-base-content">No stats found ...</div>
      ) : (
        <div className="grid grid-cols-3 gap-4 mt-4">
          {[
            {
              title: "Classes",
              value: stats?.classes,
              iconPath:
                "M4.098 19.902a3.75 3.75 0 0 0 5.304 0l6.401-6.402M6.75 21A3.75 3.75 0 0 1 3 17.25V4.125C3 3.504 3.504 3 4.125 3h5.25c.621 0 1.125.504 1.125 1.125v4.072M6.75 21a3.75 3.75 0 0 0 3.75-3.75V8.197M6.75 21h13.125c.621 0 1.125-.504 1.125-1.125v-5.25c0-.621-.504-1.125-1.125-1.125h-4.072M10.5 8.197l2.88-2.88c.438-.439 1.15-.439 1.59 0l3.712 3.713c.44.44.44 1.152 0 1.59l-2.879 2.88M6.75 17.25h.008v.008H6.75v-.008Z",
            },
            {
              title: "Records",
              // title: "Data Records",
              value: stats?.records,
              iconPath:
                "M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375m16.5 0v3.75m-16.5-3.75v3.75m16.5 0v3.75C20.25 16.153 16.556 18 12 18s-8.25-1.847-8.25-4.125v-3.75m16.5 0c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125",
            },
            {
              title: "Data Sources",
              // title: "Connections",
              value: stats?.connections,
              iconPath:
                "M7.5 21 3 16.5m0 0L7.5 12M3 16.5h13.5m0-13.5L21 7.5m0 0L16.5 12M21 7.5H7.5",
            },
          ].map(({ title, value, iconPath }, idx) => (
            <div key={idx} className="stat flex items-center">
              <div>
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth={1.5}
                  stroke="currentColor"
                  className="size-8 text-secondary"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d={iconPath}
                  />
                </svg>
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
          Explore
        </button>
      </div>
    </div>
  );
};

export default ExpandedProjectCard;
