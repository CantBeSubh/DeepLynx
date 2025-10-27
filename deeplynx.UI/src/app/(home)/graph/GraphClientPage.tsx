//src/app/(home)/graph/GraphClientPage.tsx
"use client";

import React, { useEffect, useRef, useState } from "react";
import Graph from "graphology";
import { SigmaContainer } from "@react-sigma/core";
import "@react-sigma/core/lib/style.css";
import Sigma from "sigma";
import { Attributes } from "graphology-types";
import { getGraphDataForRecord } from "@/app/lib/graph_services.client";
import { GraphResponseDto } from "../types/responseDTOs";

// ============================================
// TYPE DEFINITIONS
// ============================================
/**
 * Sigma.js node attributes
 * These properties control how nodes appear and are positioned
 */
interface NodeAttributes extends Attributes {
  x: number; // X coordinate position
  y: number; // Y coordinate position
  size: number; // Visual size of the node
  label: string; // Text label displayed on/near the node
  color: string; // Node color (hex format)
  nodeType: string; // Node type from backend (renamed from 'type' to avoid Sigma.js conflict)
}

/**
 * Sigma.js edge attributes
 */
interface EdgeAttributes extends Attributes {
  size?: number; // Optional: thickness of the edge line
  color?: string; // Optional: color of the edge line
  label?: string; // Edge label (relationship name)
  relationshipId?: number | null;
  edgeId?: number;
}

// ============================================
// MAIN COMPONENT
// ============================================

interface GraphClientPageProps {
  projectId: string;
  recordId: number;
  depth?: number;
}

const GraphClientPage = ({
  projectId,
  recordId,
  depth = 2,
}: GraphClientPageProps) => {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  return (
    <div style={{ height: "800px", width: "80%", position: "relative" }}>
      {/* Loading state */}
      {isLoading && (
        <div
          style={{
            position: "absolute",
            top: "50%",
            left: "50%",
            transform: "translate(-50%, -50%)",
          }}
        >
          Loading graph data...
        </div>
      )}

      {/* Error state */}
      {error && (
        <div
          style={{
            position: "absolute",
            top: "50%",
            left: "50%",
            transform: "translate(-50%, -50%)",
            color: "red",
          }}
        >
          Error: {error}
        </div>
      )}

      {/* Graph container */}

      <MyGraph
        projectId={projectId}
        recordId={recordId}
        depth={depth}
        onLoadingChange={setIsLoading}
        onError={setError}
      />
    </div>
  );
};

// ============================================
// GRAPH COMPONENT
// ============================================

interface MyGraphProps {
  projectId: string;
  recordId: number; // Required
  depth?: number;
  onLoadingChange: (loading: boolean) => void;
  onError: (error: string | null) => void;
}

const MyGraph = ({
  projectId,
  recordId,
  depth = 2,
  onLoadingChange,
  onError,
}: MyGraphProps) => {
  // Reference to the DOM element that will hold the graph
  const containerRef = useRef<HTMLDivElement>(null);

  // Reference to the Sigma instance for cleanup
  const sigmaRef = useRef<Sigma<
    NodeAttributes,
    EdgeAttributes,
    Attributes
  > | null>(null);

  useEffect(() => {
    // Safety check: ensure the container element exists
    if (!containerRef.current) return;

    // ========================================
    // FETCH AND RENDER GRAPH DATA
    // ========================================

    const fetchAndRenderGraph = async () => {
      try {
        onLoadingChange(true);
        onError(null);

        // Fetch graph data from backend using the service
        const data = await getGraphDataForRecord(projectId, {
          recordId,
          depth,
        });

        // Validate that we have data
        if (!data.nodes || data.nodes.length === 0) {
          onError("No nodes found in the graph data");
          onLoadingChange(false);
          return;
        }

        // ========================================
        // CREATE AND POPULATE GRAPH
        // ========================================

        // Initialize a new graph instance
        const graph = new Graph<NodeAttributes, EdgeAttributes>();

        // Generate color based on node type
        const getColorByType = (type: string): string => {
          const colors: Record<string, string> = {
            root: "#9b59b6", // Purple for root
            node: "#3498db", // Blue for nodes
            person: "#3498db", // Blue
            company: "#2ecc71", // Green
            document: "#e74c3c", // Red
            location: "#f39c12", // Orange
            default: "#95a5a6", // Gray
          };
          return colors[type.toLowerCase()] || colors["default"];
        };

        // Add all nodes to the graph
        // First, identify the root node
        const rootNode = data.nodes.find((node) => node.type === "root");
        const otherNodes = data.nodes.filter((node) => node.type !== "root");

        data.nodes.forEach((node, index) => {
          // Convert numeric ID to string (Graphology requires string IDs)
          const nodeId = String(node.id);

          let x = 0;
          let y = 0;

          // Position root node at the center
          if (node.type === "root") {
            x = 0;
            y = 0;
          } else {
            // Position other nodes in a circle around the root
            const nodeIndex = otherNodes.findIndex((n) => n.id === node.id);
            const angle = (nodeIndex / otherNodes.length) * 2 * Math.PI;
            const radius = 1; // Adjust this to make the circle bigger or smaller
            x = Math.cos(angle) * radius;
            y = Math.sin(angle) * radius;
          }

          graph.addNode(nodeId, {
            x,
            y,
            size: node.type === "root" ? 20 : 15, // Root node is larger
            label: node.label,
            color: getColorByType(node.type),
            nodeType: node.type,
          });
        });

        // Add all edges (links) to the graph
        data.links.forEach((link, index) => {
          // Convert numeric IDs to strings
          const sourceId = String(link.source);
          const targetId = String(link.target);

          // Check if both nodes exist before creating edge
          if (graph.hasNode(sourceId) && graph.hasNode(targetId)) {
            // Use edgeId, or create a unique ID
            const edgeId = link.edgeId ? String(link.edgeId) : `edge-${index}`;

            graph.addEdge(sourceId, targetId, {
              size: 2,
              color: "#999",
              label: link.relationshipName || undefined,
              relationshipId: link.relationshipId,
              edgeId: link.edgeId,
            });
          }
        });

        // ========================================
        // INITIALIZE SIGMA RENDERER
        // ========================================

        // Destroy existing Sigma instance if it exists
        if (sigmaRef.current) {
          sigmaRef.current.kill();
        }

        // Create new Sigma instance to render the graph
        if (containerRef.current) {
          sigmaRef.current = new Sigma(graph, containerRef.current, {
            // Optional: Configure Sigma settings
            renderEdgeLabels: false, // Set to true if you want to show relationship names
          });
        }

        onLoadingChange(false);
      } catch (err) {
        console.error("Error fetching graph data:", err);
        onError(
          err instanceof Error ? err.message : "Failed to load graph data"
        );
        onLoadingChange(false);
      }
    };

    fetchAndRenderGraph();

    // ========================================
    // CLEANUP FUNCTION
    // ========================================

    // This runs when the component unmounts
    return () => {
      sigmaRef.current?.kill();
    };
  }, [projectId, recordId, depth, onLoadingChange, onError]);

  // Render the container div that Sigma will attach to
  return <div ref={containerRef} style={{ width: "100%", height: "100%" }} />;
};

export default GraphClientPage;
