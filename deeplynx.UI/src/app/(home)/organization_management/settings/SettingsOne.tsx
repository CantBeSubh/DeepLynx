// src/app/(home)/organization_management/settings/OrganizationSettings.tsx
"use client";

import { useState, useEffect } from "react";
import toast from "react-hot-toast";
import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type AuthMode = "none" | "basic" | "oauth";

/* -------------------------------------------------------------------------- */
/*                           OrganizationSettings                             */
/* -------------------------------------------------------------------------- */

const SettingsOne = () => {
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

  // If your org already has a logo URL in the session, you could hydrate it here.
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
    // In a real implementation, you'd also call an API to clear the logo.
  };

  const handleSaveBranding = async () => {
    try {
      // TODO: Wire to backend endpoint for saving logo + banner
      toast.success("Branding settings saved (mock).");
    } catch (error) {
      console.error("Failed to save branding:", error);
      toast.error("Failed to save branding");
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                          Storage / Insight Settings                      */
  /* ------------------------------------------------------------------------ */

  // Mock storage locations – replace with real data when you wire it up
  const [storageLocation, setStorageLocation] = useState<string>("org-default");
  const [authMode, setAuthMode] = useState<AuthMode>("none");
  const [basicUsername, setBasicUsername] = useState("");
  const [basicPassword, setBasicPassword] = useState("");
  const [oauthClientId, setOauthClientId] = useState("");
  const [oauthClientSecret, setOauthClientSecret] = useState("");
  const [oauthTokenUrl, setOauthTokenUrl] = useState("");
  const [baseEndpointUrl, setBaseEndpointUrl] = useState("");
  const [inferenceEndpointPath, setInferenceEndpointPath] = useState("");
  const [testingConnection, setTestingConnection] = useState(false);
  const [savingSettings, setSavingSettings] = useState(false);

  const handleTestConnection = async () => {
    if (!baseEndpointUrl) {
      toast.error("Please enter a base endpoint URL before testing.");
      return;
    }

    try {
      setTestingConnection(true);
      // TODO: Wire to a backend endpoint that validates the config
      await new Promise((resolve) => setTimeout(resolve, 800));
      toast.success("Connection successful (mock).");
    } catch (error) {
      console.error("Connection test failed:", error);
      toast.error("Failed to connect to Insight services");
    } finally {
      setTestingConnection(false);
    }
  };

  const handleSaveSettings = async () => {
    try {
      setSavingSettings(true);
      // TODO: Wire to backend for storage + insight settings
      toast.success("Organization settings saved (mock).");
    } catch (error) {
      console.error("Failed to save organization settings:", error);
      toast.error("Failed to save settings");
    } finally {
      setSavingSettings(false);
    }
  };

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
              Configure branding, storage, and Insight service settings for this
              organization.
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

              {/* Logo upload / preview */}
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

              {/* Banner text */}
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
          {/*                     Storage & Insight Settings Card                 */}
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

            {/* Insight Services Settings */}
            <div className="card bg-base-100 border border-primary/40 shadow-sm">
              <div className="card-body">
                <h3 className="card-title text-lg">
                  DeepLynx Insight Services
                </h3>
                <p className="text-sm text-base-content/70 mb-4">
                  Configure authentication and endpoints for connected Insight
                  services. All projects in this organization will use these
                  settings.
                </p>

                {/* Auth Mode */}
                <div className="form-control mb-4">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Authentication Mechanism
                    </span>
                  </label>
                  <select
                    className="select select-bordered"
                    value={authMode}
                    onChange={(e) => setAuthMode(e.target.value as AuthMode)}
                  >
                    <option value="none">None</option>
                    <option value="basic">Basic</option>
                    <option value="oauth">OAuth</option>
                  </select>
                  <label className="label">
                    <span className="label-text-alt text-base-content/60">
                      Applies to all Insight requests made on behalf of projects
                      in this organization.
                    </span>
                  </label>
                </div>

                {/* Auth Fields (conditional) */}
                {authMode === "basic" && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                    <div className="form-control">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Username
                        </span>
                      </label>
                      <input
                        type="text"
                        className="input input-bordered"
                        value={basicUsername}
                        onChange={(e) => setBasicUsername(e.target.value)}
                      />
                    </div>
                    <div className="form-control">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Password
                        </span>
                      </label>
                      <input
                        type="password"
                        className="input input-bordered"
                        value={basicPassword}
                        onChange={(e) => setBasicPassword(e.target.value)}
                      />
                    </div>
                  </div>
                )}

                {authMode === "oauth" && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                    <div className="form-control">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Client ID
                        </span>
                      </label>
                      <input
                        type="text"
                        className="input input-bordered"
                        value={oauthClientId}
                        onChange={(e) => setOauthClientId(e.target.value)}
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
                        value={oauthClientSecret}
                        onChange={(e) => setOauthClientSecret(e.target.value)}
                      />
                    </div>
                    <div className="form-control md:col-span-2">
                      <label className="label">
                        <span className="label-text font-semibold">
                          Token URL
                        </span>
                      </label>
                      <input
                        type="text"
                        className="input input-bordered"
                        placeholder="https://auth.example.com/oauth/token"
                        value={oauthTokenUrl}
                        onChange={(e) => setOauthTokenUrl(e.target.value)}
                      />
                    </div>
                  </div>
                )}

                {/* Endpoints */}
                <div className="form-control mb-3">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Base Endpoint URL
                    </span>
                  </label>
                  <input
                    type="text"
                    className="input input-bordered"
                    placeholder="https://insight.example.com/api"
                    value={baseEndpointUrl}
                    onChange={(e) => setBaseEndpointUrl(e.target.value)}
                  />
                </div>

                <div className="form-control mb-4">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Inference Endpoint Path (optional)
                    </span>
                  </label>
                  <input
                    type="text"
                    className="input input-bordered"
                    placeholder="/v1/inference"
                    value={inferenceEndpointPath}
                    onChange={(e) => setInferenceEndpointPath(e.target.value)}
                  />
                  <label className="label">
                    <span className="label-text-alt text-base-content/60">
                      If provided, this path will be appended to the base URL
                      for inference calls.
                    </span>
                  </label>
                </div>

                <div className="card-actions justify-between mt-2">
                  <button
                    type="button"
                    className="btn btn-ghost"
                    onClick={handleTestConnection}
                    disabled={testingConnection}
                  >
                    {testingConnection ? (
                      <span className="loading loading-spinner loading-sm" />
                    ) : null}
                    Test Connection
                  </button>
                  <button
                    type="button"
                    className={`btn btn-primary ${
                      savingSettings ? "btn-disabled" : ""
                    }`}
                    onClick={handleSaveSettings}
                    disabled={savingSettings}
                  >
                    {savingSettings ? (
                      <span className="loading loading-spinner loading-sm" />
                    ) : (
                      "Save Settings"
                    )}
                  </button>
                </div>
              </div>
            </div>
          </div>
          {/* end right column */}
        </div>
      </div>
    </div>
  );
};

export default SettingsOne;
