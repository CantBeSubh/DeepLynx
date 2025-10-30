"use client";

import React, { useCallback, useEffect, useState } from "react";
import { useLanguage } from "@/app/contexts/Language";
import {
  getAllTags,
  updateTag,
  deleteTag,
} from "@/app/lib/tag_services.client";
import {
  ProjectResponseDto,
  RecordResponseDto,
  TagResponseDto,
} from "../types/responseDTOs";
import ProjectDropdownSingleSelect from "../components/ProjectDropdownSingleSelect";
import { getRecordsByTags } from "@/app/lib/record_services.client";
import toast from "react-hot-toast";
import SearchTags, {
  SearchTagsRecordsList,
} from "./search_create_attach_edit-tag-page/SearchTags";
import CreateTag, {
  CreateTagRecordsList,
} from "./search_create_attach_edit-tag-page/CreateTag";
import AttachTags, {
  AttachTagsRecordsList,
} from "./search_create_attach_edit-tag-page/AttachTags";
import EditTags, {
  EditTagsNameFields,
} from "./search_create_attach_edit-tag-page/EditTags";

const parseTags = (
  tags: string | TagResponseDto[] | undefined | null
): TagResponseDto[] => {
  if (!tags) return [];

  if (typeof tags === "string") {
    try {
      return JSON.parse(tags) as TagResponseDto[];
    } catch (e) {
      console.error("Error parsing tags:", e);
      return [];
    }
  }

  if (Array.isArray(tags)) {
    return tags;
  }

  return [];
};

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
  const [recordsFromTagSearch, setRecordsFromTagSearch] = useState<
    RecordResponseDto[]
  >([]);
  const [isSearchingByTags, setIsSearchingByTags] = useState(false);

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

  // Add the search handler
  const handleSearchByTags = async (tagIds: number[]) => {
    setIsSearchingByTags(true);
    try {
      const records = await getRecordsByTags(Number(selectedProject), tagIds);

      if (records.length === 0) {
        toast.error("No records found with these tags");
        setRecordsFromTagSearch([]);
        return;
      }

      // Parse tags for each record
      const recordsWithParsedTags = records.map(
        (record: RecordResponseDto) => ({
          ...record,
          tags: parseTags(
            record.tags as string | TagResponseDto[] | undefined | null
          ),
        })
      );

      setRecordsFromTagSearch(recordsWithParsedTags);
      toast.success(
        `Found ${records.length} record${
          records.length !== 1 ? "s" : ""
        } with selected tags`
      );
    } catch (error) {
      console.error("Error fetching records by tags:", error);
      toast.error("Failed to search records");
      throw error;
    } finally {
      setIsSearchingByTags(false);
    }
  };

  const handleClearSearch = () => {
    setRecordsFromTagSearch([]);
    setSelectedTagIds(new Set());
  };

  const handleRefreshSearch = async () => {
    if (selectedTagIds.size > 0) {
      await handleSearchByTags(Array.from(selectedTagIds));
    }
  };

  // Add handler for updating tags
  const handleUpdateTag = async (tagId: number, newName: string) => {
    try {
      await updateTag(Number(selectedProject), tagId, { name: newName });
      // Refetch tags to get the updated list
      await refetchTags();
    } catch (error) {
      console.error("Error updating tag:", error);
      throw error;
    }
  };

  // Add the delete handler
  const handleDeleteTag = async (tagId: number) => {
    try {
      await deleteTag(Number(selectedProject), tagId);
      // Remove the deleted tag from selected tags
      const newSelected = new Set(selectedTagIds);
      newSelected.delete(tagId);
      setSelectedTagIds(newSelected);
      // Refetch tags to get the updated list
      await refetchTags();
    } catch (error) {
      console.error("Error deleting tag:", error);
      throw error;
    }
  };

  // Get the selected tag objects from the selected IDs
  const selectedTags = tags.filter((tag) => selectedTagIds.has(tag.id));

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
              searchQuery={searchQuery}
              onSearchChange={setSearchQuery}
              selectedTagIds={selectedTagIds}
              setSelectedTagIds={setSelectedTagIds}
              projectId={selectedProject}
              onSearchByTags={handleSearchByTags}
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
              <AttachTags
                loading={loading}
                error={error}
                filteredTags={filteredTags}
                tags={tags}
                searchQuery={searchQuery}
                onSearchChange={setSearchQuery}
                selectedTagIds={selectedTagIds}
                setSelectedTagIds={setSelectedTagIds}
              />
            </div>
          )}
          {selectedMenuItem === "Edit Tags" && (
            <div>
              <EditTags
                loading={loading}
                error={error}
                filteredTags={filteredTags}
                tags={tags}
                searchQuery={searchQuery}
                onSearchChange={setSearchQuery}
                selectedTagIds={selectedTagIds}
                setSelectedTagIds={setSelectedTagIds}
              />
            </div>
          )}
        </div>

        {/* Right Column - Context-specific content */}
        <div className="card shadow-xl rounded-lg p-6">
          {selectedMenuItem === "Search Tags" && (
            <div>
              <SearchTagsRecordsList
                projectId={selectedProject}
                selectedTagIds={selectedTagIds}
                onClearSelectedTags={() => setSelectedTagIds(new Set())}
                recordsFromTagSearch={recordsFromTagSearch}
                isSearchingByTags={isSearchingByTags}
                onClearSearch={handleClearSearch}
                onRefreshSearch={handleRefreshSearch}
              />
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
              <AttachTagsRecordsList
                projectId={selectedProject}
                selectedTagIds={selectedTagIds}
                onClearSelectedTags={() => setSelectedTagIds(new Set())}
              />
            </div>
          )}
          {selectedMenuItem === "Edit Tags" && (
            <div>
              <EditTagsNameFields
                selectedTags={selectedTags}
                onUpdateTag={handleUpdateTag}
                onDeleteTag={handleDeleteTag}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TagManagementClient;
