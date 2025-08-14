import LargeSearchBar from "@/app/(home)/components/LargeSearchBar";
import SavedSearches from "@/app/(home)/components/SavedSearches";
import {
  ArrowsRightLeftIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  CircleStackIcon,
  Cog6ToothIcon,
  PlusCircleIcon,
  PlusIcon,
  RectangleGroupIcon,
  Squares2X2Icon,
} from "@heroicons/react/24/outline";
import Link from "next/link";
import React from "react";
import { translations } from "@/app/lib/translations";
import Skeleton from "react-loading-skeleton";

const LoadingProjectDetail = () => {
  const locale = "en";
  const t = translations[locale];

  const project = [1];
  const paginatedRecords = [1, 2, 3, 4, 5];
  const totalPages = 2;
  return (
    <div>
      <main>
        <div className="text-info-content bg-base-200/40 py-3 p-12">
          <h1 className="text-2xl">
            <Skeleton />
          </h1>
          <p className="mt-2 text-base-content">
            <Skeleton />
          </p>
          <p>
            <Skeleton />
          </p>
        </div>

        <div className="flex w-full mt-6">
          {/* left column */}
          <div className="w-full md:w-3/5 px-4">
            <div className="flex flex-col">
              <LargeSearchBar className="mb-4 px-4" />
            </div>

            <div className="card card-border">
              <div className="card-body">
                <div className="flex justify-between px-4">
                  <h1 className="text-xl font-semibold">
                    {t.ProjectDashboard.DATA_CATALOG_OVERVIEW}
                  </h1>
                  <Link className="btn btn-secondary" href={""}>
                    Visit
                  </Link>
                </div>
                {/* Recently Added Records Card */}
                <div className="bg-base-100 rounded-xl p-4">
                  <h2 className="text-lg text-black mb-4t">
                    Recently Added Records
                  </h2>
                  <div className="divider m-0 mt-2"></div>
                  <ul className="list mt-0">
                    {paginatedRecords.map((record, index) => (
                      <li
                        key={index}
                        className="border-b border-base-content cursor-pointer hover:bg-base-200/30 p-2 pl-0 rounded-sm"
                      >
                        <div className="text-accent-content mb-1">
                          <Skeleton />
                        </div>
                        <div className="text-sm text-base-300 space-x-2 flex flex-wrap">
                          <span>
                            Class:{" "}
                            <span className="badge badge-info badge-sm text-xs">
                              <Skeleton />
                            </span>
                          </span>
                          <span className="ml-4">
                            Last Edited: <Skeleton />
                          </span>
                          <span className="ml-4">
                            Project: <Skeleton />
                          </span>
                          <span className="ml-4">
                            Data Source: <Skeleton />
                          </span>
                        </div>
                      </li>
                    ))}
                  </ul>

                  {/* Pagination Controls */}
                  {totalPages > 1 && (
                    <div className="flex justify-end gap-2 mt-4">
                      <button className="btn btn-sm btn-ghost">
                        <ChevronLeftIcon className="size-6" />
                      </button>
                      <span className="px-2 text-sm">
                        <Skeleton />
                      </span>
                      <button className="btn btn-sm btn-ghost">
                        <ChevronRightIcon className="size-6" />
                      </button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* right column */}
          <div className="w-full md:w-2/5 px-4">
            <div className="flex justify-between items-center mb-4">
              <button className="btn btn-outline btn-secondary flex items-center mr-2">
                <Cog6ToothIcon className="h-6 w-6" />
                {t.ProjectDashboard.CUSTOMIZE}
              </button>
              <button className="btn btn-secondary text-primary-content flex items-center">
                <PlusIcon className="h-6 w-6" />
                {t.ProjectDashboard.WIDGET}
              </button>
            </div>
            {/* Widget card */}
            <div className="card card-border mt-4">
              <div className="card-body">
                <h2 className="card-title">{t.WidgetCards.DATA_OVERVIEW}</h2>
                <div className="stats shadow">
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.WidgetCards.PROJECTS}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <Squares2X2Icon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.WidgetCards.DATA_RECORD}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <CircleStackIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                </div>
                <div className="stats shadow">
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.WidgetCards.CLASSES}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <RectangleGroupIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                  <div className="stat">
                    <div className="stat-title text-secondary">
                      {t.WidgetCards.CONNECTIONS}
                    </div>
                    <div className="stat-value text-secondary flex items-center">
                      <ArrowsRightLeftIcon className="size-8 mr-2" />
                      <Skeleton width={40} />
                    </div>
                  </div>
                </div>
              </div>
            </div>
            {/* Team Members */}
            <div className="card card-border mt-4">
              <div className="card-body">
                <div className="">
                  <h2 className="card-title flex items-center">
                    {t.WidgetCards.TEAM_MEMBERS}
                  </h2>
                  <Skeleton width={700} height={50} />
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default LoadingProjectDetail;
