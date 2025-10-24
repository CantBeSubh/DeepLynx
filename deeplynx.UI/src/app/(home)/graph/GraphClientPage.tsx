"use client";

import React, { useEffect, useRef, useState } from "react";
import Graph from "graphology";
import { SigmaContainer } from "@react-sigma/core";
import "@react-sigma/core/lib/style.css";
import Sigma from "sigma";
import { Attributes } from "graphology-types";
import axios from "axios";

// ============================================
// TYPE DEFINITIONS
// ============================================

/**
 * Backend API response structure
 */
interface GraphResponse {
  nodes: Node[];
  links: Link[];
}

interface Node {
  id: string | null;
  label: string;
  type: string;
}

interface Link {
  source: string | null;
  target: string | null;
  relationshipId: string | null;
  relationshipName: string | null;
  edgeId: string | null;
}

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
  type: string; // Node type from backend
}

/**
 * Sigma.js edge attributes
 */
interface EdgeAttributes extends Attributes {
  size?: number; // Optional: thickness of the edge line
  color?: string; // Optional: color of the edge line
  label?: string; // Edge label (relationship name)
  relationshipId?: string | null;
  edgeId?: string | null;
}

// ============================================
// MAIN COMPONENT
// ============================================

interface GraphClientPageProps {
  projectId: string;
  recordId?: number;
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
      <SigmaContainer style={{ height: "100%" }}>
        <MyGraph
          projectId={projectId}
          recordId={recordId}
          depth={depth}
          onLoadingChange={setIsLoading}
          onError={setError}
        />
      </SigmaContainer>
    </div>
  );
};

// ============================================
// GRAPH COMPONENT
// ============================================

interface MyGraphProps {
  projectId: string;
  recordId?: number;
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

        // Fetch graph data from backend
        const { data } = await axios.get<GraphResponse>(
          `http://localhost:5095/projects/${projectId}/edges/GetGraphDataForRecord`,
          {
            params: {
              recordId,
              depth,
            },
          }
        );

        // Validate that we have data
        if (!data.nodes || data.nodes.length === 0) {
          onError("No nodes found in the graph data");
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
            person: "#3498db", // Blue
            company: "#2ecc71", // Green
            document: "#e74c3c", // Red
            location: "#f39c12", // Orange
            default: "#95a5a6", // Gray
          };
          return colors[type.toLowerCase()] || colors["default"];
        };

        // Add all nodes to the graph
        data.nodes.forEach((node, index) => {
          // Skip nodes with null IDs
          if (!node.id) return;

          // Calculate position using a circular layout
          // This spreads nodes evenly in a circle
          const angle = (index / data.nodes.length) * 2 * Math.PI;
          const radius = 5;

          graph.addNode(node.id, {
            x: Math.cos(angle) * radius,
            y: Math.sin(angle) * radius,
            size: 15,
            label: node.label || node.id,
            color: getColorByType(node.type),
            type: node.type,
          });
        });

        // Add all edges (links) to the graph
        data.links.forEach((link, index) => {
          // Skip links with null source or target
          if (!link.source || !link.target) return;

          // Check if both nodes exist before creating edge
          if (graph.hasNode(link.source) && graph.hasNode(link.target)) {
            // Use edgeId if available, otherwise create a unique ID
            const edgeId = link.edgeId || `edge-${index}`;

            graph.addEdge(link.source, link.target, {
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
            renderEdgeLabels: true, // Show relationship names
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
