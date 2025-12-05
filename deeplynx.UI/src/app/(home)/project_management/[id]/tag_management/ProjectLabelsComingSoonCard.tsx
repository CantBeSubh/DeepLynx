import React from "react";
import {
  ShieldCheckIcon,
  InformationCircleIcon,
} from "@heroicons/react/24/outline";

const ProjectLabelsComingSoonCard: React.FC = () => {
  return (
    <div className="card bg-base-100 border border-dashed border-base-300 shadow-sm opacity-60">
      <div className="card-body">
        {/* Header */}
        <div className="flex items-start justify-between gap-4 mb-3">
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <ShieldCheckIcon className="w-5 h-5 text-base-content/60" />
              <h3 className="font-semibold text-base text-base-content/80">
                Project Security Labels
              </h3>
              <span className="badge badge-sm badge-ghost">Coming soon</span>
            </div>

            <p className="text-xs text-base-content/70 mt-1 max-w-md">
              Project-level security labels (e.g., CUI, Export Controlled) will
              be supported in a future release. Projects will inherit any labels
              defined at the organization level and may define additional
              project-specific labels when enabled.
            </p>
          </div>
        </div>

        {/* Info */}
        <div className="flex items-start gap-2 text-xs text-base-content/70 mt-2">
          <InformationCircleIcon className="w-4 h-4" />
          <p>
            Security labels are not yet available at the project level. Only
            tags are currently supported for classification and workflow.
          </p>
        </div>

        {/* Placeholder box */}
        <div className="mt-4 py-4 text-center text-xs text-base-content/60 border border-dashed border-base-300 rounded-lg">
          Project security label management is coming soon.
        </div>
      </div>
    </div>
  );
};

export default ProjectLabelsComingSoonCard;
