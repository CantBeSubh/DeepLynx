import React from "react";
import {
  ShieldCheckIcon,
  TagIcon,
  LockClosedIcon,
  LockOpenIcon,
  InformationCircleIcon,
} from "@heroicons/react/24/outline";

interface Props {
  labelCount: number;
  projectsWithLabels: number;
  tagCount: number;
  projectsWithTags: number;
  orgTagsLocked: boolean;
  orgSecurityLabelsLocked: boolean;
}

const ProjectTagOverviewStrip: React.FC<Props> = ({
  labelCount,
  projectsWithLabels,
  tagCount,
  projectsWithTags,
  orgTagsLocked,
  orgSecurityLabelsLocked,
}) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mb-6">
      {/* Project Security Labels */}
      <div className="stat bg-base-100 border border-base-300 rounded-xl">
        <div className="stat-title flex items-center gap-1 text-xs">
          <ShieldCheckIcon className="w-4 h-4 text-base-content/70" />
          Project Security Labels
        </div>
        <div className="stat-value text-base-content text-xl">{labelCount}</div>
        <div className="stat-desc text-xs flex items-center gap-1 text-base-content/70">
          {orgSecurityLabelsLocked ? (
            <>
              <LockClosedIcon className="w-4 h-4 text-error" />
              <span>Locked by organization – no new labels</span>
            </>
          ) : (
            <>
              <LockOpenIcon className="w-4 h-4 text-success" />
              <span>Project may define labels</span>
            </>
          )}
        </div>
      </div>

      {/* Project usage of labels */}
      <div className="stat bg-base-100 border border-base-300 rounded-xl">
        <div className="stat-title flex items-center gap-1 text-xs">
          <ShieldCheckIcon className="w-4 h-4 text-base-content/50" />
          Label Usage
        </div>
        <div className="stat-value text-base-content text-xl">
          {projectsWithLabels}
        </div>
        <div className="stat-desc text-xs text-base-content/70">
          Projects using labels (this project: {labelCount > 0 ? "Yes" : "No"})
        </div>
      </div>

      {/* Project Tags */}
      <div className="stat bg-base-100 border border-base-300 rounded-xl">
        <div className="stat-title flex items-center gap-1 text-xs">
          <TagIcon className="w-4 h-4 text-primary" />
          Project Tags
        </div>
        <div className="stat-value text-primary text-xl">{tagCount}</div>
        <div className="stat-desc text-xs flex items-center gap-1">
          {orgTagsLocked ? (
            <>
              <LockClosedIcon className="w-4 h-4 text-error" />
              <span>Locked by organization – no extra tags</span>
            </>
          ) : (
            <>
              <LockOpenIcon className="w-4 h-4 text-success" />
              <span>May extend org tag set</span>
            </>
          )}
        </div>
      </div>

      {/* Projects with Tags (for this project context this is mostly 0/1 summary) */}
      <div className="stat bg-base-100 border border-base-300 rounded-xl">
        <div className="stat-title flex items-center gap-1 text-xs">
          <TagIcon className="w-4 h-4 text-secondary" />
          Project Tag Usage
        </div>
        <div className="stat-value text-secondary text-xl">
          {projectsWithTags}
        </div>
        <div className="stat-desc text-xs text-base-content/70 flex items-center gap-1">
          <InformationCircleIcon className="w-4 h-4" />
          <span>
            This project always inherits organization tags; counts here are
            project-defined tags only.
          </span>
        </div>
      </div>
    </div>
  );
};

export default ProjectTagOverviewStrip;
