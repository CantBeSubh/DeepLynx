// app/(home)/HomeDashboardClient.tsx  — Client Component
"use client";

import CreateWidget from "@/app/(home)/components/CreateWidgetsModal";
import { ExpandableTable } from "@/app/(home)/components/ExpandableTable";
import ExpandedProjectCard from "@/app/(home)/components/ExpandedProjectCard";
import { WidgetType } from "@/app/(home)/components/Widgets";
import { ProjectsList } from "@/app/(home)/types/types";
import { PlusIcon, QuestionMarkCircleIcon } from "@heroicons/react/24/outline";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useEffect, useRef } from "react";
import { useLanguage } from "../contexts/Language";
import { getAllProjects } from "../lib/projects_services.client";
import CreateProject from "./components/CreateProjectsModal";
import SearchInput from "./components/SearchInput";
import { format } from "date-fns";
import { useSession } from "next-auth/react";
import AddRecordModal from "./components/AddRecordModal";
import Shepherd from "shepherd.js";
import "shepherd.js/dist/css/shepherd.css";

type Props = { initialProjects: ProjectsList[] };

export default function HomeDashboardClient({ initialProjects }: Props) {
  const { t } = useLanguage();
  const router = useRouter();
  const { data: session, status } = useSession();
  const tourRef = useRef<any>(null);

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

  const filteredProjects = projects
    .filter((project) => {
      const term = searchTerm.toLowerCase();
      return (
        project.name.toLowerCase().includes(term) ||
        project.description?.toLowerCase().includes(term)
      );
    })
    .sort((a, b) => {
      const dateA = new Date(a.lastUpdatedAt!).getTime();
      const dateB = new Date(b.lastUpdatedAt!).getTime();
      return dateB - dateA;
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
          className="font-bold text-secondary hover:text-primary/80 underline underline-offset-2 transition-colors"
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
      header: t.translations.LAST_UPDATED_AT,
      data: (row: ProjectsList) => (
        <span className="text-base-content/60 text-sm">
          {format(new Date(row.lastUpdatedAt!), "MM/dd/yyyy hh:mm:s")}
        </span>
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

  // Initialize Shepherd Tour
  useEffect(() => {
    // Create tour instance
    const tour = new Shepherd.Tour({
      useModalOverlay: true,
      defaultStepOptions: {
        cancelIcon: {
          enabled: true,
        },
        scrollTo: true,
        modalOverlayOpeningPadding: 4,
      },
    });

    // Add tour steps
    tour.addStep({
      id: "intro",
      title: "Welcome to Your Dashboard! 👋",
      text: "Let's take a quick tour to help you get started with managing your projects and records.",
      buttons: [
        {
          text: "Skip",
          classes: "shepherd-button-secondary",
          action: () => tour.cancel(),
        },
        {
          text: "Next",
          action: () => tour.next(),
        },
      ],
    });

    tour.addStep({
      id: "search",
      title: "Search Your Projects",
      text: "Use this search bar to quickly find projects by name or description.",
      attachTo: {
        element: "[data-tour='search-input']",
        on: "bottom",
      },
      buttons: [
        {
          text: "Back",
          classes: "shepherd-button-secondary",
          action: () => tour.back(),
        },
        {
          text: "Next",
          action: () => tour.next(),
        },
      ],
    });

    tour.addStep({
      id: "projects-section",
      title: "Your Projects",
      text: "This is where all your projects are displayed. You can see project names, descriptions, and when they were last updated.",
      attachTo: {
        element: "[data-tour='projects-section']",
        on: "top",
      },
      buttons: [
        {
          text: "Back",
          classes: "shepherd-button-secondary",
          action: () => tour.back(),
        },
        {
          text: "Next",
          action: () => tour.next(),
        },
      ],
    });

    tour.addStep({
      id: "add-record",
      title: "Add Records",
      text: "Click here to add new records to your existing projects. This helps you track progress and updates.",
      attachTo: {
        element: "[data-tour='add-record']",
        on: "bottom",
      },
      buttons: [
        {
          text: "Back",
          classes: "shepherd-button-secondary",
          action: () => tour.back(),
        },
        {
          text: "Next",
          action: () => tour.next(),
        },
      ],
    });

    tour.addStep({
      id: "create-project",
      title: "Create New Projects",
      text: "Start a new project by clicking this button. You can set up all the project details and begin tracking immediately.",
      attachTo: {
        element: "[data-tour='create-project']",
        on: "bottom",
      },
      buttons: [
        {
          text: "Back",
          classes: "shepherd-button-secondary",
          action: () => tour.back(),
        },
        {
          text: "Next",
          action: () => tour.next(),
        },
      ],
    });

    tour.addStep({
      id: "complete",
      title: "You're All Set! 🎉",
      text: "That's it! You now know the basics. You can always restart this tour by clicking the help button in the header.",
      buttons: [
        {
          text: "Finish",
          action: () => {
            tour.complete();
            localStorage.setItem("dashboard-tour-completed", "true");
          },
        },
      ],
    });

    // Store tour reference
    tourRef.current = tour;

    // Auto-start for first-time users
    const hasSeenTour = localStorage.getItem("dashboard-tour-completed");
    if (!hasSeenTour && initialProjects.length === 0) {
      setTimeout(() => {
        tour.start();
      }, 500);
    }

    // Cleanup
    return () => {
      if (tourRef.current) {
        tourRef.current.complete();
      }
    };
  }, [initialProjects.length]);

  const startTour = () => {
    if (tourRef.current) {
      tourRef.current.start();
    }
  };

  return (
    <div className="min-h-screen bg-base-100">
      {/* Header Section */}
      <header className="bg-base-200/50 border-b border-base-300/30 sticky top-0 z-10 backdrop-blur-sm">
        <div className="flex justify-between items-center px-4 sm:px-6 lg:px-12 py-4">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-base-content">
              {t.translations.WELECOME}
            </h1>
            <button
              onClick={startTour}
              className="btn btn-ghost btn-sm btn-circle"
              title="Start Tour"
            >
              <QuestionMarkCircleIcon className="w-5 h-5" />
            </button>
          </div>
          <div data-tour="search-input">
            <SearchInput
              placeholder="Search Projects"
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="p-6">
        <div className="w-4/5 mx-auto">
          <div className="card card-border p-4" data-tour="projects-section">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-info-content text-lg font-semibold">
                {t.translations.YOUR_PROJECTS}
              </h3>

              <div className="flex gap-2 w-full sm:w-auto">
                <button
                  onClick={() => setIsRecordModalOpen(true)}
                  className="btn btn-outline btn-secondary btn-sm flex-1 sm:flex-initial"
                  data-tour="add-record"
                >
                  <PlusIcon className="size-5" />
                  <span>{t.translations.RECORD}</span>
                </button>
                <button
                  onClick={() => setIsProjectModalOpen(true)}
                  className="btn btn-secondary btn-sm flex-1 sm:flex-initial"
                  data-tour="create-project"
                >
                  <PlusIcon className="size-5" />
                  <span>{t.translations.PROJECT}</span>
                </button>
              </div>
            </div>

            <ExpandableTable
              data={filteredProjects}
              columns={columns}
              renderExpandedContent={(project, onClose) => (
                <ExpandedProjectCard project={project} onClose={onClose} />
              )}
              onExplore={onExplore}
            />
          </div>
        </div>

        {/* Commented out widgets section
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

      {/* Custom Shepherd Styles */}
      <style jsx global>{`
        .shepherd-element {
          background: oklch(86% 0.022 252.894); /* base-200 */
          border-radius: 1rem; /* radius-box */
          box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
          max-width: 400px;
        }

        .shepherd-header {
          padding: 1rem 1.25rem;
          background: oklch(86% 0.022 252.894); /* base-200 */
          border-radius: 1rem 1rem 0 0;
        }

        .shepherd-title {
          font-size: 1.125rem;
          font-weight: 600;
          color: oklch(13.454% 0.033 35.791); /* base-content */
        }

        .shepherd-text {
          padding: 1.25rem;
          color: oklch(13.454% 0.033 35.791); /* base-content */
          line-height: 1.6;
          background: oklch(86% 0.022 252.894); /* base-200 */
          font-size: 0.95rem;
        }

        .shepherd-footer {
          padding: 1rem 1.25rem 1.25rem;
          display: flex;
          justify-content: flex-end;
          gap: 0.75rem;
          background: oklch(86% 0.022 252.894); /* base-200 */
          border-radius: 0 0 1rem 1rem;
        }

        .shepherd-button {
          padding: 0.5rem 1.25rem;
          border-radius: 0.5rem; /* radius-field */
          font-weight: 500;
          background: oklch(33.78% 0.1 254.3); /* primary */
          color: oklch(88.34% 0.019 251.473); /* primary-content */
          border: none;
          cursor: pointer;
          transition: all 0.2s ease;
          font-size: 0.9rem;
        }

        .shepherd-button:hover {
          background: oklch(33.78% 0.12 254.3); /* primary hover */
          transform: translateY(-1px);
          box-shadow: 0 4px 12px oklch(33.78% 0.1 254.3 / 0.3);
        }

        .shepherd-button-secondary {
          background: oklch(98% 0.003 247.858); /* base-100 */
          color: oklch(13.454% 0.033 35.791); /* base-content */
          border: 1.5px solid oklch(55% 0.046 257.417); /* base-300 */
        }

        .shepherd-button-secondary:hover {
          background: oklch(
            55% 0.046 257.417 / 0.2
          ); /* base-300 with opacity */
          transform: translateY(-1px);
          box-shadow: none;
        }

        .shepherd-modal-overlay-container {
          background: oklch(
            13.454% 0.033 35.791 / 0.5
          ); /* base-content with opacity */
          backdrop-filter: blur(4px);
        }

        .shepherd-arrow:before {
          background: oklch(86% 0.022 252.894); /* base-200 */
          border: 1.5px solid oklch(55% 0.046 257.417); /* base-300 */
        }

        .shepherd-modal-is-visible .shepherd-element {
          opacity: 1 !important;
        }

        .shepherd-cancel-icon {
          color: oklch(13.454% 0.033 35.791); /* base-content */
        }

        .shepherd-cancel-icon:hover {
          color: oklch(70.61% 0.146 29.674); /* error color for cancel */
        }

        /* Add smooth entrance animation */
        .shepherd-element {
          animation: shepherd-fade-in 0.3s ease-out;
        }

        @keyframes shepherd-fade-in {
          from {
            opacity: 0;
            transform: scale(0.95);
          }
          to {
            opacity: 1;
            transform: scale(1);
          }
        }

        /* Ensure good contrast for step counter if it appears */
        .shepherd-progress {
          color: oklch(
            13.454% 0.033 35.791 / 0.7
          ); /* base-content with reduced opacity */
          font-size: 0.875rem;
        }
      `}</style>
    </div>
  );
}
