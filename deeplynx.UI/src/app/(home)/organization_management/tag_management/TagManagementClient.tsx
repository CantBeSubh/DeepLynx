"use client";

import React, { useCallback, useEffect, useState } from "react";
import {
  getAllTags,
  updateTag,
  deleteTag,
} from "@/app/lib/tag_services.client";
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
import ProjectDropdownSingleSelect from "../../components/ProjectDropdownSingleSelect";
import {
  TagResponseDto,
  ProjectResponseDto,
  RecordResponseDto,
} from "../../types/responseDTOs";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";

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
  initialSelectedProject?: ProjectResponseDto | null;
}

const TagManagementClient = ({
  initialProjects,
  initialSelectedProject,
}: Props) => {
  const [projects] = useState<ProjectResponseDto[]>(initialProjects);
  const [selectedProject, setSelectedProject] = useState<string>(
    initialSelectedProject?.id?.toString() || ""
  );
  const [selectedMenuItem, setSelectedMenuItems] = useState("Search Tags");
  const [tags, setTags] = useState<TagResponseDto[]>([]);
  const [filteredTags, setFilteredTags] = useState<TagResponseDto[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedTagIds, setSelectedTagIds] = useState<Set<number>>(new Set());
  const [recordsFromTagSearch, setRecordsFromTagSearch] = useState<
    RecordResponseDto[]
  >([]);
  const [isSearchingByTags, setIsSearchingByTags] = useState(false);

  const menuItems = ["Search Tags", "Create Tag", "Attach Tags", "Edit Tags"];
  const { organization } = useOrganizationSession();


  useEffect(() => {
    setSelectedTagIds(new Set());
  }, [selectedMenuItem]);

  const refetchTags = useCallback(async () => {
    if (!selectedProject) {
      setTags([]);
      setFilteredTags([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const allTags = await getAllTags(organization?.organizationId as number, Number(selectedProject));
      setTags(allTags);
      setFilteredTags(allTags);
    } catch (error) {
      setError("Failed to fetch tags");
      console.error("Error fetching tags:", error);
    } finally {
      setLoading(false);
    }
  }, [selectedProject]);

  useEffect(() => {
    refetchTags();
  }, [refetchTags]);

  const handleProjectChange = useCallback((newProjectId: string) => {
    setSelectedProject(newProjectId);
  }, []);

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

  const handleSearchByTags = async (tagIds: number[]) => {
    setIsSearchingByTags(true);
    try {
      const records = await getRecordsByTags(organization?.organizationId as number, Number(selectedProject), tagIds);

      if (records.length === 0) {
        toast.error("No records found with these tags");
        setRecordsFromTagSearch([]);
        return;
      }

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
        `Found ${records.length} record${records.length !== 1 ? "s" : ""
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

  const handleUpdateTag = async (tagId: number, newName: string) => {
    try {
      await updateTag(organization?.organizationId as number, Number(selectedProject), tagId, { name: newName });
      await refetchTags();
    } catch (error) {
      console.error("Error updating tag:", error);
      throw error;
    }
  };

  const handleDeleteTag = async (tagId: number) => {
    try {
      await deleteTag(organization?.organizationId as number, Number(selectedProject), tagId);
      const newSelected = new Set(selectedTagIds);
      newSelected.delete(tagId);
      setSelectedTagIds(newSelected);
      await refetchTags();
    } catch (error) {
      console.error("Error deleting tag:", error);
      throw error;
    }
  };

  const selectedTags = tags.filter((tag) => selectedTagIds.has(tag.id));

  return (
    <div>
      {/* Header */}
      <div className="p-6">
        <h1 className="text-2xl font-bold text-base-content">
          Tags & Security Labels
        </h1>
        <p className="text-base-content/70 mt-2">
          Define organization-level tags and security labels for attribute-based
          access controls. These settings will propagate to all projects within
          the organization.
        </p>
        <div className="mt-4">
          <ProjectDropdownSingleSelect
            projects={projects}
            onSelectionChange={handleProjectChange}
            defaultSelectedId={selectedProject}
          />
        </div>
      </div>

      {/* Main Content Grid with Height Constraint */}
      <div
        className="grid grid-cols-[20%_40%_40%] p-6 gap-6 transition-all"
        style={{ height: "calc(100vh - 23rem)" }}
      >
        {/* Left Menu */}
        <div className="card shadow-xl rounded-lg p-6 overflow-y-auto">
          <ul>
            {menuItems.map((item) => (
              <li
                key={item}
                onClick={() => setSelectedMenuItems(item)}
                className={`cursor-pointer px-4 py-2 rounded-lg transition-colors font-bold ${selectedMenuItem === item
                  ? "bg-info/50 text-info-content"
                  : "hover:bg-base-200"
                  }`}
              >
                {item}
              </li>
            ))}
          </ul>
        </div>

        {/* Middle Panel */}
        <div className="card shadow-xl rounded-lg p-6 overflow-y-auto">
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
          )}
          {selectedMenuItem === "Edit Tags" && (
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
          )}
        </div>

        {/* Right Panel */}
        <div className="card shadow-xl rounded-lg p-6 overflow-y-auto">
          {selectedMenuItem === "Search Tags" && (
            <SearchTagsRecordsList
              projectId={selectedProject}
              selectedTagIds={selectedTagIds}
              onClearSelectedTags={() => setSelectedTagIds(new Set())}
              recordsFromTagSearch={recordsFromTagSearch}
              isSearchingByTags={isSearchingByTags}
              onClearSearch={handleClearSearch}
              onRefreshSearch={handleRefreshSearch}
            />
          )}
          {selectedMenuItem === "Create Tag" && (
            <CreateTagRecordsList
              projectId={selectedProject}
              selectedTagIds={selectedTagIds}
            />
          )}
          {selectedMenuItem === "Attach Tags" && (
            <AttachTagsRecordsList
              projectId={selectedProject}
              selectedTagIds={selectedTagIds}
              onClearSelectedTags={() => setSelectedTagIds(new Set())}
            />
          )}
          {selectedMenuItem === "Edit Tags" && (
            <EditTagsNameFields
              selectedTags={selectedTags}
              onUpdateTag={handleUpdateTag}
              onDeleteTag={handleDeleteTag}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default TagManagementClient;
