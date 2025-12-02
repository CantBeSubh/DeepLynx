// src/app/(home)/project_management/data_source/DataSource.tsx
"use client";

import type {
  DataSourceResponseDto,
  ProjectStatResponseDto,
} from "@/app/(home)/types/responseDTOs";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  archiveDataSource,
  createDataSource,
  getAllDataSourcesOrg,
  setDefaultDataSource,
} from "@/app/lib/client_service/data_source_services.client";
import { getProjectStats } from "@/app/lib/client_service/projects_services.client";
// import { getRecordCountsByDataSource } from "@/app/lib/client_service/record_services.client";
import {
  ArrowPathIcon,
  ChartBarIcon,
  CheckIcon,
  CircleStackIcon,
  ClipboardDocumentIcon,
  EyeIcon,
  EyeSlashIcon,
  KeyIcon,
  PencilIcon,
  PlusIcon,
  ShieldCheckIcon,
  StarIcon,
  TrashIcon,
} from "@heroicons/react/24/outline";
import { useCallback, useEffect, useMemo, useState } from "react";
import DetailsEditor from "./DetailsEditor";
import { getUserApiKeys } from "@/app/lib/client_service/token_services.client";
import { CreateDataSourceRequestDto } from "@/app/(home)/types/requestDTOs";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

type Props = {
  projectId: number;
};

// Define API Key type
interface APIKeyData {
  key: string;
  created?: string;
  expires?: string;
  expiresIn?: string;
  lastUsed?: string;
  requests?: string | number;
  permissions?: string[];
}

// Extend DataSourceResponseDto with optional fields
interface ExtendedDataSource extends DataSourceResponseDto {
  icon?: string;
  health?: number;
  lastSync?: string;
  apiKey?: APIKeyData;
}

const DataSources = ({ projectId }: Props) => {
  const { organization } = useOrganizationSession();
  const [sources, setSources] = useState<DataSourceResponseDto[]>([]);
  const [defaultSourceId, setDefaultSourceId] = useState<number | null>(null);
  const [hideArchived, setHideArchived] = useState(true);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<ProjectStatResponseDto | null>(null);
  const [userKeys, setUserKeys] = useState<string[] | null>(null);

  // We'll add this back later when record counts are implemented
  // const [counts, setCounts] = useState<Record<number, number>>({});

  const [showKey, setShowKey] = useState<Record<string, boolean>>({});
  const [copiedKey, setCopiedKey] = useState<string | null>(null);
  const [expandedSource, setExpandedSource] = useState<number | null>(null);
  const [showCreate, setShowCreate] = useState(false);

  const [createForm, setCreateForm] = useState<CreateDataSourceRequestDto>({
    name: "",
    description: "",
    abbreviation: "",
    type: "",
    baseUri: "",
    config: {},
    default: false,
  });

  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [dataSourceList, projectStats, keys] = await Promise.all([
        getAllDataSourcesOrg(
          organization?.organizationId as number,
          [projectId],
          hideArchived
        ),
        getProjectStats(
          organization?.organizationId as number,
          projectId
        ).catch((err) => {
          console.warn("⚠️ getProjectStats failed, defaulting to null:", err);
          return null;
        }),
        getUserApiKeys().catch((err) => {
          console.warn("⚠️ getUserApiKeys failed, defaulting to []:", err);
          return [] as string[];
        }),
      ]);

      setStats(projectStats ?? null);

      setSources(dataSourceList ?? []);
      setStats(projectStats);
      setUserKeys(keys ?? null);

      const defaultFromList = (dataSourceList ?? []).find(
        (ds) => ds.default === true
      );

      setDefaultSourceId(
        defaultFromList && typeof defaultFromList.id === "number"
          ? defaultFromList.id
          : null
      );
    } catch (e) {
      const errorMessage =
        e instanceof Error ? e.message : "Failed to load data sources.";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [projectId, hideArchived, organization?.organizationId]);

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  const handleCopyKey = (keyId: string, key: string) => {
    navigator.clipboard.writeText(key);
    setCopiedKey(keyId);
    setTimeout(() => setCopiedKey(null), 2000);
  };

  const toggleKeyVisibility = (keyId: string) => {
    setShowKey((prev) => ({ ...prev, [keyId]: !prev[keyId] }));
  };

  const maskKey = (key: string) =>
    key.substring(0, 15) + "•".repeat(25) + key.substring(key.length - 6);

  const toggleExpand = (sourceId: number) => {
    setExpandedSource(expandedSource === sourceId ? null : sourceId);
  };

  const onArchiveToggle = async (s: DataSourceResponseDto) => {
    setSaving(true);
    try {
      await archiveDataSource(projectId, Number(s.id), !s.isArchived);
      await fetchAll();
    } catch (e) {
      const errorMessage = e instanceof Error ? e.message : "Operation failed.";
      setError(errorMessage);
    } finally {
      setSaving(false);
    }
  };

  const onSetDefault = async (id: number) => {
    setSaving(true);
    try {
      await setDefaultDataSource(projectId, id);
      await fetchAll();
    } catch (e) {
      const errorMessage =
        e instanceof Error ? e.message : "Could not set default.";
      setError(errorMessage);
    } finally {
      setSaving(false);
    }
  };

  const onCreate = async () => {
    if (!createForm.name?.trim()) {
      setError("Name is required.");
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await createDataSource(projectId, createForm);
      setShowCreate(false);
      setCreateForm({
        name: "",
        description: "",
        abbreviation: "",
        type: "",
        baseUri: "",
        config: {},
        default: false,
      });
      await fetchAll();
    } catch (e) {
      const errorMessage = e instanceof Error ? e.message : "Create failed.";
      setError(errorMessage);
    } finally {
      setSaving(false);
    }
  };

  const sortedSources = useMemo(() => {
    const defId = defaultSourceId ?? -1;
    return [...sources].sort((a, b) => {
      const aIsDef = Number(a.id) === defId;
      const bIsDef = Number(b.id) === defId;
      if (aIsDef !== bIsDef) return aIsDef ? -1 : 1;
      if (a.isArchived !== b.isArchived) return a.isArchived ? 1 : -1;
      return (a.name ?? "").localeCompare(b.name ?? "");
    });
  }, [sources, defaultSourceId]);

  const avgHealth = useMemo(() => {
    const extendedSources = sources as ExtendedDataSource[];
    const vals = extendedSources
      .map((s) => Number(s.health))
      .filter((n) => !Number.isNaN(n));
    if (!vals.length) return 100;
    return Math.round(vals.reduce((a, b) => a + b, 0) / vals.length);
  }, [sources]);

  const formatRecordCount = (n: number | null | undefined) => {
    if (n == null || Number.isNaN(Number(n))) return "—";
    const value = Number(n);
    return value < 2000
      ? value.toLocaleString()
      : `${(value / 1000).toFixed(1)}K`;
  };

  return (
    <div className="p-6 mx-auto">
      {/* Header */}
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold mb-2">Data Sources</h2>
          <p className="text-base-content/70">
            Manage catalog data sources for this project
          </p>
        </div>
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            className="toggle toggle-primary"
            checked={hideArchived}
            onChange={() => setHideArchived((s) => !s)}
          />
          <span className="text-sm">Hide archived</span>
        </label>
      </div>

      {/* Alerts */}
      {error && (
        <div className="alert alert-error mb-4">
          <span>{error}</span>
        </div>
      )}

      {/* Summary Stats */}
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
          <div className="stat-title text-success-content/70">
            Total Records
          </div>
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

      {/* Quick Actions */}
      <div className="flex justify-end gap-2 mb-6">
        <button
          className="btn btn-ghost gap-2"
          onClick={fetchAll}
          disabled={loading || saving}
        >
          <ArrowPathIcon
            className={`w-5 h-5 ${loading ? "animate-spin" : ""}`}
          />
          Refresh
        </button>
        <button
          className="btn btn-primary gap-2"
          onClick={() => setShowCreate((s) => !s)}
          disabled={saving}
        >
          <PlusIcon className="w-5 h-5" />
          Add Data Source
        </button>
      </div>

      {/* Create Form */}
      {showCreate && (
        <div className="card bg-base-200/50 border-2 border-dashed border-primary/30 mb-6">
          <div className="card-body">
            <h3 className="text-lg font-semibold">Create Data Source</h3>
            <div className="grid md:grid-cols-2 gap-3">
              <input
                className="input input-bordered"
                placeholder="Name *"
                value={createForm.name}
                onChange={(e) =>
                  setCreateForm((f) => ({ ...f, name: e.target.value }))
                }
              />
              <input
                className="input input-bordered"
                placeholder="Abbreviation"
                value={createForm.abbreviation ?? ""}
                onChange={(e) =>
                  setCreateForm((f) => ({ ...f, abbreviation: e.target.value }))
                }
              />
              <input
                className="input input-bordered"
                placeholder="Type (e.g., PostgreSQL, S3)"
                value={createForm.type ?? ""}
                onChange={(e) =>
                  setCreateForm((f) => ({ ...f, type: e.target.value }))
                }
              />
              <input
                className="input input-bordered"
                placeholder="Base URI / Connection"
                value={createForm.baseUri ?? ""}
                onChange={(e) =>
                  setCreateForm((f) => ({ ...f, baseUri: e.target.value }))
                }
              />
              <textarea
                className="textarea textarea-bordered md:col-span-2"
                placeholder="Description"
                value={createForm.description ?? ""}
                onChange={(e) =>
                  setCreateForm((f) => ({ ...f, description: e.target.value }))
                }
              />
            </div>
            <div className="flex justify-end gap-2 mt-3">
              <button
                className="btn btn-ghost"
                onClick={() => setShowCreate(false)}
                disabled={saving}
              >
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={onCreate}
                disabled={saving}
              >
                {saving ? (
                  <span className="loading loading-spinner" />
                ) : (
                  "Create"
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Data Sources List */}
      <div className="space-y-4">
        {loading ? (
          <div className="flex items-center gap-3">
            <span className="loading loading-spinner loading-md" />
            <span>Loading data sources…</span>
          </div>
        ) : sources.length === 0 ? (
          <div className="card bg-base-200/50 border-2 border-dashed">
            <div className="card-body items-center justify-center py-12">
              <PlusIcon className="w-16 h-16 text-base-content/30" />
              <h3 className="text-lg font-semibold text-base-content/60">
                No data sources yet
              </h3>
              <p className="text-sm text-base-content/40">
                Create your first data source to get started.
              </p>
            </div>
          </div>
        ) : (
          sortedSources.map((source) => {
            const extSource = source as ExtendedDataSource;
            const hasId = source.id != null;
            const idNum = typeof source.id === "number" ? source.id : undefined;
            const isDefault =
              hasId && defaultSourceId != null && idNum === defaultSourceId;
            const apiKey = extSource.apiKey;

            const health = Number(extSource.health);
            // const recordCount = idNum !== undefined ? counts[idNum] ?? 0 : 0;
            const lastSync = extSource.lastSync;

            return (
              <div
                key={source.id ?? `ds-${source.name}`}
                className={`card bg-base-100 shadow-xl border-l-4 ${source.isArchived ? "border-l-warning" : "border-l-success"
                  }`}
              >
                <div className="card-body">
                  {/* Header row */}
                  <div className="flex justify-between items-start mb-4">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <span className="text-3xl">
                          {extSource.icon ?? "🗄️"}
                        </span>
                        <div>
                          <div className="flex items-center gap-2">
                            <h3 className="text-xl font-bold">{source.name}</h3>
                            {isDefault && (
                              <div
                                className="tooltip"
                                data-tip="Default data source"
                              >
                                <StarIconSolid className="size-6 text-warning" />
                              </div>
                            )}
                            {source.isArchived && (
                              <div className="badge badge-warning">
                                Archived
                              </div>
                            )}
                          </div>
                          <div className="flex gap-2 items-center mt-1">
                            {source.type && (
                              <div className="badge badge-ghost">
                                {source.type}
                              </div>
                            )}
                            {source.baseuri && (
                              <div className="badge badge-ghost">URI</div>
                            )}
                          </div>
                        </div>
                      </div>

                      {/* Connection String */}
                      {source.baseuri && (
                        <div className="bg-base-200 rounded-lg p-3 mb-3">
                          <div className="text-xs text-base-content/60 mb-1">
                            Connection
                          </div>
                          <code className="text-xs">{source.baseuri}</code>
                        </div>
                      )}

                      {/* Stats Row */}
                      <div className="grid grid-cols-1 sm:grid-cols-4 gap-3 mb-3">
                        <div className="bg-base-200 rounded-lg p-2 text-center">
                          <div className="text-xs text-base-content/60">
                            Records
                          </div>
                          <div className="text-lg font-bold text-primary">
                            {/* We'll wire this up when per-source counts are ready */}
                            {/* {recordCount ? recordCount.toLocaleString() : "—"} */}
                            —
                          </div>
                        </div>
                        <div className="bg-base-200 rounded-lg p-2 text-center">
                          <div className="text-xs text-base-content/60">
                            Health
                          </div>
                          <div
                            className={`text-lg font-bold ${Number.isFinite(health)
                                ? health > 80
                                  ? "text-success"
                                  : health > 50
                                    ? "text-warning"
                                    : "text-error"
                                : ""
                              }`}
                          >
                            {Number.isFinite(health) ? `${health}%` : "—"}
                          </div>
                        </div>
                        <div className="bg-base-200 rounded-lg p-2 text-center">
                          <div className="text-xs text-base-content/60">
                            Last Sync
                          </div>
                          <div className="text-sm font-semibold">
                            {lastSync ?? "—"}
                          </div>
                        </div>
                        <div className="bg-base-200 rounded-lg p-2 text-center">
                          <div className="text-xs text-base-content/60">
                            Last Updated
                          </div>
                          <div className="text-sm font-semibold">
                            {source.lastUpdatedAt
                              ? new Date(source.lastUpdatedAt).toLocaleString()
                              : "—"}
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex flex-col gap-2 ml-2">
                      <button
                        className="btn btn-ghost btn-sm gap-2"
                        onClick={() => hasId && toggleExpand(idNum!)}
                        disabled={!hasId}
                      >
                        <PencilIcon className="w-4 h-4" />
                        Details
                      </button>
                      {!isDefault && !source.isArchived && (
                        <button
                          className="btn btn-ghost btn-sm gap-2"
                          onClick={() => hasId && onSetDefault(idNum!)}
                          disabled={saving || !hasId}
                          title="Set as default"
                        >
                          <StarIcon className="w-4 h-4" />
                          Default
                        </button>
                      )}
                      <button
                        className="btn btn-ghost btn-sm gap-2"
                        onClick={() => hasId && onArchiveToggle(source)}
                        disabled={saving || !hasId}
                      >
                        {source.isArchived ? (
                          <>
                            <ArrowPathIcon className="w-4 h-4" />
                            Unarchive
                          </>
                        ) : (
                          <>
                            <TrashIcon className="w-4 h-4" />
                            Archive
                          </>
                        )}
                      </button>
                    </div>
                  </div>

                  {/* Expanded Details Editor */}
                  {expandedSource === idNum && (
                    <DetailsEditor
                      projectId={projectId}
                      source={source}
                      onClose={() => setExpandedSource(null)}
                      onSaved={fetchAll}
                      setError={setError}
                    />
                  )}

                  {/* API Key Section */}
                  {apiKey?.key && (
                    <>
                      <div className="divider my-2">API Integration Key</div>
                      <div className="bg-base-200/50 rounded-lg p-4">
                        <div className="grid grid-cols-2 gap-3 mb-3 text-sm">
                          {apiKey.created && (
                            <div>
                              <span className="text-base-content/60">
                                Created:
                              </span>
                              <span className="ml-2 font-semibold">
                                {apiKey.created}
                              </span>
                            </div>
                          )}
                          {apiKey.expiresIn && (
                            <div>
                              <span className="text-base-content/60">
                                Expires:
                              </span>
                              <span
                                className={`ml-2 font-semibold ${apiKey.expiresIn === "Expired"
                                    ? "text-error"
                                    : ""
                                  }`}
                              >
                                {apiKey.expiresIn}
                              </span>
                            </div>
                          )}
                          {apiKey.lastUsed && (
                            <div>
                              <span className="text-base-content/60">
                                Last Used:
                              </span>
                              <span className="ml-2 font-semibold">
                                {apiKey.lastUsed}
                              </span>
                            </div>
                          )}
                          {apiKey.permissions &&
                            apiKey.permissions.length > 0 && (
                              <div>
                                <span className="text-base-content/60">
                                  Permissions:
                                </span>
                                <div className="inline-flex gap-1 ml-2">
                                  {apiKey.permissions.map((perm) => (
                                    <div
                                      key={perm}
                                      className="badge badge-primary badge-xs"
                                    >
                                      {perm}
                                    </div>
                                  ))}
                                </div>
                              </div>
                            )}
                        </div>

                        <div className="form-control">
                          <label className="label py-1">
                            <span className="label-text font-semibold text-xs">
                              API Key
                            </span>
                          </label>
                          <div className="flex gap-2">
                            <div className="flex-1 bg-base-300 rounded-lg p-3 font-mono text-xs overflow-x-auto">
                              {showKey[String(source.id)]
                                ? apiKey.key
                                : maskKey(apiKey.key)}
                            </div>
                            <button
                              className="btn btn-sm btn-ghost btn-square"
                              onClick={() =>
                                toggleKeyVisibility(String(source.id))
                              }
                              title={
                                showKey[String(source.id)]
                                  ? "Hide key"
                                  : "Show key"
                              }
                            >
                              {showKey[String(source.id)] ? (
                                <EyeSlashIcon className="w-5 h-5" />
                              ) : (
                                <EyeIcon className="w-5 h-5" />
                              )}
                            </button>
                            <button
                              className="btn btn-sm btn-primary gap-1"
                              onClick={() =>
                                handleCopyKey(String(source.id), apiKey.key)
                              }
                            >
                              {copiedKey === String(source.id) ? (
                                <>
                                  <CheckIcon className="w-4 h-4" />
                                  Copied
                                </>
                              ) : (
                                <>
                                  <ClipboardDocumentIcon className="w-4 h-4" />
                                  Copy
                                </>
                              )}
                            </button>
                          </div>
                          <label className="label py-1">
                            <span className="label-text-alt text-warning text-xs">
                              🔐 Keep this key secure - it provides access to
                              this data source
                            </span>
                          </label>
                        </div>
                      </div>
                    </>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Info Section */}
      <div className="alert alert-info mt-6">
        <KeyIcon className="w-5 h-5" />
        <div>
          <h3 className="font-bold">About API Keys</h3>
          <p className="text-sm">
            Each data source can have API keys for integrations with tools like
            Airflow, ETL pipelines, and automated workflows.
          </p>
        </div>
      </div>
    </div>
  );
};

export default DataSources;
