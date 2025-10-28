import { createTag } from "@/app/lib/tag_services.client";
import React, { useEffect, useState } from "react";
import toast from "react-hot-toast";

interface CreateTagProps {
  projectId: string;
}

const CreateTag = ({ projectId }: CreateTagProps) => {
  const [tagName, setTagName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleCreateTag = async () => {
    if (!tagName.trim()) {
      setError("Tag name cannot be empty");
      return;
    }

    if (!projectId || projectId === "0") {
      setError("Invalid project selected");
      return;
    }

    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      await createTag(Number(projectId), { name: tagName });
      setSuccess(true);
      setTagName("");
      toast.success("Tag Created");
    } catch (err) {
      setError("Failed to create tag");
      console.error("Error creating tag:", err);
      toast.error("Failed to create tag.");
    } finally {
      setLoading(false);
    }
  };

  const isProjectSelected = projectId && projectId !== "0";

  return (
    <div className="w-[70%] mx-auto">
      <div>
        <h3 className="font-bold mb-4">Name</h3>
        <input
          type="text"
          placeholder="Example: Reactor"
          className="input input-bordered w-full mb-4"
          value={tagName}
          onChange={(e) => setTagName(e.target.value)}
          disabled={loading || !isProjectSelected}
        />
        {!isProjectSelected && (
          <p className="text-warning text-sm mb-2">
            Please select a project first
          </p>
        )}
        <div className="flex justify-end">
          <button
            className="btn btn-primary"
            onClick={handleCreateTag}
            disabled={loading || !tagName.trim() || !isProjectSelected}
          >
            {loading ? "Creating..." : "Create Tag"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateTag;
