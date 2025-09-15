// app/(home)/ProjectsClient.tsx  — Client Component
"use client";

import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { PlusIcon } from "@heroicons/react/24/outline";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useLanguage } from "../contexts/Language";
import { getAllProjects } from "../lib/projects_services.client";
import CreateProject from "./components/CreateProjectsModal";
import SearchInput from "./components/SearchInput";

type Props = { initialProjects: ProjectsList[] };

export default function HomeDashboard({ initialProjects }: Props) {
  const { t } = useLanguage();
  const router = useRouter();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [widgetModal, setWidgetModal] = useState(false);
  const [projects, setProjects] = useState<ProjectsList[]>(initialProjects);
  const [searchTerm, setSearchTerm] = useState("");
  const [canCustomize, setCanCustomize] = useState(false);
  const [homeWidgets, setHomeWidgets] = useState<WidgetType[]>([
    "Links",
    "DataOverview",
    "Graph",
  ]);

  const filteredProjects = projects.filter((project) => {
    const term = searchTerm.toLowerCase();
    return (
      project.name.toLowerCase().includes(term) ||
      project.description?.toLowerCase().includes(term)
    );
  });

  const refreshProjects = async () => {
    try {
      const data = await getAllProjects();
      setProjects(data);
    } catch (err) {
      console.error("Failed to refresh projects:", err);
    }
  };

  const onExplore = (row: ProjectsList) => {
    router.push(`/project/${row.id}`);
  };

  const columns = [
    {
      header: t.translations.PROJECT_NAME,
      data: (row: ProjectsList) => (
        <Link
          href={`/project/${row.id}`}
          className="font-bold text-primary hover:text-primary/80 underline underline-offset-2 transition-colors"
        >
          {row.name}
        </Link>
      ),
    },
    {
      header: t.translations.DESCRIPTION,
      data: (row: ProjectsList) => (
        <span className="text-base-content/80">{row.description || "—"}</span>
      ),
    },
    {
      header: t.translations.LAST_VIEWED,
      data: (row: ProjectsList) => (
        <span className="text-base-content/60 text-sm">{row.lastViewed}</span>
      ),
    },
  ];

  const handleSave = (newWidgets: WidgetType[]) => {
    setHomeWidgets(newWidgets);
    setCanCustomize(false);
  };

  const handleCancel = () => {
    setCanCustomize(false);
  };

  return (
    <div className="min-h-screen bg-base-100">
      {/* Header Section */}
      <header className="bg-base-200/50 border-b border-base-300/30 sticky top-0 z-10 backdrop-blur-sm">
        <div className="flex justify-between items-center px-4 sm:px-6 lg:px-12 py-4">
          <h1 className="text-2xl font-bold text-base-content">
            {t.translations.WELECOME}
          </h1>
          <SearchInput
            placeholder="Search Projects"
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </header>

      {/* Main Content Area */}
      <main className="px-4 sm:px-6 lg:px-8 py-6">
        <div className="flex flex-col lg:flex-row gap-6">
          {/* Projects Section */}
          <section
            className={`flex-1 lg:w-3/5 transition-opacity duration-300 ${
              canCustomize ? "opacity-50 pointer-events-none" : ""
            }`}
          >
            <div className="card bg-base-200/30 border border-base-300/50 shadow-sm">
              <div className="card-body">
                {/* Section Header */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
                  <h2 className="text-lg font-semibold text-base-content">
                    {t.translations.YOUR_PROJECTS}
                  </h2>

                  <div className="flex gap-2 w-full sm:w-auto">
                    <button className="btn btn-outline btn-secondary btn-sm flex-1 sm:flex-initial">
                      <PlusIcon className="size-5" />
                      <span>{t.translations.RECORD}</span>
                    </button>
                    <button
                      onClick={() => setIsModalOpen(true)}
                      className="btn btn-secondary btn-sm flex-1 sm:flex-initial"
                    >
                      <PlusIcon className="size-5" />
                      <span>{t.translations.PROJECT}</span>
                    </button>
                  </div>
                </div>

                {/* Table */}
                <div className="overflow-x-auto">
                  <ExpandableTable
                    data={filteredProjects}
                    columns={columns}
                    renderExpandedContent={(project, onClose) => (
                      <ExpandedProjectCard
                        project={project}
                        onClose={onClose}
                      />
                    )}
                    onExplore={onExplore}
                  />
                </div>

                {/* Empty State */}
                {filteredProjects.length === 0 && (
                  <div className="text-center py-12">
                    <p className="text-base-content/60">
                      {searchTerm
                        ? "No projects found matching your search"
                        : "No projects yet. Create your first project to get started!"}
                    </p>
                  </div>
                )}
              </div>
            </div>
          </section>

          {/* Widgets Section */}
          <aside className="lg:w-2/5">
            <div className="sticky top-24">
              <WidgetCard
                widgets={homeWidgets}
                onSave={handleSave}
                onCustomizeChange={setCanCustomize}
              />
            </div>
          </aside>
        </div>
      </main>

      {/* Modals */}
      <CreateProject
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onProjectCreated={refreshProjects}
      />
      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />
    </div>
  );
}
