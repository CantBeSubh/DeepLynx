// app/(home)/ProjectsClient.tsx  — Client Component
"use client";

import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import WidgetCard, { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { PlusIcon } from "@heroicons/react/24/outline";
import Link from "next/link"; // prefer Link over useRouter().push
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useLanguage } from "../contexts/Language";
import { getAllProjects } from "../lib/projects_services.client"; // optional (for refresh)
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
        <Link href={`/project/${row.id}`} className="font-bold underline">
          {row.name}
        </Link>
      ),
    },
    {
      header: t.translations.DESCRIPTION,
      data: (row: ProjectsList) => row.description,
    },
    {
      header: t.translations.LAST_VIEWED,
      data: (row: ProjectsList) => row.lastViewed,
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
    <div className="bg-base-100">
      {/* Header */}
      <div className="flex justify-between items-center bg-base-200/40 pl-12 pt-3 pb-2">
        <h1 className="text-2xl font-bold text-info-content">
          {t.translations.WELECOME}
        </h1>
        <SearchInput
          placeholder="Search Projects"
          onChange={(e) => setSearchTerm(e.target.value)}
          className="mr-6"
        />
      </div>

      {/* Main Content */}
      <div className="mr-6 py-6">
        <div className="flex">
          <div
            className={`w-full md:w-3/5 px-4 ${
              canCustomize ? "grayed-out" : ""
            }`}
          >
            <div className="card card-border p-4">
              <div className="flex justify-between items-center mb-4">
                <h3 className="text-info-content text-lg font-semibold">
                  {t.translations.YOUR_PROJECTS}
                </h3>

                <div className="flex gap-2">
                  <button className="btn btn-outline btn-secondary flex items-center gap-1">
                    <PlusIcon className="size-6" />
                    <span>{t.translations.RECORD}</span>
                  </button>
                  <button
                    onClick={() => setIsModalOpen(true)}
                    className="btn btn-secondary text-primary-content flex items-center gap-1"
                  >
                    <PlusIcon className="size-6" />
                    <span>{t.translations.PROJECT}</span>
                  </button>
                </div>
              </div>

              <ExpandableTable
                data={filteredProjects}
                columns={columns}
                // If your table needs an "Explore" button, use Link inside column renderers
                renderExpandedContent={(project, onClose) => (
                  <ExpandedProjectCard project={project} onClose={onClose} />
                )}
                onExplore={onExplore}
              />
            </div>
          </div>

          <div className="w-full md:w-2/5 px-4">
            <WidgetCard
              widgets={homeWidgets}
              onSave={handleSave}
              onCustomizeChange={setCanCustomize}
            />
          </div>
        </div>
      </div>

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
