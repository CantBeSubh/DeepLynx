// src/app/(home)/graph/page.tsx
import React from "react";
import GraphClientPage from "./GraphClientPage";

interface Props {
  projectId: string;
  recordId: number;
  depth: number;
}

const GraphPage = ({ projectId, recordId, depth }: Props) => {
  return (
    <GraphClientPage projectId={projectId} recordId={recordId} depth={depth} />
  );
};

export default GraphPage;
