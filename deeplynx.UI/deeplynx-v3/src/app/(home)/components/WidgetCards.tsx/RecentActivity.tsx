import React, { useState } from "react";

const RecentActivityWidget = () => {
  return (
    <div className="card-body">
      <h2 className="card-title">Recent Activity</h2>
        <ul className="list bg-base-100">

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom">
              <AvatarBadge />
            </span>
            <div className="h-10 w-10 rounded-full">
              <img
                src="https://img.daisyui.com/images/profile/demo/1@94.webp"/>
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
              <AvatarBadge />
            </span>
            <div className="h-10 w-10 rounded-full">
              <img
                src="https://img.daisyui.com/images/profile/demo/batperson@192.webp"
              />
            </div>
          </div>
          <div className="pt-2">
              <b>Natalie Hergesheimer</b> generated a new report
          </div>
        </li>

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom">
              <AvatarBadge />
            </span>
            <div className="h-10 w-10 rounded-full">
              <img
                src="https://img.daisyui.com/images/profile/demo/4@94.webp"/>
            </div>
          </div>
          <div>
            <div className="pt-2">
              <b>Jason Kuipers</b> linked a new sourcec employee_records.xlsx
            </div>
          </div>
        </li>

        <li className="list-row">
          <div className="avatar indicator relative inline-block">
            <span className="indicator-item indicator-bottom inline-block">
              <AvatarBadge />
            </span>
            <div className="h-10 w-10 rounded-full">
              <img
                src="https://img.daisyui.com/images/profile/demo/3@94.webp"/>
            </div>
          </div>
          <div>
            <div className="pt-2">
              <b>Isaac Huffman</b> created a new connection between weather_data.csv and project_timeline.docx
            </div>
          </div>
        </li>

      </ul>
    </div>
  );
};

const AvatarBadge = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="oklch(44.08% 0.141 255.19)"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="white"
    className="w-6 h-6 rounded-full"
  >
    <circle
        cx="12"
        cy="12"
        r="12"
        fill="btn-secondary"
    />
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 4.5v15m7.5-7.5h-15"
    />
  </svg>
);

export default RecentActivityWidget;