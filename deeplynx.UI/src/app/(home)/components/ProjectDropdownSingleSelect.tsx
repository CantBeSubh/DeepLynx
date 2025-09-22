"use client"

import { useLanguage } from "@/app/contexts/Language";
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import React, { useEffect, useMemo, useRef, useState } from "react";
import { ProjectsList } from "@/app/(home)/types/types";

interface ProjectDropdownSingleSelectProps {
    projects: ProjectsList[];
    onSelectionChange?: (selected: string) => void;
    defaultSelectedId?: string;
}

const ProjectDropdownSingleSelect: React.FC<ProjectDropdownSingleSelectProps> = ({
    projects,
    onSelectionChange,
    defaultSelectedId
}) => {
    const { t } = useLanguage();
    const [isOpen, setIsOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const [selectedId, setSelectedId] = useState(defaultSelectedId ? defaultSelectedId : "");
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
    }, [selectedId, projects.length, onSelectionChange, defaultSelectedId]);

    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (
                dropDownRef.current 
                && !dropDownRef.current.contains(e.target as Node)
            ){
                setIsOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);
    
    const toggleProject = (id: string) => {
        setSelectedId(id)
    }
    
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
        <div
            className="relative inline-block text-left min-w-sm text-accent-content"
            ref={dropDownRef}
            >
            <button
                className="flex items-center gap-1 text-md"
                onClick={() => setIsOpen(currentState => !currentState)}
                type="button"
            >
                {selectedLabel} {" "}
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
                    <div className="flex flex-col gap-2">
                        {filteredProjects.map((project) => (
                            <label
                                key={project.id}
                                className="label cursor-pointer justify-start gap-2"
                            >
                                <input
                                    type="radio"
                                    name="project"
                                    className="radio radio-primary"
                                    checked={selectedId === project.id}
                                    onChange={() => {
                                        if (project.id) {
                                            toggleProject(project.id);
                                        }
                                    }}
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

export default ProjectDropdownSingleSelect;