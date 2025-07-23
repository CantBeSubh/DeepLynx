import React, { useEffect, useRef, useState } from "react";
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import { getAllProjects } from "@/app/lib/projects_services";
import { FileViewerTableRow } from "../../types/types";

interface ProjectDropdownProps {
  projects: string[];
}

const ProjectDropdown: React.FC<ProjectDropdownProps> = ({ projects }) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedProjects, setSelectedProjects] = useState<string[]>([
    "All your Projects",
  ]);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Close dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const toggleProject = (project: string) => {
    if (project === "All your Projects") {
      setSelectedProjects(["All your Projects"]);
    } else {
      setSelectedProjects((prev) => {
        const newSelection = prev.includes(project)
          ? prev.filter((p) => p !== project)
          : [...prev.filter((p) => p !== "All your Projects"), project];

        return newSelection.length > 0 ? newSelection : ["All your Projects"];
      });
    }
  };

  const filteredProjects = projects.filter((p) =>
    p.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedLabel =
    selectedProjects.length === 1
      ? selectedProjects[0]
      : `${selectedProjects.length} selected`;

  return (
    <div className="relative inline-block text-left min-w-sm" ref={dropdownRef}>
      <button
        className="flex items-center gap-1 font-semibold text-lg"
        onClick={() => setIsOpen(!isOpen)}
      >
        {selectedLabel} ({projects.length})
        {isOpen ? (
          <ChevronUpIcon className="w-5 h-5" />
        ) : (
          <ChevronDownIcon className="w-5 h-5" />
        )}
      </button>

      {isOpen && (
        <div className="absolute z-10 mt-2 w-full bg-base-100 shadow rounded-box p-4 max-h-80 overflow-auto">
          <input
            type="text"
            placeholder="Search"
            className="input input-bordered w-full mb-4"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />

          <div className="space-y-2">
            <label className="label cursor-pointer justify-start gap-2">
              <input
                type="checkbox"
                className="checkbox checkbox-primary"
                checked={selectedProjects.includes("All your Projects")}
                onChange={() => toggleProject("All your Projects")}
              />
              <span className="label-text">All your Projects</span>
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {filteredProjects.map((project) => (
              <label
                key={project}
                className="label cursor-pointer justify-start gap-2"
              >
                <input
                  type="checkbox"
                  className="checkbox checkbox-primary"
                  checked={selectedProjects.includes(project)}
                  onChange={() => toggleProject(project)}
                />
                <span className="label-text">{project}</span>
              </label>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDropdown;
