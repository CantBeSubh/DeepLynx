'use client'

import { useParams } from 'next/navigation'
import Sidebar from "@/app/components/SideMenu";

export default function ProjectDetailPage() {
  const { projectId } = useParams();

  return (
    <div className="flex min-h-screen bg-base-100 text-base-content">
      <Sidebar />

      {/* Main content wrapper */}
      <div className={`ml-64 flex-grow p-6 transition-margin duration-300`}>
        {/* Main content */}
        <div className="w-full mx-auto">
          {/* Header */}
          <div className="flex justify-between items-center mb-4">
            <h1 className="text-2xl font-bold">Project Dashboard</h1>
            <div className="text-right">
              <p className="mb-1">{projectId}</p>
              <button className="btn btn-outline btn-sm">Manage Dashboard</button>
            </div>
          </div>

          {/* Project Overview Card */}
          <div className="bg-white shadow rounded p-6 mb-8">
            <h2 className="text-lg font-semibold mb-2">DeepLynx Project Overview</h2>
            <div className="h-32 border border-dashed rounded flex items-center justify-center text-gray-400">
              Overview content goes here
            </div>
          </div>

          {/* Your Data */}
          <div>
            <h2 className="text-lg font-semibold mb-4">Your Data</h2>
            <div className="grid grid-cols-3 gap-4">
              {/* File Viewer */}
              <div className="bg-white p-4 rounded shadow flex flex-col">
                <h3 className="font-semibold mb-2">File Viewer</h3>
                <div className="flex-grow">
                  {/* What other content do we want? */}
                </div>
                <button className="btn btn-primary btn-sm w-full">Go to viewer</button>
              </div>

              {/* Model Viewer */}
              <div className="bg-white p-4 rounded shadow flex flex-col">
                <h3 className="font-semibold mb-2">2D/3D Model Viewer</h3>
                <div className="flex-grow">
                  {/* What other content do we want? */}
                </div>
                <button className="btn btn-primary btn-sm w-full">Go to Viewer</button>
              </div>

              {/* Data Viewer */}
              <div className="bg-white p-4 rounded shadow flex flex-col">
                <div className="flex justify-between mb-2">
                  <div>
                    <h3 className="font-semibold">Data Viewer</h3>
                    <p className="text-sm">12 Nodes</p>
                    <p className="text-sm">18 Edges</p>
                  </div>
                  <div className="w-24 h-16 bg-gray-200 flex items-center justify-center text-xs">
                    Snapshot of graph
                  </div>
                </div>
                <div className="flex-grow">
                   {/* What other content do we want? */}
                </div>
                <button className="btn btn-primary btn-sm mt-4">Go to Data Viewer</button>
              </div>
            </div>
          </div>

          {/* TODO: Data Management? */}
        </div>
      </div>
    </div>
  )
}