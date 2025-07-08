import React, { useState } from "react";

// need to move to the backend and connect to an avatar on the front end.
// const users = [
//   { id: 1, name: 'Peter Syderhoud', avatar: '' },
//   { id: 2, name: 'Jason Kuipers', avatar: '' },
//   { id: 3, name: 'Jaren Brownlee', avatar: '' },
//   { id: 4, name: 'Victor Walker', avatar: '' },
//   { id: 5, name: 'Jaren Brownlee', avatar: '' },
//   { id: 6, name: 'Jaren Brownlee', avatar: '' }
// ]

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
              <b>Jason Kuipers</b> linked a new source employee_records.xlsx
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
    fill="white"
    viewBox="0 0 24 24"
    strokeWidth={2}
    stroke="oklch(44.08% 0.141 255.19)"
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
      d="M12 8v8m4-4h-8"
    />
  </svg>
);

const ConnectionBadge = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="white"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="oklch(44.08% 0.141 255.19)"
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
    d="M7.5 21 3 16.5m0 0L7.5 12M3 16.5h13.5m0-13.5L21 7.5m0 0L16.5 12M21 7.5H7.5" />
</svg>
);

const LinkBadge = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="white"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="oklch(44.08% 0.141 255.19)"
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
    d="M13.19 8.688a4.5 4.5 0 0 1 1.242 7.244l-4.5 4.5a4.5 4.5 0 0 1-6.364-6.364l1.757-1.757m13.35-.622 1.757-1.757a4.5 4.5 0 0 0-6.364-6.364l-4.5 4.5a4.5 4.5 0 0 0 1.242 7.244" />
</svg>
);

const GenerateBadge = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="white"
    viewBox="0 0 24 24"
    strokeWidth={1.5}
    stroke="oklch(44.08% 0.141 255.19)"
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
      d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25M9 16.5v.75m3-3v3M15 12v5.25m-4.5-15H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
</svg>
);

const UploadBadge = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    fill="white"
    viewBox="0 0 24 24"
    strokeWidth={2}
    stroke="oklch(44.08% 0.141 255.19)"
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
    d="M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375m16.5 0v3.75m-16.5-3.75v3.75m16.5 0v3.75C20.25 16.153 16.556 18 12 18s-8.25-1.847-8.25-4.125v-3.75m16.5 0c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125"
    />
  </svg>
);

export default RecentActivityWidget;