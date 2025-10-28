"use client";

import React, { useEffect, useState } from "react";
import ProjectDropdown from "../components/ProjectDropdown";
import { useLanguage } from "@/app/contexts/Language";
import { getAllTags } from "@/app/lib/tag_services.client";
import { TagResponseDto } from "../types/responseDTOs";
import SearchTags from "./search_create_attach_edit-tag-page/SearchTags";

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
  const [filteredTags, setFilteredTags] = useState<TagResponseDto[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedTag, setSelectedTag] = useState<TagResponseDto | null>(null);

  const menuItems = ["Search Tags", "Create Tag", "Attach Tags", "Edit Tags"];
  // Fetches tags when ever project changes - by default it fetches all tags from all projects
  useEffect(() => {
    const fetchTags = async () => {
      if (selectedProjects.length === 0) {
        setTags([]);
        setFilteredTags([]);
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
        setFilteredTags(allTags);
      } catch (error) {
        setError("Failed to fetch tags");
        console.error("Error fetching tags:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchTags();
  }, [selectedProjects]);

  // Does a client side filter for tags
  useEffect(() => {
    if (!searchQuery.trim()) {
      setFilteredTags(tags);
      return;
    }

    const query = searchQuery.toLowerCase();
    const filtered = tags.filter((tag) =>
      tag.name?.toLowerCase().includes(query)
    );
    setFilteredTags(filtered);
  }, [searchQuery, tags]);

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
      <div
        className={`grid ${
          selectedTag ? "grid-cols-[20%_40%_40%]" : "grid-cols-[20%_40%]"
        } p-6 transition-all`}
      >
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
            <SearchTags
              loading={loading}
              error={error}
              filteredTags={filteredTags}
              tags={tags}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default TagManagementClient;
