"use client"

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
import ProjectDropdownSingleSelect from "../../components/ProjectDropdownSingleSelect";

type Props = {
  projects: ProjectsList[];
  initialProject: ProjectsList | null;
  projectId: string;
};

export default function ProjectDetailClient(
    {
      projects,
      initialProject,
      projectId,
    }: Props
) {
  const { t } = useLanguage();
  const router = useRouter();

  const [project, setProject] = useState<ProjectsList | null>(initialProject);
  const [selectedProjectId, setSelectedProjectId] = useState(initialProject ? initialProject.id : null);
  const [canCustomize, setCanCustomize] = useState(false);

  const [projectWidgets, setProjectWidgets] = useState<WidgetType[]>([
    "RecentActivity",
    "ProjectOverview",
    "TeamMembers",
  ]);

  // Sync project session from initial server data
  const { setProject: setProjectSession, hasLoaded } = useProjectSession();
    useEffect(() => {
        if (!selectedProjectId) return;
        const selectedProject = projects.find(proj => proj.id === selectedProjectId);
        if (selectedProject) {
            setProject(selectedProject);
            setProjectSession({
                projectId: selectedProject.id!,
                projectName: selectedProject.name,
            });
        }
    }, [selectedProjectId, projects, setProjectSession]);
  
  const handleSave = (newWidgets: WidgetType[]) => {
    setProjectWidgets(newWidgets);
    localStorage.setItem(
        `projectWidgets-${selectedProjectId ? selectedProjectId : ""}`,
        JSON.stringify(newWidgets)
    );
  };

  const handleSearchEnter = (searchTerm : string) => {
      const query = new URLSearchParams({
          fromProject: selectedProjectId ? selectedProjectId : "",
          search: searchTerm,
      }).toString();
      router.push(`/data_catalog?${query}`);
  };
  
  if (!hasLoaded) return <p className="p-4">{t.translations.LOADING}</p>;
  if (!project) return <p className="p-4">{t.translations.NO_PROJECT_FOUND}</p>;

  return (
      <div>
        <main>
          <div className="text-info-content bg-base-200/40 py-3 p-12">
            <h1 className="text-2xl">{project.name}</h1>
            <p className="mt-2 text-base-content">{project.description}</p>
            <p>
              <strong>{t.translations.CREATED} </strong>
              {project.createdAt &&
                  format(new Date(project.createdAt), "MM/dd/yyyy")}
            </p>
            <ProjectDropdownSingleSelect
                projects={projects}
                onSelectionChange={setSelectedProjectId}
                defaultSelectedId={initialProject?.id || ""}
            />
          </div>

          <div className="flex w-full mt-6">
            {/* left column */}
            <div
                className={`w-full md:w-3/5 pr-4 ${
                    canCustomize ? "grayed-out" : ""
                }`}
            >
              <div className="flex flex-col">
                <LargeSearchBar
                    className="mb-4 px-4"
                    onEnter={handleSearchEnter}
                />
              </div>

              <div className="card card-border m-2">
                <div className="card-body">
                  <div className="flex justify-between">
                    <h1 className="text-xl font-semibold">
                      {t.translations.DATA_CATALOG_OVERVIEW}
                    </h1>
                    <Link
                        className="btn btn-secondary"
                        href={{
                          pathname: "/data_catalog",
                          query: { fromProject: selectedProjectId ? selectedProjectId : "" },
                        }}
                    >
                      Visit
                    </Link>
                  </div>

                  <RecentRecordsCard selectedProjects={[selectedProjectId ? selectedProjectId : ""]} />
                </div>
              </div>

              <SavedSearches />
            </div>

            {/* right column */}
            <div className="w-full md:w-2/5 px-4">
              <WidgetCard
                  widgets={projectWidgets}
                  onSave={handleSave}
                  onCustomizeChange={setCanCustomize}
              />
            </div>
          </div>
        </main>
      </div>
  );
}
