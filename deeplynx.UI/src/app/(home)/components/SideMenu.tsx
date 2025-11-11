// src/app/(home)/components/SideMenu.tsx
"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import React, { useEffect, useState, useCallback } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { useProjectSession } from "@/app/contexts/ProjectSessionProvider";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import { getAllProjects } from "@/app/lib/projects_services.client";
import { ProjectResponseDto } from "../types/responseDTOs";
import {
  AdjustmentsHorizontalIcon,
  ArrowUpTrayIcon,
  BellIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronUpIcon,
  FolderIcon,
  HomeIcon,
  QuestionMarkCircleIcon,
  RectangleGroupIcon,
  TagIcon,
} from "@heroicons/react/24/outline";

interface SideMenuProps {
  onToggle: (isCollapsed: boolean) => void;
}

const SideMenu: React.FC<SideMenuProps> = ({ onToggle }) => {
  const { t } = useLanguage();
  const router = useRouter();
  const pathname = usePathname();
  const { project, setProject } = useProjectSession();
  const { organization } = useOrganizationSession();

  // State variables
  const [selectedItem, setSelectedItem] = useState<string>("");
  const [isCollapsed, setIsCollapsed] = useState<boolean>(false);
  const [isProjectsExpanded, setIsProjectsExpanded] = useState<boolean>(false);
  const [projects, setProjects] = useState<ProjectResponseDto[]>([]);
  const [loadingProjects, setLoadingProjects] = useState(false);

  // Memoize fetchProjects to prevent it from changing on every render
  const fetchProjects = useCallback(async () => {
    if (!organization) return;

    try {
      setLoadingProjects(true);
      const data = await getAllProjects(organization.organizationId, true);
      setProjects(data);
    } catch (error) {
      console.error("Failed to fetch projects:", error);
    } finally {
      setLoadingProjects(false);
    }
  }, [organization]);

  // Fetch projects when organization changes
  useEffect(() => {
    fetchProjects();
  }, [fetchProjects]);

  // Effect to set the selected item based on the current pathname
  useEffect(() => {
    setSelectedItem(pathname);
  }, [pathname]);

  // Effect to get the selected item from localStorage on initial render
  useEffect(() => {
    const savedItem = localStorage.getItem("selectedItem");
    if (savedItem) setSelectedItem(savedItem);
  }, []);

  // Effect to save the selected item to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem("selectedItem", selectedItem);
  }, [selectedItem]);

  useEffect(() => {
    onToggle(isCollapsed);
  }, [isCollapsed, onToggle]);

  // Function to toggle the collapse state of the menu
  const toggleMenu = () => {
    const newState = !isCollapsed;
    setIsCollapsed(newState);
    onToggle(newState);
  };

  // Function to handle project selection
  const handleProjectClick = (selectedProject: ProjectResponseDto) => {
    setProject({
      projectId: selectedProject.id.toString(),
      projectName: selectedProject.name,
    });
    router.push(`/project/${selectedProject.id}`);
  };

  // Function to handle item click events
  const handleItemClick = (
    item: string,
    event: React.MouseEvent<HTMLElement>
  ) => {
    event.preventDefault();
    setSelectedItem(item);
    router.push(item);
  };

  // Function to check if an item is disabled
  const isDisabled = (targetPath: string) =>
    pathname === "/" && targetPath !== "/";

  // Function to get the CSS class for an item based on its state
  const getItemClass = (targetPath: string) => {
    const alwaysActivePaths = [
      "/settings",
      "/help",
      "/contact",
      "/fileBug",
      "/upload_center",
      "project_homepage",
      "/member_management",
      "/event_management",
      "/tag_management",
    ];
    const isExactMatch = selectedItem === targetPath;
    const isDynamicProject =
      targetPath === "/project/[id]" && /^\/project\/[^/]+$/.test(pathname);
    const isSelected = isExactMatch || isDynamicProject;
    const isAlwaysActive = alwaysActivePaths.includes(targetPath);

    return [
      "flex items-center block py-2 px-4 rounded transition",
      isSelected ? "bg-info/30" : "hover:bg-info/30",
      isDisabled(targetPath) && !isAlwaysActive
        ? "pointer-events-none opacity-50 cursor-not-allowed"
        : "",
    ].join(" ");
  };

  // Check if a project is currently active
  const isProjectActive = (projectId: string | number) => {
    // Check if we're on the project page
    const isOnProjectPage = pathname.includes(`/project/${projectId}`);

    // Check if this is the current session project (for data catalog, etc.)
    const isSessionProject = project?.projectId === projectId.toString();

    return isOnProjectPage || isSessionProject;
  };

  return (
    <div className="fixed top-18 bottom-0 left-18 flex z-30">
      <aside
        className={`h-full shadow-xl ${
          isCollapsed ? "w-22" : "w-64"
        } bg-[var(--base-400)] brightness-120 text-primary-content p-4 transition-all duration-300 flex flex-col overflow-y-auto`}
      >
        {/* Projects Section */}
        <div className="mt-5">
          <div
            className="flex items-center justify-between py-2 px-4 cursor-pointer hover:bg-info/20 rounded transition"
            onClick={() => setIsProjectsExpanded(!isProjectsExpanded)}
          >
            <div className="flex items-center">
              <FolderIcon className="size-6" />
              {!isCollapsed && <p className="ml-2 font-semibold">Projects</p>}
            </div>
            {!isCollapsed && (
              <button className="btn btn-ghost btn-xs btn-circle">
                {isProjectsExpanded ? (
                  <ChevronUpIcon className="size-4" />
                ) : (
                  <ChevronDownIcon className="size-4" />
                )}
              </button>
            )}
          </div>

          {/* Projects List */}
          {!isCollapsed && isProjectsExpanded && (
            <ul className="ml-4 mt-2 space-y-1 max-h-64 overflow-y-auto">
              {loadingProjects ? (
                <li className="py-2 px-4 text-sm text-primary-content/70">
                  <span className="loading loading-spinner loading-sm"></span>
                  <span className="ml-2">Loading...</span>
                </li>
              ) : projects.length === 0 ? (
                <li className="py-2 px-4 text-sm text-base-content/70">
                  No projects found
                </li>
              ) : (
                projects.map((proj) => (
                  <li key={proj.id}>
                    <button
                      onClick={() => handleProjectClick(proj)}
                      className={`w-full text-left py-2 px-4 rounded transition text-sm flex items-center ${
                        isProjectActive(proj.id)
                          ? "bg-info/30 text-primary-content font-semibold"
                          : "hover:bg-info/20 text-primary-content"
                      }`}
                    >
                      <span className="truncate">{proj.name}</span>
                      {isProjectActive(proj.id) && (
                        <span className="ml-auto badge badge-xs">Active</span>
                      )}
                    </button>
                  </li>
                ))
              )}
            </ul>
          )}
        </div>

        <div className="divider" />

        {/* Home */}
        <ul>
          <li>
            <Link
              href={`/project/${project?.projectId}`}
              prefetch={false}
              onClick={(e) =>
                handleItemClick(`/project/${project?.projectId}`, e)
              }
              className={getItemClass(`/project/${project?.projectId}`)}
            >
              <RectangleGroupIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Project Dashboard</p>}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href={"/upload_center"}
              className={getItemClass("/upload_center")}
            >
              <ArrowUpTrayIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.UPLOAD_CENTER}</p>
              )}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href="/tag_management"
              prefetch={false}
              className={getItemClass("/tag_management")}
            >
              <TagIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Tag Management</p>}
            </Link>
          </li>
          <li className="mt-2">
            <Link
              href={"/member_management"}
              onClick={(e) => handleItemClick("/member_management", e)}
              className={getItemClass("/member_management")}
            >
              <AdjustmentsHorizontalIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.MEMBER_MANAGEMENT}</p>
              )}
            </Link>
          </li>

          <li className="mt-2">
            <Link
              href="#"
              onClick={(e) => handleItemClick("/event_management", e)}
              className={getItemClass("/event_management")}
            >
              <BellIcon className="size-6" />
              {!isCollapsed && <p className="ml-2">Event Management</p>}
            </Link>
          </li>
        </ul>

        <div className="divider" />

        <ul className="flex-grow">
          <li className="mt-2">
            <Link
              href={`/project/${project?.projectId || ""}/project_settings`}
              onClick={(e) =>
                handleItemClick(
                  `/project/${project?.projectId || ""}/project_settings`,
                  e
                )
              }
              className={getItemClass(
                `/project/${project?.projectId || ""}/project_settings`
              )}
            >
              <AdjustmentsHorizontalIcon className="size-6" />
              {!isCollapsed && (
                <p className="ml-2">{t.translations.PROJECT_SETINGS}</p>
              )}
            </Link>
          </li>
        </ul>

        <div className="divider" />

        {/* Last Menu Items */}
        <div className="mt-auto">
          <ul>
            <li className="mt-2">
              <button className={getItemClass("/help")}>
                <a
                  href={
                    process.env.NEXT_PUBLIC_DOCS_PATH
                      ? `${process.env.NEXT_PUBLIC_DOCS_PATH}`
                      : "/docs"
                  }
                  className="flex items-center"
                >
                  <QuestionMarkCircleIcon className="size-6" />
                  {!isCollapsed && (
                    <div className="ml-2">{t.translations.HELP}</div>
                  )}
                </a>
              </button>
            </li>
          </ul>
        </div>
      </aside>

      {/* Toggle tab */}
      <div
        className="h-8 w-4 bg-base-300 brightness-120 text-primary-content flex items-center justify-center cursor-pointer rounded-r-md mt-16"
        onClick={toggleMenu}
      >
        {isCollapsed ? (
          <ChevronRightIcon className="size-6" />
        ) : (
          <ChevronLeftIcon className="size-6" />
        )}
      </div>
    </div>
  );
};

export default SideMenu;
