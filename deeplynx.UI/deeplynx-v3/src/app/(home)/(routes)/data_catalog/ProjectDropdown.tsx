import React, { useEffect, useRef, useState } from "react";
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";

interface ProjectDropdownProps {
  projects: { id: string; name: string }[];
  onSelectionChange?: (selected: string[]) => void;
  defaultSelected?: string[];
}

const ProjectDropdown: React.FC<ProjectDropdownProps> = ({
  projects,
  onSelectionChange,
  defaultSelected,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const allIds = projects.map((p) => p.id);

  // ⏳ Apply defaultSelected when loaded
  useEffect(() => {
    if (!projects.length) return;

    if (defaultSelected?.length) {
      setSelectedIds(defaultSelected);
    } else {
      setSelectedIds(["ALL"]);
    }
  }, [projects.length, defaultSelected?.toString()]);

  // 🔄 Notify parent anytime selectedIds changes
  useEffect(() => {
    if (!projects.length) return;

    const isAll = selectedIds.includes("ALL");
    onSelectionChange?.(isAll ? allIds : selectedIds);
  }, [selectedIds, projects.length]);

  // 🧹 Close dropdown on outside click
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

  const toggleProject = (id: string) => {
    let newSelection: string[];

    if (id === "ALL") {
      newSelection = ["ALL"];
    } else {
      newSelection = selectedIds.includes(id)
        ? selectedIds.filter((sid) => sid !== id)
        : [...selectedIds.filter((sid) => sid !== "ALL"), id];

      if (newSelection.length === 0) {
        newSelection = ["ALL"];
      }
    }

    setSelectedIds(newSelection);
  };

  const filteredProjects = projects.filter((p) =>
    p.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedLabel = (() => {
    if (selectedIds.includes("ALL")) return "All your Projects";
    if (selectedIds.length === 1) {
      const project = projects.find((p) => p.id === selectedIds[0]);
      return project?.name || "1 project selected";
    }
    return `${selectedIds.length} projects selected`;
  })();

  return (
    <div className="relative inline-block text-left min-w-sm" ref={dropdownRef}>
      <button
        className="flex items-center gap-1 font-semibold text-lg"
        onClick={() => setIsOpen(!isOpen)}
      >
        {selectedLabel}{" "}
        {selectedLabel === "All your Projects" && `(${projects.length})`}
        {isOpen ? (
          <ChevronUpIcon className="w-5 h-5 ml-1" />
        ) : (
          <ChevronDownIcon className="w-5 h-5 ml-1" />
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
                checked={selectedIds.includes("ALL")}
                onChange={() => toggleProject("ALL")}
              />
              <span className="label-text">All your Projects</span>
            </label>
          </div>

          <div className="flex flex-col gap-2">
            {filteredProjects.map((project) => (
              <label
                key={project.id}
                className="label cursor-pointer justify-start gap-2"
              >
                <input
                  type="checkbox"
                  className="checkbox checkbox-primary"
                  checked={selectedIds.includes(project.id)}
                  onChange={() => toggleProject(project.id)}
                />
                <span className="label-text">{project.name}</span>
              </label>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDropdown;
