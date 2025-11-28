// src/app/(home)/organization_management/settings/OrganizationSettingsOption3.tsx
"use client";

import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import {
  ShieldCheckIcon,
  CloudIcon,
  Cog6ToothIcon,
  PhotoIcon,
  InformationCircleIcon,
} from "@heroicons/react/24/outline";

import { useOrganizationSession } from "@/app/contexts/OrganizationSessionProvider";

/* -------------------------------------------------------------------------- */
/*                                   Types                                    */
/* -------------------------------------------------------------------------- */

type AuthMode = "none" | "basic" | "oauth";

/* -------------------------------------------------------------------------- */
/*                       Organization Settings – Option 3                     */
/* -------------------------------------------------------------------------- */

const SettingsThree = () => {
  /* ------------------------------------------------------------------------ */
  /*                           Organization Context                           */
  /* ------------------------------------------------------------------------ */

  const { organization } = useOrganizationSession();

  /* ------------------------------------------------------------------------ */
  /*                               Branding State                             */
  /* ------------------------------------------------------------------------ */

  const [logoPreview, setLogoPreview] = useState<string | null>(null);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [bannerText, setBannerText] = useState(
    "This organization space may contain CUI/ECI data that must be protected accordingly."
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
      // TODO: Wire this into your backend
      toast.success("Branding saved (mock)");
    } catch (err) {
      toast.error("Failed to save branding");
    }
  };

  /* ------------------------------------------------------------------------ */
  /*                          Storage / Insight State                         */
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
      toast.error("Base endpoint URL is required");
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
  /*                               Main Render                                */
  /* ------------------------------------------------------------------------ */

  return (
    <div className="p-6">
      <div className="max-w-6xl mx-auto space-y-6">
        {/* Page Header */}
        <div>
          <h2 className="text-2xl font-bold text-base-content">
            Organization Settings
          </h2>
          <p className="text-base-content/70 mt-1">
            Configure branding, storage defaults, and Insight services for this
            organization.
          </p>
        </div>
        {/* Summary Strip */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Org Info */}
          <div className="card bg-base-100 border border-base-300 shadow-sm">
            <div className="card-body flex flex-row items-center gap-4">
              <div className="avatar">
                <div className="w-12 h-12 rounded-xl bg-base-200 flex items-center justify-center overflow-hidden">
                  {logoPreview ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                      src={logoPreview}
                      alt="Org logo"
                      className="object-cover w-full h-full"
                    />
                  ) : (
                    <PhotoIcon className="w-6 h-6 text-base-content/50" />
                  )}
                </div>
              </div>
              <div>
                <p className="text-xs text-base-content/60">Organization</p>
                <p className="font-semibold">
                  {organization?.organizationName || "Current Organization"}
                </p>
                <p className="text-xs text-base-content/50 mt-1">
                  Branding and security banner apply across this org.
                </p>
              </div>
            </div>
          </div>

          {/* Compliance Hint */}
          <div className="card bg-base-100 border border-base-300 shadow-sm">
            <div className="card-body flex flex-row items-center gap-4">
              <div className="rounded-xl bg-warning/20 p-2">
                <ShieldCheckIcon className="w-6 h-6 text-warning" />
              </div>
              <div>
                <p className="text-xs text-base-content/60">
                  Compliance Banner
                </p>
                <p className="font-semibold text-sm">
                  Banner visible under the header
                </p>
                <p className="text-xs text-base-content/60 mt-1">
                  Use this to communicate handling rules (CUI, ECI, etc.).
                </p>
              </div>
            </div>
          </div>

          {/* Insight Summary */}
          <div className="card bg-base-100 border border-base-300 shadow-sm">
            <div className="card-body flex flex-row items-center gap-4">
              <div className="rounded-xl bg-primary/10 p-2">
                <Cog6ToothIcon className="w-6 h-6 text-primary" />
              </div>
              <div>
                <p className="text-xs text-base-content/60">Insight Services</p>
                <p className="font-semibold text-sm">
                  Shared across all projects
                </p>
                <p className="text-xs text-base-content/60 mt-1">
                  Authentication and endpoints are configured at the org level.
                </p>
              </div>
            </div>
          </div>
        </div>
        {/* Two main cards side by side */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left: Branding & Banner */}
          <div className="card bg-base-100 border border-primary/40 shadow-md">
            <div className="card-body space-y-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <PhotoIcon className="w-5 h-5 text-primary" />
                  <h3 className="text-lg font-semibold">
                    Branding & Header Banner
                  </h3>
                </div>
              </div>

              {/* Logo Upload */}
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
                      <PhotoIcon className="w-7 h-7 text-base-content/40" />
                    )}
                  </div>
                </div>
                <div className="flex flex-col gap-2">
                  <label className="btn btn-sm btn-primary">
                    Upload Logo
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
                      Remove
                    </button>
                  )}
                  <p className="text-xs text-base-content/60">
                    Appears next to the organization name in the top header.
                  </p>
                </div>
              </div>

              {/* Banner Text */}
              <div>
                <label className="label">
                  <span className="label-text font-semibold">
                    Header Banner Text
                  </span>
                </label>
                <textarea
                  className="textarea textarea-bordered w-full min-h-24"
                  value={bannerText}
                  onChange={(e) => setBannerText(e.target.value)}
                />
                <div className="flex items-center justify-between mt-1">
                  <span className="text-xs text-base-content/60 flex items-center gap-1">
                    <InformationCircleIcon className="w-4 h-4" />
                    Displayed under the top header in the organization space.
                  </span>
                  <span className="text-xs text-base-content/40">
                    {bannerText.length} / 240
                  </span>
                </div>
              </div>

              <div className="flex justify-end pt-2">
                <button
                  className="btn btn-primary"
                  onClick={handleSaveBranding}
                >
                  Save Branding
                </button>
              </div>
            </div>
          </div>

          {/* Right: Storage & Insight Services */}
          <div className="card bg-base-100 border border-primary/40 shadow-md">
            <div className="card-body space-y-6">
              {/* Storage */}
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <CloudIcon className="w-5 h-5 text-primary" />
                  <h3 className="text-lg font-semibold">
                    Default Object Storage
                  </h3>
                </div>
                <p className="text-sm text-base-content/70">
                  Configure the default unmounted object storage location used
                  by projects in this organization.
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
                      Appears in the project admin portal under &quot;Data
                      Sources &amp; Integrations&quot; as the default.
                    </span>
                  </label>
                </div>
              </div>

              <div className="divider my-2" />

              {/* Insight Services */}
              <div className="space-y-3">
                <div className="flex items-center gap-2">
                  <Cog6ToothIcon className="w-5 h-5 text-primary" />
                  <h3 className="text-lg font-semibold">Insight Services</h3>
                </div>
                <p className="text-sm text-base-content/70">
                  Configure how DeepLynx Insight services connect and
                  authenticate. All projects in this organization share these
                  settings.
                </p>

                {/* Auth Mode */}
                <div className="form-control">
                  <label className="label">
                    <span className="label-text font-semibold">
                      Authentication Mode
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

                {/* Auth details */}
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
                  <div className="space-y-3">
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
                    placeholder="https://insight.example.com/api"
                    value={baseEndpointUrl}
                    onChange={(e) => setBaseEndpointUrl(e.target.value)}
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

                {/* Actions */}
                <div className="flex justify-between pt-2">
                  <button
                    className="btn btn-ghost"
                    onClick={handleTestConnection}
                    disabled={testingConnection}
                  >
                    {testingConnection && (
                      <span className="loading loading-spinner loading-sm mr-1" />
                    )}
                    Test Connection
                  </button>

                  <button
                    className="btn btn-primary"
                    onClick={handleSaveSettings}
                    disabled={savingSettings}
                  >
                    {savingSettings && (
                      <span className="loading loading-spinner loading-sm mr-1" />
                    )}
                    Save Settings
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>{" "}
        {/* end grid */}
      </div>
    </div>
  );
};

export default SettingsThree;
