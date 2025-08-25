"use client";
import React from "react";
import { RecentUpload } from "@/app/(home)/types/upload";
import {
  ArrowsRightLeftIcon,
  LinkIcon,
  CircleStackIcon,
} from "@heroicons/react/24/outline";

const iconMap = {
  arrows: ArrowsRightLeftIcon,
  link: LinkIcon,
  stack: CircleStackIcon,
} as const;

type IconKey = keyof typeof iconMap;

export default function RecentUploadsCard({
  title = "Recent Uploads to Project",
  uploads,
  uploadText,
}: {
  title?: string;
  uploads: RecentUpload[];
  uploadText: string;
}) {
  return (
    <div className="card card-border w-auto">
      <div className="card-body">
        <h2 className="card-title">{title}</h2>
        <ul className="list bg-base-200/20 rounded rounded-xl">
          {uploads.map((u) => {
            const Icon = iconMap[u.icon as IconKey];
            return (
              <li className="list-row" key={u.id}>
                <div className="avatar indicator relative inline-block">
                  <span className="indicator-item indicator-bottom">
                    <div className="h-7 w-7 rounded-full border-2 border-secondary bg-white flex items-center justify-center">
                      <Icon className="h-5 w-5 text-secondary" />
                    </div>
                  </span>
                  <div className="h-10 w-10 rounded-full bg-base-200" />
                </div>
                <div className="pt-2">
                  <b className="text-black">{u.name}</b> {uploadText} {u.file}
                </div>
              </li>
            );
          })}
        </ul>
      </div>
    </div>
  );
}
