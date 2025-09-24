// src/app/(home)/components/SelectedFilesCard.tsx

"use client";
import { useLanguage } from "@/app/contexts/Language";

type Props = {
  files: File[];
  onRemoveAt: (idx: number) => void;
  onClear: () => void;
  onUpload: () => void;
  canUpload: boolean;
};

export default function SelectedFilesCard({
  files,
  onRemoveAt,
  onClear,
  onUpload,
  canUpload,
}: Props) {
  const { t } = useLanguage();
  if (files.length === 0) return null;
  return (
    <div className="card card-border mt-4">
      <div className="card-body">
        <h2 className="card-title">{t.translations.SELECTED_FILES}</h2>
        {files.length === 0 ? (
          <p className="text-sm opacity-70">
            {t.translations.NO_FILES_SELECTED_YET}
          </p>
        ) : (
          <>
            <ul className="space-y-2">
              {files.map((f, i) => (
                <li key={i} className="flex items-center justify-between gap-3">
                  <div className="truncate">
                    <b className="mr-2">{f.name}</b>
                    <span className="opacity-60 text-xs">
                      {Math.round(f.size / 1024)} KB
                    </span>
                  </div>
                  <button className="btn btn-xs" onClick={() => onRemoveAt(i)}>
                    {t.translations.REMOVE}
                  </button>
                </li>
              ))}
            </ul>
            <div className="mt-4 flex gap-2">
              <button className="btn btn-ghost btn-sm" onClick={onClear}>
                {t.translations.CLEAR_ALL}
              </button>
              <button
                className="btn btn-secondary btn-sm"
                onClick={onUpload}
                disabled={!canUpload}
              >
                {t.translations.UPLOAD}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
