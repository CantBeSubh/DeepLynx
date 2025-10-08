"use client";

import { useLanguage } from "@/app/contexts/Language";
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import React, { useEffect, useMemo, useRef, useState } from "react";
import { ProjectDTO } from "../types/responseDTOs/projectResponseDto";

interface ProjectDropdownSingleSelectProps {
  projects: ProjectDTO[];
  onSelectionChange?: (selected: string) => void;
  defaultSelectedId?: string;
}

const ProjectDropdownSingleSelect: React.FC<
  ProjectDropdownSingleSelectProps
> = ({ projects, onSelectionChange, defaultSelectedId }) => {
  const { t } = useLanguage();
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedId, setSelectedId] = useState(
    defaultSelectedId ? defaultSelectedId : ""
  );
  const dropDownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!projects.length) return;
    if (defaultSelectedId) {
      setSelectedId(defaultSelectedId);
    }
  }, [projects.length, defaultSelectedId]);

  // Notify parent anytime selectedId changes (and projects exists)
  useEffect(() => {
    if (!projects.length) return;
    if (onSelectionChange) {
      onSelectionChange(selectedId);
    }
  }, [selectedId, projects.length, onSelectionChange]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        dropDownRef.current &&
        !dropDownRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const toggleProject = (id: string) => {
    setSelectedId(id);
  };

  const filteredProjects = useMemo(
    () =>
      projects.filter((p) =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase())
      ),
    [projects, searchTerm]
  );

  const selectedLabel = useMemo(() => {
    if (selectedId !== "") {
      const project = projects.find((p) => p.id === selectedId);
      return project?.name || "";
    }
    return "";
  }, [selectedId, projects]);

  return (
    <div className="relative inline-block text-left min-w-sm" ref={dropDownRef}>
      <button
        className="flex items-center gap-1 text-md text-base-content hover:text-secondary transition-colors font-medium"
        onClick={() => setIsOpen((currentState) => !currentState)}
        type="button"
      >
        {selectedLabel || (
          <span className="text-base-content/60">
            {t.translations.SELECT_PROJECT || "Select Project"}
          </span>
        )}{" "}
        {isOpen ? (
          <ChevronUpIcon className="w-5 h-5 ml-1" />
        ) : (
          <ChevronDownIcon className="w-5 h-5 ml-1" />
        )}
      </button>

      {isOpen && (
        <div className="absolute z-50 mt-2 w-full min-w-[250px] bg-base-200 border border-base-300 shadow-xl rounded-box p-4 max-h-80 overflow-auto">
          <input
            type="text"
            placeholder={t.translations.SEARCH}
            className="input input-bordered input-sm w-full mb-4 bg-base-100 text-base-content placeholder:text-base-content/50"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <div className="flex flex-col gap-1">
            {filteredProjects.length > 0 ? (
              filteredProjects.map((project) => (
                <label
                  key={project.id}
                  className="label cursor-pointer justify-start gap-3 hover:bg-base-300/50 rounded-lg px-2 py-1 transition-colors"
                >
                  <input
                    type="radio"
                    name="project"
                    className="radio radio-secondary radio-sm"
                    checked={selectedId === project.id}
                    onChange={() => {
                      if (project.id) {
                        toggleProject(project.id.toString());
                      }
                    }}
                  />
                  <span className="label-text text-base-content flex-1">
                    {project.name}
                  </span>
                </label>
              ))
            ) : (
              <div className="text-base-content/60 text-sm py-2 px-2">
                {t.translations.NO_PROJECT_FOUND}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDropdownSingleSelect;
