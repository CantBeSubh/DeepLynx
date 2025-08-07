import React, {useState} from "react";
import CreateLink from "@/app/(home)/components/WidgetCards/WidgetCardModals/CreateLinksModal"
import {AdjustmentsHorizontalIcon, FolderIcon, DocumentDuplicateIcon, ArrowTrendingUpIcon} from "@heroicons/react/24/outline";
import {PlusCircleIcon} from "@heroicons/react/24/solid";

const LinksWidget = () => {
  const [linkModal, setLinkModal] = useState(false);

  return (
    <div className="card-body">
      <div className="flex justify-between items-center">
        <h2 className="card-title">
          Links
        </h2>
        <button onClick={() => setLinkModal(true)}>
          <PlusCircleIcon
            className="w-10 h-10 text-secondary"
          />
        </button>
      </div>
      <div className="flex justify-between p-4">
        <div className="flex flex-col items-center">
          <AdjustmentsHorizontalIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary">Roles</button>
        </div>
        <div className="flex flex-col items-center">
          <FolderIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            File Explorer
          </button>
        </div>
        <div className="flex flex-col items-center">
          <DocumentDuplicateIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary flex flex-col items-center">
            Reports
          </button>
        </div>
        <div className="flex flex-col items-center">
          <ArrowTrendingUpIcon
            className="size-8 text-secondary"
            />
          <button className="btn btn-link text-secondary ">Trends</button>
        </div>
      </div>

      {/* Create Link Modal */}
      <CreateLink
        isOpen={linkModal}
        onClose={() => setLinkModal(false)}
      />

    </div>
  );
};

export default LinksWidget;
