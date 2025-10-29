"use client";

import React, { useCallback, useEffect, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import { getAllTags } from "@/app/lib/tag_services.client";
import { ProjectResponseDto, TagResponseDto } from "../types/responseDTOs";
import SearchTags from "./search_create_attach_edit-tag-page/SearchTags";
import CreateTag, {
  CreateTagRecordsList,
} from "./search_create_attach_edit-tag-page/CreateTag";
import ProjectDropdownSingleSelect from "../components/ProjectDropdownSingleSelect";

interface Props {
  initialProjects: ProjectResponseDto[];
  initialSelectedProjects: ProjectResponseDto;
}

const TagManagementClient = ({
  initialProjects,
  initialSelectedProjects,
}: Props) => {
  const { t } = useLanguage();
  const [projects] = useState<ProjectResponseDto[]>(initialProjects);
  const [selectedProject, setSelectedProject] = useState<string>(
    initialSelectedProjects?.id?.toString() || ""
  );
  const [selectedMenuItem, setSelectedMenuItems] = useState("Search Tags");
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [filteredTags, setFilteredTags] = useState<TagResponseDto[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedTag, setSelectedTag] = useState<TagResponseDto | null>(null);
  const [selectedTagIds, setSelectedTagIds] = useState<Set<number>>(new Set());

  const menuItems = ["Search Tags", "Create Tag", "Attach Tags", "Edit Tags"];

  // Fetches tags when project changes
  useEffect(() => {
    const fetchTags = async () => {
      if (!selectedProject) {
        setTags([]);
        setFilteredTags([]);
        return;
      }

      setLoading(true);
      setError(null);

      try {
        const allTags = await getAllTags(Number(selectedProject));
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
  }, [selectedProject]);

  // Create a refetch function
  const refetchTags = useCallback(async () => {
    if (!selectedProject) {
      setTags([]);
      setFilteredTags([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const allTags = await getAllTags(Number(selectedProject));
      setTags(allTags);
      setFilteredTags(allTags);
    } catch (error) {
      setError("Failed to fetch tags");
      console.error("Error fetching tags:", error);
    } finally {
      setLoading(false);
    }
  }, [selectedProject]);

  // Update the existing useEffect to use refetchTags
  useEffect(() => {
    refetchTags();
  }, [refetchTags]);

  const handleProjectChange = useCallback((newProjectId: string) => {
    setSelectedProject(newProjectId);
  }, []);

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
        <ProjectDropdownSingleSelect
          projects={projects}
          onSelectionChange={handleProjectChange}
          defaultSelectedId={initialSelectedProjects?.id?.toString() || ""}
        />
      </div>

      {/* Content - Always 3 columns */}
      <div className="grid grid-cols-[20%_40%_40%] p-6 transition-all">
        {/* Menu Column */}
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

        {/* Middle Column - Main Content */}
        <div className="card shadow-xl rounded-lg p-6 mr-6">
          {selectedMenuItem === "Search Tags" && (
            <SearchTags
              loading={loading}
              error={error}
              filteredTags={filteredTags}
              tags={tags}
            />
          )}
          {selectedMenuItem === "Create Tag" && (
            <CreateTag
              projectId={selectedProject}
              onTagCreated={refetchTags}
              selectedTagIds={selectedTagIds}
              setSelectedTagIds={setSelectedTagIds}
            />
          )}
          {selectedMenuItem === "Attach Tags" && (
            <div>
              <h3 className="font-bold mb-4">Attach Tags</h3>
              {/* Attach Tags content */}
            </div>
          )}
          {selectedMenuItem === "Edit Tags" && (
            <div>
              <h3 className="font-bold mb-4">Edit Tags</h3>
              {/* Edit Tags content */}
            </div>
          )}
        </div>

        {/* Right Column - Context-specific content */}
        <div className="card shadow-xl rounded-lg p-6">
          {selectedMenuItem === "Search Tags" && (
            <div>
              <h3 className="font-bold mb-4">Tag Details</h3>
              {/* Tag details will go here */}
            </div>
          )}
          {selectedMenuItem === "Create Tag" && (
            <div>
              <CreateTagRecordsList
                projectId={selectedProject}
                selectedTagIds={selectedTagIds}
              />
            </div>
          )}
          {selectedMenuItem === "Attach Tags" && (
            <div>
              <h3 className="font-bold mb-4">Selected Records</h3>
              {/* Selected records to attach tags to */}
            </div>
          )}
          {selectedMenuItem === "Edit Tags" && (
            <div>
              <h3 className="font-bold mb-4">Edit History</h3>
              {/* Edit history or preview */}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TagManagementClient;
