"use client";

import React, { useEffect, useState } from "react";
import ProjectDropdown from "../components/ProjectDropdown";
import { useLanguage } from "@/app/contexts/Language";
import AdvancedSearchBar from "../components/AdvancedSearchBar";
import { getAllTags } from "@/app/lib/tag_services.client";
import { TagResponseDto } from "../types/responseDTOs";

interface Props {
  initialProjects: { id: string; name: string }[];
  initialSelectedProjects: string[];
}

const TagManagementClient = ({
  initialProjects,
  initialSelectedProjects,
}: Props) => {
  const { t } = useLanguage();
  const [projects] = useState(initialProjects);
  const [selectedProjects, setSelectedProjects] = useState<string[]>(
    initialSelectedProjects
  );
  const [selectedMenuItem, setSelectedMenuItems] = useState("Search Tags");
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const menuItems = ["Search Tags", "Create Tag", "Attach Tags", "Edit Tags"];

  useEffect(() => {
    const fetchTags = async () => {
      if (selectedProjects.length === 0) {
        setTags([]);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        const tagPromise = selectedProjects.map((projectId) =>
          getAllTags(Number(projectId))
        );
        const tagResults = await Promise.all(tagPromise);
        const allTags = tagResults.flat();
        setTags(allTags);
      } catch (error) {
        setError("Failed to fetch tags");
        console.error("Error fetching tags:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchTags();
  }, [selectedProjects]);

  return (
    <div>
      {/* Header */}
      <div className="items-center bg-base-200/40 pl-12 py-2 pb-4">
        <h1 className="text-2xl font-bold text-info-content">Tag Management</h1>
        <ProjectDropdown
          projects={projects}
          onSelectionChange={setSelectedProjects}
          defaultSelected={
            initialSelectedProjects.length ? initialSelectedProjects : undefined
          }
        />
      </div>

      {/* Content */}
      <div className="grid grid-cols-[20%_40%_40%] p-6">
        <div className="card shadow-xl rounded-lg p-6 mr-6">
          <ul>
            {menuItems.map((item) => (
              <li
                key={item}
                onClick={() => setSelectedMenuItems(item)}
                className={`cursor-pointer px-4 py-2 rounded-lg transition-colors font-bold ${
                  selectedMenuItem === item
                    ? "bg-info/50 text-info-content"
                    : "hover:bg-base-200"
                }`}
              >
                {item}
              </li>
            ))}
          </ul>
        </div>
        <div className="card shadow-xl rounded-lg p-6 mr-6">
          {selectedMenuItem === "Search Tags" && (
            <div>
              <h3 className="font-bold">Search Tags</h3>{" "}
              <AdvancedSearchBar placeholder="Search Tags" />
              <div className="mt-4">
                {loading && <p>Loading tags ...</p>}
                {error && (
                  <p className="text-error flex justify-center">{error}</p>
                )}
                {!loading && tags.length === 0 && (
                  <p className="text-base-300">No Tags found</p>
                )}
                {!loading && tags.length > 0 && (
                  <div className="space-y-2">
                    <p className="text-sm font-semibold">
                      Tags ({tags.length}):
                    </p>
                    <ul className="space-y-1">
                      {tags.map((tag, index) => (
                        <li key={index} className="px-3 py-1">
                          <input
                            type="checkbox"
                            className="checkbox checkbox-primary"
                          />
                          <span className="badge ml-2">
                            {tag.name || JSON.stringify(tag)}
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
        <div className="card shadow-xl rounded-lg p-6">Records</div>
      </div>
    </div>
  );
};

export default TagManagementClient;
