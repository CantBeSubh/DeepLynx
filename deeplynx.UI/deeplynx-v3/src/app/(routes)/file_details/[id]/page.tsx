"use client";

import Tabs from "@/app/components/Tabs";
import { fileData } from "@/app/dummy_data/data";
import { notFound, useParams } from "next/navigation";
import { useRouter } from "next/navigation";
import React from "react";

const FileDetailsPage = () => {
  const router = useRouter();
  const params = useParams();
  const fileId = parseInt(params?.id as string);
  const file = fileData.find((item) => item.id === fileId);

  const tabsData = [
    {
      label: "Metadata",
      content: (
        <div className="card w-100">
          <div className="card-body">
            <p>File Name: {file?.fileName}</p>
            <p>Source: {file?.source}</p>
            <p>Time: {file?.time}</p>
            <p>Container: {file?.container}</p>
            <p>Location: {file?.location}</p>
          </div>
        </div>
      ),
    },
    {
      label: "Ontology",
      content: <div></div>,
    },
    {
      label: "History",
      content: <div></div>,
    },
  ];

  if (!file) return notFound();

  return (
    <div>
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">{file.fileName}</h1>
        <button
          className="btn btn-outline btn-primary"
          onClick={() => router.push("/data_catalog")}
        >
          Return to Data Catalog
        </button>
      </div>

      <div className="divider"></div>

      <div>
        <div className="flex justify-between">
          <h2>Module: 1, Class: A</h2>
          <button className="btn btn-outline btn-primary">Download Data</button>
        </div>
        <div className="flex justify-between flex-wrap  mt-4">
          <div className="flex gap-4">
            {file.tags.map((tag, index) => (
              <span
                key={index}
                className="bg-info/40 text-sm text-center  px-4 py-3 rounded-full shadow-sm"
              >
                {tag}
              </span>
            ))}
          </div>
          <button className="btn btn-outline btn-primary">Edit Tags</button>
        </div>
      </div>
      <div className="mt-4">
        <Tabs tabs={tabsData} />
      </div>
    </div>
  );
};

export default FileDetailsPage;
