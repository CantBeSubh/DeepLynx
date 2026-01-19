import React from "react";
import {
  ShieldCheckIcon,
  InformationCircleIcon,
} from "@heroicons/react/24/outline";

const LabelsComingSoonCard: React.FC = () => {
  return (
    <div className="card bg-base-100 border border-dashed border-base-300 shadow-sm opacity-60">
      <div className="card-body">
        <div className="flex items-start justify-between gap-4 mb-3">
          <div className="flex-1">
            <div className="flex items-center gap-2">
              <ShieldCheckIcon className="w-5 h-5 text-base-content/60" />
              <h3 className="font-semibold text-base text-base-content/80">
                Organization Security Labels
              </h3>
              <span className="badge badge-sm badge-ghost">Coming soon</span>
            </div>
            <p className="text-xs text-base-content/70 mt-1 max-w-md">
              Security labels (e.g., CUI) for attribute-based access control
              will appear here in a future release. Projects will inherit labels
              defined at the organization level.
            </p>
          </div>
        </div>

        <div className="flex items-start gap-2 text-xs text-base-content/70 mt-2">
          <InformationCircleIcon className="w-4 h-4" />
          <p>
            You&apos;ll be able to define and lock organization-wide security
            labels here. Until then, only tags are available for project use.
          </p>
        </div>

        <div className="mt-4 py-4 text-center text-xs text-base-content/60 border border-dashed border-base-300 rounded-lg">
          Security label management is not yet enabled for this organization.
        </div>
      </div>
    </div>
  );
};

export default LabelsComingSoonCard;
