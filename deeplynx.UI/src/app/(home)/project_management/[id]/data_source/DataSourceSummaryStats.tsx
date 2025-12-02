// src/app/(home)/project_management/data_source/DataSourceSummaryStats.tsx

import {
  ChartBarIcon,
  CircleStackIcon,
  KeyIcon,
  ShieldCheckIcon,
} from "@heroicons/react/24/outline";
import type {
  DataSourceResponseDto,
  ProjectStatResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { formatRecordCount } from "./DataSourcesClient";

type SummaryProps = {
  loading: boolean;
  stats: ProjectStatResponseDto | null;
  sources: DataSourceResponseDto[];
  userKeys: string[] | null;
  avgHealth: number;
};

const DataSourceSummaryStats = ({
  loading,
  stats,
  sources,
  userKeys,
  avgHealth,
}: SummaryProps) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
      <div className="stat bg-gradient-to-br from-primary to-primary/70 text-primary-content rounded-lg shadow-lg">
        <div className="stat-figure">
          <CircleStackIcon className="w-8 h-8" />
        </div>
        <div className="stat-title text-primary-content/70">Data Sources</div>
        <div className="stat-value">
          {loading ? "…" : stats?.datasources ?? sources.length}
        </div>
        <div className="stat-desc text-primary-content/60">
          {sources.filter((s) => !s.isArchived).length} active
        </div>
      </div>

      <div className="stat bg-gradient-to-br from-success to-success/70 text-success-content rounded-lg shadow-lg">
        <div className="stat-figure">
          <ChartBarIcon className="w-8 h-8" />
        </div>
        <div className="stat-title text-success-content/70">Total Records</div>
        <div className="stat-value text-2xl">
          {loading ? "…" : formatRecordCount(stats?.records)}
        </div>
        <div className="stat-desc text-success-content/60">
          Across all sources
        </div>
      </div>

      <div className="stat bg-gradient-to-br from-info to-info/70 text-info-content rounded-lg shadow-lg">
        <div className="stat-figure">
          <KeyIcon className="w-8 h-8" />
        </div>
        <div className="stat-title text-info-content/70">API Keys</div>
        <div className="stat-value text-2xl">
          {loading ? "…" : userKeys?.length ?? 0}
        </div>
        <div className="stat-desc text-info-content/60">For current user</div>
      </div>

      <div className="stat bg-gradient-to-br from-warning to-warning/70 text-warning-content rounded-lg shadow-lg">
        <div className="stat-figure">
          <ShieldCheckIcon className="w-8 h-8" />
        </div>
        <div className="stat-title text-warning-content/70">Avg Health</div>
        <div className="stat-value text-2xl">{avgHealth}%</div>
        <div className="stat-desc text-warning-content/60">System health</div>
      </div>
    </div>
  );
};

export default DataSourceSummaryStats;
