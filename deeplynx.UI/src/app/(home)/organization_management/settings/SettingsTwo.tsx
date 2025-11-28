// src/app/(home)/organization_management/settings/OrganizationSettingsOption2.tsx
"use client";

import { useState, useEffect } from "react";
import toast from "react-hot-toast";
import {
  PlusIcon,
  LockClosedIcon,
  LockOpenIcon,
  InformationCircleIcon,
} from "@heroicons/react/24/outline";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type AuthMode = "none" | "basic" | "oauth";

/* -------------------------------------------------------------------------- */
/*                         Organization Settings (Opt 2)                      */
/* -------------------------------------------------------------------------- */

const SettingsTwo = () => {
  /* ------------------------------------------------------------------------ */
  /*                           Organization Context                           */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  /* ------------------------------------------------------------------------ */
  /*                                   Tabs                                   */
  /* ------------------------------------------------------------------------ */

  type TabName = "branding" | "storage" | "insight";

  const [activeTab, setActiveTab] = useState<TabName>("branding");

  /* ------------------------------------------------------------------------ */
  /*                               Branding State                             */
  /* ------------------------------------------------------------------------ */

  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);

  const [bannerText, setBannerText] = useState(
    "This organization space may contain CUI/ECI and must be protected accordingly."
  );

  useEffect(() => {
    if (organization?.logoUrl) {
      setLogoPreview(organization.logoUrl);
    }
  }, [organization?.logoUrl]);

  const handleLogoChange = (fileList: FileList | null) => {
    if (!fileList?.length) return;

    const file = fileList[0];
    if (!file.type.startsWith("image/")) {
      toast.error("Please upload a valid image file.");
      return;
    }

    setLogoFile(file);
    setLogoPreview(URL.createObjectURL(file));
  };

  const handleRemoveLogo = () => {
    setLogoFile(null);
    setLogoPreview(null);
  };

  const handleSaveBranding = async () => {
    try {
      // TODO: Wire into backend
      toast.success("Branding saved (mock)");
    } catch (err) {
      toast.error("Failed to save branding");
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                         Storage / Insight Service State                  */
  /* ------------------------------------------------------------------------ */

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
      toast.error("Base endpoint URL required");
      return;
    }

    setTestingConnection(true);
    await new Promise((res) => setTimeout(res, 700));
    setTestingConnection(false);
    toast.success("Connection successful (mock)");
  };

  const handleSaveSettings = async () => {
    setSavingSettings(true);
    await new Promise((res) => setTimeout(res, 700));
    setSavingSettings(false);
    toast.success("Settings saved (mock)");
  };

  /* ------------------------------------------------------------------------ */
  /*                               Render Tabs                                */
  /* ------------------------------------------------------------------------ */

  const TabButton = ({ label, tab }: { label: string; tab: TabName }) => (
    <button
      className={`tab tab-sm ${
        activeTab === tab ? "tab-active font-semibold" : ""
      }`}
      onClick={() => setActiveTab(tab)}
    >
      {label}
    </button>
  );

  /* ------------------------------------------------------------------------ */
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-6">
          <h2 className="text-2xl font-bold text-base-content">
            Organization Settings
          </h2>
          <p className="text-base-content/70 mt-1">
            Manage branding, storage defaults, and Insight integrations for this
            organization.
          </p>
        </div>

        {/* Main Card */}
        <div className="card bg-base-100 border border-primary/40 shadow-md">
          <div className="card-body">
            {/* Tabs */}
            <div className="tabs tabs-bordered mb-4">
              <TabButton label="Branding" tab="branding" />
              <TabButton label="Storage" tab="storage" />
              <TabButton label="Insight Services" tab="insight" />
            </div>

            {/* TAB CONTENT BELOW */}

            {/* ------------------------------------------------------------------ */}
            {/*                               Branding                            */}
            {/* ------------------------------------------------------------------ */}
            {activeTab === "branding" && (
              <div className="space-y-6">
                {/* Logo */}
                <div>
                  <h3 className="text-lg font-semibold mb-2">
                    Organization Logo
                  </h3>
                  <div className="flex items-center gap-4">
                    <div className="avatar">
                      <div className="w-20 h-20 rounded-xl bg-base-200 overflow-hidden flex items-center justify-center">
                        {logoPreview ? (
                          // eslint-disable-next-line @next/next/no-img-element
                          <img
                            src={logoPreview}
                            alt="Logo preview"
                            className="object-cover w-full h-full"
                          />
                        ) : (
                          <span className="text-base-content/40">Logo</span>
                        )}
                      </div>
                    </div>

                    <div className="flex flex-col gap-2">
                      <label className="btn btn-sm btn-primary">
                        Upload PNG
                        <input
                          type="file"
                          className="hidden"
                          accept="image/png,image/jpeg"
                          onChange={(e) => handleLogoChange(e.target.files)}
                        />
                      </label>

                      {logoPreview && (
                        <button
                          className="btn btn-sm btn-ghost"
                          onClick={handleRemoveLogo}
                        >
                          Remove Logo
                        </button>
                      )}

                      <p className="text-xs text-base-content/60">
                        Appears next to the organization name in the header.
                      </p>
                    </div>
                  </div>
                </div>

                {/* Banner */}
                <div>
                  <h3 className="text-lg font-semibold mb-2">Warning Banner</h3>

                  <textarea
                    className="textarea textarea-bordered w-full min-h-24"
                    value={bannerText}
                    onChange={(e) => setBannerText(e.target.value)}
                  />

                  <label className="label">
                    <span className="label-text-alt text-base-content/60">
                      Displayed beneath the top header in organization context.
                    </span>
                    <span className="label-text-alt text-base-content/40">
                      {bannerText.length} / 240
                    </span>
                  </label>
                </div>

                <div className="flex justify-end">
                  <button
                    className="btn btn-primary"
                    onClick={handleSaveBranding}
                  >
                    Save Branding
                  </button>
                </div>
              </div>
            )}

            {/* ------------------------------------------------------------------ */}
            {/*                               Storage                             */}
            {/* ------------------------------------------------------------------ */}
            {activeTab === "storage" && (
              <div className="space-y-6">
                <h3 className="text-lg font-semibold">Default Storage</h3>
                <p className="text-sm text-base-content/70">
                  Configure the default unmounted object storage location for
                  all projects.
                </p>

                <div className="form-control">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Storage Location
                    </span>
                  </label>
                  <select
                    className="select select-bordered"
                    value={storageLocation}
                    onChange={(e) => setStorageLocation(e.target.value)}
                  >
                    <option value="org-default">Organization Default</option>
                    <option value="s3-west">AWS S3 (us-west-2)</option>
                    <option value="s3-east">AWS S3 (us-east-1)</option>
                    <option value="local-cluster">Local Cluster Storage</option>
                  </select>
                  <label className="label">
                    <span className="label-text-alt text-base-content/60">
                      This location will appear in project admin under "Data
                      Sources & Integrations".
                    </span>
                  </label>
                </div>

                <div className="flex justify-end">
                  <button
                    className="btn btn-primary"
                    onClick={handleSaveSettings}
                  >
                    Save Storage Settings
                  </button>
                </div>
              </div>
            )}

            {/* ------------------------------------------------------------------ */}
            {/*                             Insight Services                       */}
            {/* ------------------------------------------------------------------ */}
            {activeTab === "insight" && (
              <div className="space-y-6">
                <h3 className="text-lg font-semibold">
                  Insight Services Configuration
                </h3>
                <p className="text-sm text-base-content/70">
                  Set authentication and endpoints for DeepLynx Insight
                  services. All projects in this organization will use these
                  settings.
                </p>

                {/* Auth Mode */}
                <div className="form-control">
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
                </div>

                {/* Conditional Auth Fields */}
                {authMode === "basic" && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                  <div className="space-y-4">
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
                        value={oauthTokenUrl}
                        onChange={(e) => setOauthTokenUrl(e.target.value)}
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
                    value={baseEndpointUrl}
                    onChange={(e) => setBaseEndpointUrl(e.target.value)}
                    placeholder="https://insight.example.com/api"
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
                    value={inferenceEndpointPath}
                    onChange={(e) => setInferenceEndpointPath(e.target.value)}
                  />
                </div>

                {/* Footer Actions */}
                <div className="flex justify-between mt-4">
                  <button
                    className="btn btn-ghost"
                    onClick={handleTestConnection}
                    disabled={testingConnection}
                  >
                    {testingConnection && (
                      <span className="loading loading-spinner loading-sm" />
                    )}
                    Test Connection
                  </button>

                  <button
                    className="btn btn-primary"
                    onClick={handleSaveSettings}
                    disabled={savingSettings}
                  >
                    {savingSettings && (
                      <span className="loading loading-spinner loading-sm" />
                    )}
                    Save Insight Settings
                  </button>
                </div>
              </div>
            )}

            {/* ------------------------------------------------------------------ */}
            {/*                        End Tab Content                            */}
            {/* ------------------------------------------------------------------ */}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SettingsTwo;
