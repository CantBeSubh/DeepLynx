import React, { useState } from "react";
import Image from 'next/image';
import {CircleStackIcon, DocumentChartBarIcon, LinkIcon, ArrowsRightLeftIcon } from "@heroicons/react/24/outline";

const RecentActivityWidget = () => {
  return (
    <div className="card-body">
      <h2 className="card-title">Recent Activity</h2>
        <ul className="list bg-base-100">

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom">
              <div className="h-7 w-7 rounded-full border-2 border-secondary bg-white flex items-center justify-center">
                <CircleStackIcon className="h-5 w-5 text-secondary" />
              </div>
            </span>
            <div className="h-10 w-10 rounded-full">
              <Image
                src="/images/j2.png"
                alt="avatar"
                width="300"
                height="300"/>
            </div>
          </div>
          <div>
            <div className="pt-2">
              <b>Jaren Brownlee</b> uploaded a new source weather_data.csv
            </div>
          </div>
        </li>

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom">
              <div className="h-7 w-7 rounded-full border-2 border-secondary bg-white flex items-center justify-center">
                <DocumentChartBarIcon className="h-5 w-5 text-secondary" />
              </div>
            </span>
            <div className="h-10 w-10 rounded-full">
              <Image
                src="/images/natalie.png"
                alt="avatar"
                width="300"
                height="300"/>
            </div>
          </div>
          <div className="pt-2">
              <b>Natalie Hergesheimer</b> generated a new report
          </div>
        </li>

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom">
              <div className="h-7 w-7 rounded-full border-2 border-secondary bg-white flex items-center justify-center">
                <LinkIcon className="h-5 w-5 text-secondary" />
              </div>
            </span>
            <div className="h-10 w-10 rounded-full">
              <Image
                src="/images/jason.png"
                alt="avatar"
                width="300"
                height="300"/>
            </div>
          </div>
          <div>
            <div className="pt-2">
              <b>Jason Kuipers</b> linked a new source employee_records.xlsx
            </div>
          </div>
        </li>

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom inline-block">
              <div className="h-7 w-7 rounded-full border-2 border-secondary bg-white flex items-center justify-center">
                <ArrowsRightLeftIcon className="h-5 w-5 text-secondary" />
              </div>
            </span>
            <div className="h-10 w-10 rounded-full">
              <Image
                src="/images/victor.png"
                alt="avatar"
                width="300"
                height="300"/>
            </div>
          </div>
          <div>
            <div className="pt-2">
              <b>Victor Walker</b> created a new connection between weather_data.csv and project_timeline.docx
            </div>
          </div>
        </li>

      </ul>
    </div>
  );
};

export default RecentActivityWidget;