// app/(home)/project/[id]/ProjectDetailClient.tsx
"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import SavedSearches from "@/app/(home)/components/SavedSearches";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { useLanguage } from "@/app/contexts/Language";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { format } from "date-fns";
import RecentRecordsCard from "../../components/RecentRecordsCard";

type Props = {
  initialProject: ProjectsList | null;
  projectId: string;
};

export default function ProjectDetailClient({
  initialProject,
  projectId,
}: Props) {
  const { t } = useLanguage();
  const router = useRouter();

  const [project, setProject] = useState<ProjectsList | null>(initialProject);
  const [canCustomize, setCanCustomize] = useState(false);

  const [projectWidgets, setProjectWidgets] = useState<WidgetType[]>([
    "RecentActivity",
    "ProjectOverview",
    "TeamMembers",
  ]);

  // Sync project session from initial server data
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();
  useEffect(() => {
    if (!hasLoaded || !project) return;
    setProjectSession({ projectId: project.id!, projectName: project.name });
  }, [hasLoaded, project, setProjectSession]);

  const handleSave = (newWidgets: WidgetType[]) => {
    setProjectWidgets(newWidgets);
    localStorage.setItem(
      `projectWidgets-${projectId}`,
      JSON.stringify(newWidgets)
    );
  };

  if (!hasLoaded) return <p className="p-4">{t.translations.LOADING}</p>;
  if (!project) return <p className="p-4">{t.translations.NO_PROJECT_FOUND}</p>;

  return (
    <div className="min-h-screen bg-base-100">
      {/* Project Header */}
      <div className="bg-base-200/50 border-b border-base-300/30 py-4 px-6 lg:px-12">
        <h1 className="text-2xl font-bold text-base-content">{project.name}</h1>
        <p className="mt-2 text-base-content/70">{project.description}</p>
        <p className="mt-2 text-sm text-base-content/60">
          <span className="font-semibold">{t.translations.CREATED}: </span>
          {project.createdAt &&
            format(new Date(project.createdAt), "MM/dd/yyyy")}
        </p>
      </div>

      {/* Main Content */}
      <div className="flex flex-col lg:flex-row gap-6 px-4 lg:px-6 mt-6">
        {/* Left Column */}
        <div
          className={`flex-1 lg:w-3/5 transition-opacity duration-300 ${
            canCustomize ? "opacity-50 pointer-events-none" : ""
          }`}
        >
          {/* Search Bar */}
          <div className="mb-6">
            <LargeSearchBar
              className="w-full"
              onEnter={(searchTerm) => {
                const query = new URLSearchParams({
                  fromProject: projectId,
                  search: searchTerm,
                }).toString();
                router.push(`/data_catalog?${query}`);
              }}
            />
          </div>

          {/* Data Catalog Card */}
          <div className="card bg-base-200/30 border border-base-300/50 shadow-sm mb-6">
            <div className="card-body">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold text-base-content">
                  {t.translations.DATA_CATALOG_OVERVIEW}
                </h2>
                <Link
                  className="btn btn-secondary btn-sm"
                  href={{
                    pathname: "/data_catalog",
                    query: { fromProject: projectId },
                  }}
                >
                  Visit
                </Link>
              </div>

              <RecentRecordsCard selectedProjects={[projectId]} />
            </div>
          </div>

          {/* Saved Searches */}
          <SavedSearches />
        </div>

        {/* Right Column */}
        <aside className="lg:w-2/5">
          <div className="sticky top-6">
            <WidgetCard
              widgets={projectWidgets}
              onSave={handleSave}
              onCustomizeChange={setCanCustomize}
            />
          </div>
        </aside>
      </div>
    </div>
  );
}
