// src/app/(home)/organization_management/settings/OrganizationSettingsGridApproach.tsx
"use client";

import { useState, useEffect } from "react";
import toast from "react-hot-toast";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";
import {
  ChartBarIcon,
  Squares2X2Icon,
  EyeIcon,
  CheckIcon,
  ArrowTopRightOnSquareIcon,
  Cog6ToothIcon,
} from "@heroicons/react/24/outline";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type AuthMode = "none" | "basic" | "oauth";
type ServiceId = "insight" | "lattice" | "visualize";

interface Service {
  id: ServiceId;
  name: string;
  icon: JSX.Element;
  description: string;
  status: "connected" | "disconnected";
  authMode: AuthMode;
  baseUrl: string;
  inferenceEndpointPath: string;
  basicUsername: string;
  basicPassword: string;
  oauthClientId: string;
  oauthClientSecret: string;
  oauthTokenUrl: string;
  lastSync: string | null;
}

/* -------------------------------------------------------------------------- */
/*                           OrganizationSettings                             */
/* -------------------------------------------------------------------------- */

const OrganizationSettingsGridApproach = () => {
  /* ------------------------------------------------------------------------ */
  /*                           Organization Context                           */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  /* ------------------------------------------------------------------------ */
  /*                             Branding / Banner                            */
  /* ------------------------------------------------------------------------ */

  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [bannerText, setBannerText] = useState(
    "This organization space may contain sensitive data that must be protected accordingly."
  );

  useEffect(() => {
    if (organization?.logoUrl) {
      setLogoPreview(organization.logoUrl);
    }
  }, [organization?.logoUrl]);

  const handleLogoChange = (fileList: FileList | null) => {
    if (!fileList || fileList.length === 0) return;

    const file = fileList[0];
    if (!file.type.startsWith("image/")) {
      toast.error("Please upload a valid image file (PNG recommended).");
      return;
    }

    setLogoFile(file);
    const previewUrl = URL.createObjectURL(file);
    setLogoPreview(previewUrl);
  };

  const handleRemoveLogo = () => {
    setLogoFile(null);
    setLogoPreview(null);
  };

  const handleSaveBranding = async () => {
    try {
      toast.success("Branding settings saved (mock).");
    } catch (error) {
      console.error("Failed to save branding:", error);
      toast.error("Failed to save branding");
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                          Storage Settings                                */
  /* ------------------------------------------------------------------------ */

  const [storageLocation, setStorageLocation] = useState<string>("org-default");

  /* ------------------------------------------------------------------------ */
  /*                       Services State (GRID APPROACH)                     */
  /* ------------------------------------------------------------------------ */

  const [configuringService, setConfiguringService] =
    useState<ServiceId | null>(null);
  const [services, setServices] = useState<Service[]>([
    {
      id: "insight",
      name: "Insight",
      icon: <ChartBarIcon className="w-5 h-5" />,
      description: "Advanced analytics and reporting",
      status: "connected",
      authMode: "oauth",
      baseUrl: "https://insight.deeplynx.io/api",
      inferenceEndpointPath: "/v1/inference",
      basicUsername: "",
      basicPassword: "",
      oauthClientId: "insight-client-123",
      oauthClientSecret: "••••••••",
      oauthTokenUrl: "https://auth.deeplynx.io/oauth/token",
      lastSync: "2 minutes ago",
    },
    {
      id: "lattice",
      name: "Lattice",
      icon: <Squares2X2Icon className="w-5 h-5" />,
      description: "Data mesh and federation",
      status: "connected",
      authMode: "basic",
      baseUrl: "https://lattice.deeplynx.io/api",
      inferenceEndpointPath: "",
      basicUsername: "lattice-user",
      basicPassword: "••••••••",
      oauthClientId: "",
      oauthClientSecret: "",
      oauthTokenUrl: "",
      lastSync: "5 minutes ago",
    },
    {
      id: "visualize",
      name: "Visualize",
      icon: <EyeIcon className="w-5 h-5" />,
      description: "3D visualization and rendering",
      status: "disconnected",
      authMode: "none",
      baseUrl: "",
      inferenceEndpointPath: "",
      basicUsername: "",
      basicPassword: "",
      oauthClientId: "",
      oauthClientSecret: "",
      oauthTokenUrl: "",
      lastSync: null,
    },
  ]);

  const [testingConnection, setTestingConnection] = useState<ServiceId | null>(
    null
  );
  const [savingSettings, setSavingSettings] = useState<ServiceId | null>(null);

  const updateServiceField = <K extends keyof Service>(
    serviceId: ServiceId,
    field: K,
    value: Service[K]
  ) => {
    setServices((prev) =>
      prev.map((s) => (s.id === serviceId ? { ...s, [field]: value } : s))
    );
  };

  const handleTestConnection = async (serviceId: ServiceId) => {
    const service = services.find((s) => s.id === serviceId)!;
    if (!service.baseUrl) {
      toast.error("Please enter a base endpoint URL before testing.");
      return;
    }

    try {
      setTestingConnection(serviceId);
      await new Promise((resolve) => setTimeout(resolve, 800));
      toast.success(`${service.name} connection successful (mock).`);
    } catch (error) {
      console.error("Connection test failed:", error);
      toast.error(`Failed to connect to ${service.name} service`);
    } finally {
      setTestingConnection(null);
    }
  };

  const handleSaveServiceSettings = async (serviceId: ServiceId) => {
    const service = services.find((s) => s.id === serviceId)!;
    try {
      setSavingSettings(serviceId);
      toast.success(`${service.name} settings saved (mock).`);
      setConfiguringService(null); // Close modal
    } catch (error) {
      console.error("Failed to save service settings:", error);
      toast.error("Failed to save settings");
    } finally {
      setSavingSettings(null);
    }
  };

  const configuringServiceData = configuringService
    ? services.find((s) => s.id === configuringService)
    : null;

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      <div className="max-w-6xl mx-auto">
        {/* Page Header */}
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-2xl font-bold text-base-content">
              Organization Settings
            </h2>
            <p className="text-base-content/70 text-sm mt-1">
              Configure branding, storage, and ecosystem service settings for
              this organization.
            </p>
          </div>
        </div>

        {/* Two-column layout */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* ------------------------------------------------------------------ */}
          {/*                          Branding / Banner Card                    */}
          {/* ------------------------------------------------------------------ */}
          <div className="card bg-base-100 border border-primary/40 shadow-sm">
            <div className="card-body">
              <div className="flex items-center justify-between mb-4">
                <h3 className="card-title text-lg">Branding & Banner</h3>
              </div>

              <div className="flex items-center gap-4 mb-6">
                <div className="avatar">
                  <div className="w-16 h-16 rounded-xl bg-base-200 flex items-center justify-center overflow-hidden">
                    {logoPreview ? (
                      // eslint-disable-next-line @next/next/no-img-element
                      <img
                        src={logoPreview}
                        alt="Organization Logo"
                        className="object-cover w-full h-full"
                      />
                    ) : (
                      <span className="text-base-content/40 text-sm">Logo</span>
                    )}
                  </div>
                </div>
                <div className="flex flex-col gap-2">
                  <span className="font-semibold">
                    {organization?.organizationName || "Organization"}
                  </span>
                  <div className="flex gap-2">
                    <label className="btn btn-sm btn-primary">
                      Upload PNG
                      <input
                        type="file"
                        accept="image/png,image/jpeg,image/svg+xml"
                        className="hidden"
                        onChange={(e) => handleLogoChange(e.target.files)}
                      />
                    </label>
                    {logoPreview && (
                      <button
                        type="button"
                        className="btn btn-sm btn-ghost"
                        onClick={handleRemoveLogo}
                      >
                        Remove
                      </button>
                    )}
                  </div>
                  <p className="text-xs text-base-content/60">
                    Appears next to the organization name in the top header.
                    Recommended: PNG, 256×256, transparent background.
                  </p>
                </div>
              </div>

              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Organization Warning Banner
                  </span>
                </label>
                <textarea
                  className="textarea textarea-bordered min-h-24"
                  placeholder='e.g. "This organization space contains CUI/ECI data that must be protected accordingly."'
                  value={bannerText}
                  onChange={(e) => setBannerText(e.target.value)}
                />
                <label className="label">
                  <span className="label-text-alt text-base-content/60">
                    Displayed beneath the top header for all pages in this
                    organization.
                  </span>
                  <span className="label-text-alt text-base-content/40">
                    {bannerText.length} / 240
                  </span>
                </label>
              </div>

              <div className="card-actions justify-end mt-4">
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={handleSaveBranding}
                >
                  Save Branding
                </button>
              </div>
            </div>
          </div>

          {/* ------------------------------------------------------------------ */}
          {/*                     Storage & Services Settings Card               */}
          {/* ------------------------------------------------------------------ */}
          <div className="flex flex-col gap-4">
            {/* Storage Settings */}
            <div className="card bg-base-100 border border-primary/40 shadow-sm">
              <div className="card-body">
                <h3 className="card-title text-lg">Storage Settings</h3>
                <p className="text-sm text-base-content/70 mb-4">
                  Set the default unmounted object storage location for this
                  organization. This will appear as the organization default in
                  project administration.
                </p>

                <div className="form-control mb-4">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Default Unmounted Object Storage
                    </span>
                  </label>
                  <select
                    className="select select-bordered"
                    value={storageLocation}
                    onChange={(e) => setStorageLocation(e.target.value)}
                  >
                    <option value="org-default">Organization Default</option>
                    <option value="s3-west">S3 – us-west-2</option>
                    <option value="s3-east">S3 – us-east-1</option>
                    <option value="local-cluster">Local Cluster Storage</option>
                  </select>
                  <label className="label">
                    <span className="label-text-alt text-base-content/60">
                      Used as the default when creating new data sources for
                      projects in this organization.
                    </span>
                  </label>
                </div>
              </div>
            </div>

            {/* ================================================================ */}
            {/*            GRID APPROACH - DeepLynx Ecosystem Services           */}
            {/* ================================================================ */}
            <div className="card bg-base-100 border border-primary/40 shadow-sm">
              <div className="card-body">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="card-title text-lg">
                    DeepLynx Ecosystem Services
                  </h3>
                </div>
                <p className="text-sm text-base-content/70 mb-4">
                  Configure and manage connected services
                </p>

                {/* Grid of Service Cards */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {services.map((service) => (
                    <div
                      key={service.id}
                      className={`card border-2 shadow-sm transition-all hover:shadow-md ${
                        service.status === "connected"
                          ? "border-success/30 bg-success/5"
                          : "border-base-300 bg-base-100"
                      }`}
                    >
                      <div className="card-body p-4">
                        {/* Card Header */}
                        <div className="flex items-start justify-between mb-3">
                          <div className="p-2 bg-base-200 rounded-lg border border-base-300">
                            {service.icon}
                          </div>
                          <div
                            className={`badge badge-sm ${
                              service.status === "connected"
                                ? "badge-success"
                                : "badge-ghost"
                            }`}
                          >
                            {service.status === "connected" ? (
                              <span className="flex items-center gap-1">
                                <CheckIcon className="w-3 h-3" />
                                Active
                              </span>
                            ) : (
                              "Inactive"
                            )}
                          </div>
                        </div>

                        {/* Card Content */}
                        <h4 className="font-bold text-base-content mb-1">
                          {service.name}
                        </h4>
                        <p className="text-sm text-base-content/70 mb-3 min-h-[40px]">
                          {service.description}
                        </p>

                        {service.status === "connected" ? (
                          <>
                            <div className="space-y-2 mb-3">
                              <div className="flex items-center justify-between text-xs">
                                <span className="text-base-content/60">
                                  Auth:
                                </span>
                                <span className="font-medium text-base-content uppercase">
                                  {service.authMode}
                                </span>
                              </div>
                              <div className="flex items-center justify-between text-xs">
                                <span className="text-base-content/60">
                                  Last sync:
                                </span>
                                <span className="font-medium text-base-content">
                                  {service.lastSync}
                                </span>
                              </div>
                            </div>
                            <div className="flex gap-2">
                              <button
                                type="button"
                                className="flex-1 btn btn-sm btn-ghost"
                                onClick={() =>
                                  setConfiguringService(service.id)
                                }
                              >
                                <Cog6ToothIcon className="w-4 h-4" />
                                Configure
                              </button>
                              <button
                                type="button"
                                className="btn btn-sm btn-ghost btn-square"
                              >
                                <ArrowTopRightOnSquareIcon className="w-4 h-4" />
                              </button>
                            </div>
                          </>
                        ) : (
                          <button
                            type="button"
                            className="btn btn-sm btn-primary w-full"
                            onClick={() => {
                              updateServiceField(
                                service.id,
                                "status",
                                "connected"
                              );
                              toast.success(`${service.name} connected (mock)`);
                            }}
                          >
                            Connect Service
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* ================================================================ */}
      {/*                     Configuration Modal                          */}
      {/* ================================================================ */}
      {configuringServiceData && (
        <div className="modal modal-open">
          <div className="modal-box max-w-2xl">
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-3">
                {configuringServiceData.icon}
                <h3 className="font-bold text-xl">
                  Configure {configuringServiceData.name}
                </h3>
              </div>
              <button
                type="button"
                className="btn btn-sm btn-circle btn-ghost"
                onClick={() => setConfiguringService(null)}
              >
                ✕
              </button>
            </div>

            <div className="space-y-4">
              {/* Auth Mode */}
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Authentication Mechanism
                  </span>
                </label>
                <select
                  className="select select-bordered"
                  value={configuringServiceData.authMode}
                  onChange={(e) =>
                    updateServiceField(
                      configuringServiceData.id,
                      "authMode",
                      e.target.value as AuthMode
                    )
                  }
                >
                  <option value="none">None</option>
                  <option value="basic">Basic</option>
                  <option value="oauth">OAuth</option>
                </select>
              </div>

              {/* Auth Fields */}
              {configuringServiceData.authMode === "basic" && (
                <div className="grid grid-cols-2 gap-4">
                  <div className="form-control">
                    <label className="label">
                      <span className="label-text font-semibold">Username</span>
                    </label>
                    <input
                      type="text"
                      className="input input-bordered"
                      value={configuringServiceData.basicUsername}
                      onChange={(e) =>
                        updateServiceField(
                          configuringServiceData.id,
                          "basicUsername",
                          e.target.value
                        )
                      }
                    />
                  </div>
                  <div className="form-control">
                    <label className="label">
                      <span className="label-text font-semibold">Password</span>
                    </label>
                    <input
                      type="password"
                      className="input input-bordered"
                      value={configuringServiceData.basicPassword}
                      onChange={(e) =>
                        updateServiceField(
                          configuringServiceData.id,
                          "basicPassword",
                          e.target.value
                        )
                      }
                    />
                  </div>
                </div>
              )}

              {configuringServiceData.authMode === "oauth" && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="form-control">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Client ID
                        </span>
                      </label>
                      <input
                        type="text"
                        className="input input-bordered"
                        value={configuringServiceData.oauthClientId}
                        onChange={(e) =>
                          updateServiceField(
                            configuringServiceData.id,
                            "oauthClientId",
                            e.target.value
                          )
                        }
                      />
                    </div>
                    <div className="form-control">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Client Secret
                        </span>
                      </label>
                      <input
                        type="password"
                        className="input input-bordered"
                        value={configuringServiceData.oauthClientSecret}
                        onChange={(e) =>
                          updateServiceField(
                            configuringServiceData.id,
                            "oauthClientSecret",
                            e.target.value
                          )
                        }
                      />
                    </div>
                  </div>
                  <div className="form-control">
                    <label className="label">
                      <span className="label-text font-semibold">
                        Token URL
                      </span>
                    </label>
                    <input
                      type="text"
                      className="input input-bordered"
                      placeholder="https://auth.example.com/oauth/token"
                      value={configuringServiceData.oauthTokenUrl}
                      onChange={(e) =>
                        updateServiceField(
                          configuringServiceData.id,
                          "oauthTokenUrl",
                          e.target.value
                        )
                      }
                    />
                  </div>
                </div>
              )}

              {/* Endpoints */}
              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Base Endpoint URL
                  </span>
                </label>
                <input
                  type="text"
                  className="input input-bordered"
                  placeholder="https://service.example.com/api"
                  value={configuringServiceData.baseUrl}
                  onChange={(e) =>
                    updateServiceField(
                      configuringServiceData.id,
                      "baseUrl",
                      e.target.value
                    )
                  }
                />
              </div>

              <div className="form-control">
                <label className="label">
                  <span className="label-text font-semibold">
                    Inference Endpoint Path (optional)
                  </span>
                </label>
                <input
                  type="text"
                  className="input input-bordered"
                  placeholder="/v1/inference"
                  value={configuringServiceData.inferenceEndpointPath}
                  onChange={(e) =>
                    updateServiceField(
                      configuringServiceData.id,
                      "inferenceEndpointPath",
                      e.target.value
                    )
                  }
                />
              </div>
            </div>

            <div className="modal-action">
              <button
                type="button"
                className="btn btn-ghost"
                onClick={handleTestConnection.bind(
                  null,
                  configuringServiceData.id
                )}
                disabled={testingConnection === configuringServiceData.id}
              >
                {testingConnection === configuringServiceData.id && (
                  <span className="loading loading-spinner loading-sm" />
                )}
                Test Connection
              </button>
              <button
                type="button"
                className="btn btn-primary"
                onClick={() =>
                  handleSaveServiceSettings(configuringServiceData.id)
                }
                disabled={savingSettings === configuringServiceData.id}
              >
                {savingSettings === configuringServiceData.id && (
                  <span className="loading loading-spinner loading-sm" />
                )}
                Save Settings
              </button>
            </div>
          </div>
          <div
            className="modal-backdrop"
            onClick={() => setConfiguringService(null)}
          />
        </div>
      )}
    </div>
  );
};

export default OrganizationSettingsGridApproach;
