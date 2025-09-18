// app/(home)/HomeDashboardClient.tsx  — Client Component
"use client";

import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { PlusIcon } from "@heroicons/react/24/outline";
import Link from "next/link"; // prefer Link over useRouter().push
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useLanguage } from "../contexts/Language";
import { getAllProjects } from "../lib/projects_services.client"; // optional (for refresh)
import CreateProject from "./components/CreateProjectsModal";
import SearchInput from "./components/SearchInput";

import { useSession } from "next-auth/react";
import AddRecordModal from "./components/AddRecordModal";

type Props = { initialProjects: ProjectsList[] };

export default function HomeDashboardClient({ initialProjects }: Props) {
  const { t } = useLanguage();
  const router = useRouter();

  const { data: session, status } = useSession();
  const [isProjectModalOpen, setIsProjectModalOpen] = useState(false);
  const [isRecordModalOpen, setIsRecordModalOpen] = useState(false);
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
      <div className="p-6">
        <div
          //This is commented out till we get digets
          // className={`w-full md:w-3/5 px-4 ${
          //   canCustomize ? "grayed-out" : ""
          // }`}
          className="w-4/5 mx-auto"
        >
          <div className="card card-border p-4">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-info-content text-lg font-semibold">
                {t.translations.YOUR_PROJECTS}
              </h3>

              <div className="flex gap-2">
                <button
                  onClick={() => setIsRecordModalOpen(true)}
                  className="btn btn-outline btn-secondary flex items-center gap-1"
                >
                  <PlusIcon className="size-6" />
                  <span>{t.translations.RECORD}</span>
                </button>
                <button
                  onClick={() => setIsProjectModalOpen(true)}
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

        {/* 
          This will be added later ...
        <div className="w-full md:w-2/5 px-4">
            <WidgetCard
              widgets={homeWidgets}
              onSave={handleSave}
              onCustomizeChange={setCanCustomize}
            />
            {session?.user && (
              <div className="bg-green-100 p-4 m-4 rounded">
                <p>Welcome back, {session.user.name}!</p>
              </div>
            )}
          </div> */}
      </div>

      {/* Modals */}
      <AddRecordModal
        isOpen={isRecordModalOpen}
        onClose={() => setIsRecordModalOpen(false)}
        initialProjects={projects}
      />
      <CreateProject
        isOpen={isProjectModalOpen}
        onClose={() => setIsProjectModalOpen(false)}
        onProjectCreated={refreshProjects}
      />
      <CreateWidget
        isOpen={widgetModal}
        onClose={() => setWidgetModal(false)}
      />
    </div>
  );
}
