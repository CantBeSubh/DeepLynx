"use client";
import { ExistingFile } from "@/app/(home)/types/upload";
import { useLanguage } from "@/app/contexts/Language";

type Props = {
  needsTarget: boolean;
  selectedTarget: ExistingFile | null;
  className?: string;
};

export default function FileDetailsCard({
  needsTarget,
  selectedTarget,
  className = "",
}: Props) {
  const { t } = useLanguage();
  if (!needsTarget) return null;

  return (
    <div className={`card card-border ${className}`}>
      <div className="card-body">
        <div className="flex justify-between items-center card-title w-full">
          <h2>
            {selectedTarget
              ? selectedTarget.alias || selectedTarget.name
              : "File details"}
          </h2>
          {selectedTarget?.timeSeries && (
            <span className="badge badge-info">
              {t.translations.TIMESERIES}
            </span>
          )}
        </div>

        {!selectedTarget ? (
          <p className="text-sm opacity-70">
            {t.translations.SELECT_EXISTING_FILE_FROM_DROPDOWN}
          </p>
        ) : (
          <>
            <p className="text-sm opacity-90">{selectedTarget.description}</p>

            <div className="mt-3 grid grid-cols-1 sm:grid-cols-2 gap-2 text-sm">
              <div>
                <span className="opacity-60">{t.translations.FILE_NAME}</span>{" "}
                {selectedTarget.name}
              </div>
              <div>
                <span className="opacity-60">{t.translations.ALIAS}</span>{" "}
                {selectedTarget.alias}
              </div>
              <div>
                <span className="opacity-60">{t.translations.LAST_UPDATE}</span>{" "}
                {selectedTarget.lastUpdate}
              </div>
              <div>
                <span className="opacity-60">{t.translations.UPDATED_BY}</span>{" "}
                {selectedTarget.updatedBy}
              </div>
              <div className="sm:col-span-2">
                <span className="opacity-60">{t.translations.DATA_SOURCE}</span>{" "}
                {selectedTarget.dataSource}
              </div>
              <div className="sm:col-span-2">
                <span className="opacity-60">
                  {t.translations.PROPERTIES_SOURCE}
                </span>{" "}
                {selectedTarget.propertiesSources}
              </div>
            </div>

            <div className="mt-3 flex flex-wrap gap-2">
              {selectedTarget.tags?.map((tag) => (
                <span key={tag} className="badge badge-outline">
                  {tag}
                </span>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
