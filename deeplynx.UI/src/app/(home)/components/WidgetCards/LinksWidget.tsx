import CreateLink from "@/app/(home)/components/WidgetCards/WidgetCardModals/CreateLinksModal";
import { useLanguage } from "@/app/contexts/Language";
import {
  AdjustmentsHorizontalIcon,
  ArrowTrendingUpIcon,
  DocumentDuplicateIcon,
  FolderIcon,
} from "@heroicons/react/24/outline";
import { PlusCircleIcon } from "@heroicons/react/24/solid";
import { useState } from "react";

const LinksWidget = () => {
  const { t } = useLanguage();
  const [linkModal, setLinkModal] = useState(false);

  return (
    <div className="card-body">
      <div className="flex justify-between items-center">
        <h2 className="card-title">{t.translations.LINKS}</h2>
        <button onClick={() => setLinkModal(true)}>
          <PlusCircleIcon className="w-10 h-10 text-secondary" />
        </button>
      </div>
      <div className="flex justify-between p-4">
        <div className="flex flex-col items-center">
          <AdjustmentsHorizontalIcon className="size-8 text-secondary" />
          <button className="btn btn-link text-secondary">
            {t.translations.ROLES}
          </button>
        </div>
        <div className="flex flex-col items-center">
          <FolderIcon className="size-8 text-secondary" />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            {t.translations.FILE_EXPLORER}
          </button>
        </div>
        <div className="flex flex-col items-center">
          <DocumentDuplicateIcon className="size-8 text-secondary" />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            {t.translations.REPORTS}
          </button>
        </div>
        <div className="flex flex-col items-center">
          <ArrowTrendingUpIcon className="size-8 text-secondary" />
          <button className="btn btn-link text-secondary ">
            {t.translations.TRENDS}
          </button>
        </div>
      </div>

      {/* Create Link Modal */}
      <CreateLink isOpen={linkModal} onClose={() => setLinkModal(false)} />
    </div>
  );
};

export default LinksWidget;
