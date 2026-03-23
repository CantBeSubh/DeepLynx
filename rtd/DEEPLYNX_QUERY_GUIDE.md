# DeepLynx Data Retrieval & Query Guide

## Overview

This guide documents all methods to query and retrieve data from the DeepLynx system after ingestion. DeepLynx provides multiple query patterns for accessing records, edges, timeseries data, and graph relationships.

**Base URL:** `http://localhost:5000/api/v1/` (Docker environment)

---

## Table of Contents

1. [Record Queries](#record-queries)
2. [Edge Queries](#edge-queries)
3. [Timeseries Queries](#timeseries-queries)
4. [Graph Traversal](#graph-traversal)
5. [Advanced Query Building](#advanced-query-building)
6. [Common Query Patterns](#common-query-patterns)

---

## Record Queries

### 1. Get All Records

Retrieve all records within a project with optional filtering.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records`

**Path Parameters:**
- `organizationId` (integer, required) - Organization ID
- `projectId` (integer, required) - Project ID

**Query Parameters:**
- `dataSourceId` (integer, optional) - Filter by data source
- `fileType` (string, optional) - Filter by file extension (e.g., pdf, png, jpg)
- `hideArchived` (boolean, optional, default: true) - Hide archived records

**Response:** Array of `RecordResponseDto`

**Example:**
```bash
GET /organizations/1/projects/1/records?hideArchived=true
```

---

### 2. Get Single Record by ID

Retrieve a specific record by its database ID.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/{recordId}`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `recordId` (integer, required) - Record database ID

**Query Parameters:**
- `hideArchived` (boolean, optional, default: true)

**Response:** Single `RecordResponseDto`

**Example:**
```bash
GET /organizations/1/projects/1/records/123
```

---

### 3. Get Records Count

Get the total count of records in a project.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/count`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Response:** Integer count

---

### 4. Get Records by Tags

Filter records by associated tags.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/by-tags`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Query Parameters:**
- Tag filtering parameters (specific tags)

**Response:** Array of `RecordResponseDto`

---

### 5. Full Text Search for Records

Search across records using a text query.

**Endpoint:** `GET /organizations/{organizationId}/query/records`

**Path Parameters:**
- `organizationId` (integer, required)

**Query Parameters:**
- `userQuery` (string, optional) - Search phrase
- `projectIds` (array of integers, optional) - Filter by specific projects

**Response:** Array of `HistoricalRecordResponseDto`

**Example:**
```bash
GET /organizations/1/query/records?userQuery=RTD-2026&projectIds=1,2
```

---

### 6. Get Recent Records

Retrieve most recently created/updated records.

**Endpoint:** `GET /organizations/{organizationId}/query/recent`

**Path Parameters:**
- `organizationId` (integer, required)

**Query Parameters:**
- `projectIds` (array of integers, optional) - Filter by projects

**Response:** Array of `HistoricalRecordResponseDto` sorted by most recent

**Example:**
```bash
GET /organizations/1/query/recent?projectIds=1
```

---

### 7. Multi-Project Record Query

Retrieve records across multiple projects in an organization.

**Endpoint:** `GET /organizations/{organizationId}/query/multiproject`

**Path Parameters:**
- `organizationId` (integer, required)

**Query Parameters:**
- Project filtering parameters

**Response:** Records from multiple projects

---

### 8. Advanced Query Builder (POST)

Build complex queries with custom filters, operators, and conditions.

**Endpoint:** `POST /organizations/{organizationId}/query/records/advanced`

**Path Parameters:**
- `organizationId` (integer, required)

**Query Parameters:**
- `textSearch` (string, optional) - Full text search phrase
- `projectIds` (array of integers, optional) - Filter by projects

**Request Body:** Array of `CustomQueryRequestDto`

```json
[
  {
    "connector": "AND",
    "filter": "properties.accuracy_class",
    "operator": "eq",
    "value": "Class A",
    "json": null
  },
  {
    "connector": "AND",
    "filter": "properties.resistance_at_0C",
    "operator": "gt",
    "value": "100.0",
    "json": null
  }
]
```

**CustomQueryRequestDto Fields:**
- `connector` (string, nullable) - Logical connector: "AND" or "OR"
- `filter` (string, required) - Field to filter on (e.g., "properties.field_name", "name", "class_name")
- `operator` (string, required) - Comparison operator: "eq", "neq", "gt", "gte", "lt", "lte", "like", "in"
- `value` (string, nullable) - Value to compare against
- `json` (string, nullable) - JSON query for complex filtering

**Response:** Array of `HistoricalRecordResponseDto`

**Example:**
```bash
POST /organizations/1/query/records/advanced?textSearch=platinum&projectIds=1

Body:
[
  {
    "connector": "AND",
    "filter": "class_name",
    "operator": "eq",
    "value": "PlatinumElement"
  },
  {
    "connector": "AND",
    "filter": "properties.resistance_at_0C",
    "operator": "gte",
    "value": "99.9"
  }
]
```

---

### 9. Historical Records

Retrieve historical versions of records.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/historical`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Response:** Historical record snapshots

**Single Historical Record:**

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/historical/{recordId}`

**Get Record History:**

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/historical/{recordId}/history`

---

## Edge Queries

Edges represent relationships between records. Query edges to understand connections in your data graph.

### 1. Get All Edges

Retrieve all edges in a project.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Query Parameters:**
- `dataSourceId` (integer, optional) - Filter by data source
- `hideArchived` (boolean, optional, default: true)

**Response:** Array of `EdgeResponseDto`

---

### 2. Get Single Edge by ID

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges/{edgeId}`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `edgeId` (integer, required)

**Response:** Single `EdgeResponseDto`

---

### 3. Get Edge by Origin and Destination

Find edge connecting two specific records.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges/by-relationship`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Query Parameters:**
- `originId` (integer, optional) - Origin record ID
- `destinationId` (integer, optional) - Destination record ID
- `hideArchived` (boolean, optional, default: true)

**Response:** `EdgeResponseDto`

**Example:**
```bash
GET /organizations/1/projects/1/edges/by-relationship?originId=10&destinationId=5
```

---

### 4. Get Edges for a Specific Record

Retrieve all edges connected to a record (incoming and outgoing).

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/{recordId}/edges`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `recordId` (integer, required) - Record to query edges for

**Query Parameters:**
- `isOrigin` (boolean, optional) - Filter by direction:
  - `true` - Get edges where recordId is the origin (outgoing)
  - `false` - Get edges where recordId is the destination (incoming)
  - `null` - Get both incoming and outgoing edges
- `page` (integer, optional) - Page number for pagination
- `pageSize` (integer, optional, default: 20) - Number of results per page

**Response:** Array of `RelatedRecordsResponseDto`

**Example - Get all outgoing edges:**
```bash
GET /organizations/1/projects/1/records/42/edges?isOrigin=true&page=0&pageSize=20
```

**Example - Get all related records (both directions):**
```bash
GET /organizations/1/projects/1/records/42/edges
```

---

### 5. Historical Edges

Query historical edge data.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges/historical`

**Get Historical Edge by Relationship:**

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges/historical/by-relationship`

**Get Edge History:**

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/edges/historical/{edgeId}/history`

---

## Graph Traversal

### Get Graph Data for Record

Retrieve graph structure starting from a record with configurable depth.

**Endpoint:** `GET /organizations/{organizationId}/projects/{projectId}/records/{recordId}/graph`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `recordId` (integer, required) - Starting record for traversal

**Query Parameters:**
- `depth` (integer, optional) - Number of relationship levels to traverse
  - `1` - Direct neighbors only
  - `2` - Neighbors and their neighbors
  - `n` - N levels deep

**Response:** `GraphResponse` containing nodes and edges

**Example - Get 2 levels of graph:**
```bash
GET /organizations/1/projects/1/records/42/graph?depth=2
```

**Use Cases:**
- **Lineage Tracking**: Find all raw materials used in a sensor (traverse "madeFrom" backwards)
- **Impact Analysis**: Find all sensors affected by a raw material batch (traverse "madeFrom" forwards)
- **Process Flow**: Trace manufacturing steps from raw material → element → sensor → inspection

---

## Timeseries Queries

Query timeseries data stored in DuckDB tables.

### Query Timeseries with SQL

Execute SQL queries against timeseries data.

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/query`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `dataSourceId` (integer, required) - Data source containing timeseries

**Query Parameters:**
- `fileType` (string, optional) - Export format (e.g., csv, json)

**Request Body:** `TimeseriesQueryRequestDto`

```json
{
  "query": "SELECT * FROM timeseries_laser WHERE serial_number = 'RTD-2026-00001' AND timestamp > '2026-01-01' LIMIT 100"
}
```

**Response:** Query results as JSON or specified file format

**Example Queries:**

**Get all measurements for a sensor:**
```sql
SELECT * FROM timeseries_laser
WHERE serial_number = 'RTD-2026-00001'
ORDER BY timestamp DESC
```

**Aggregate statistics:**
```sql
SELECT
  serial_number,
  AVG(laser_measurement) as avg_measurement,
  MAX(laser_measurement) as max_measurement,
  MIN(laser_measurement) as min_measurement,
  COUNT(*) as measurement_count
FROM timeseries_laser
GROUP BY serial_number
```

**Time-range filtering:**
```sql
SELECT * FROM timeseries_laser
WHERE timestamp BETWEEN '2026-01-01' AND '2026-01-31'
AND serial_number LIKE 'RTD-2026-%'
```

---

## Advanced Query Building

### Query Builder Components

The advanced query builder allows complex filtering on record properties.

**Operators:**
- `eq` - Equal to
- `neq` - Not equal to
- `gt` - Greater than
- `gte` - Greater than or equal to
- `lt` - Less than
- `lte` - Less than or equal to
- `like` - Pattern matching (use % as wildcard)
- `in` - Value in list

**Filter Targets:**
- `name` - Record name
- `description` - Record description
- `class_name` - Class/type name
- `original_id` - Original identifier from source system
- `properties.field_name` - Any property within the properties JSON object

**Connectors:**
- `AND` - All conditions must be true
- `OR` - Any condition can be true

### Example: Complex Query

Find all RTD sensors with specific criteria:

```json
[
  {
    "connector": "AND",
    "filter": "class_name",
    "operator": "eq",
    "value": "RTDSensor"
  },
  {
    "connector": "AND",
    "filter": "properties.model_number",
    "operator": "eq",
    "value": "0068P41AAZAZ"
  },
  {
    "connector": "AND",
    "filter": "properties.ai_decision",
    "operator": "eq",
    "value": "ACCEPT"
  },
  {
    "connector": "OR",
    "filter": "properties.human_decision",
    "operator": "eq",
    "value": "ACCEPT"
  }
]
```

---

## Common Query Patterns

### 1. Find All Sensors Using a Specific Raw Material

**Step 1:** Get the raw material record
```bash
GET /organizations/1/query/records?userQuery=PT-001
```

**Step 2:** Traverse graph to find connected sensors
```bash
GET /organizations/1/projects/1/records/{rawMaterialId}/graph?depth=2
```

This returns:
- Raw Material (PT-001)
- → Platinum Elements made from PT-001
- → RTD Sensors assembled from those elements

---

### 2. Find All Inspections for a Sensor

**Step 1:** Get sensor record ID
```bash
GET /organizations/1/query/records?userQuery=RTD-2026-00001
```

**Step 2:** Get edges where sensor is origin
```bash
GET /organizations/1/projects/1/records/{sensorId}/edges?isOrigin=true
```

Filter results to `hasInspection` relationship to get all inspection records.

---

### 3. Find Failed Inspections Across All Sensors

**Advanced Query:**
```json
[
  {
    "connector": "AND",
    "filter": "class_name",
    "operator": "eq",
    "value": "Inspection"
  },
  {
    "connector": "AND",
    "filter": "properties.pass_fail",
    "operator": "eq",
    "value": "FAIL"
  }
]
```

---

### 4. Get Timeseries Data for Specific Process Step

**SQL Query:**
```sql
SELECT
  tl.*
FROM timeseries_laser tl
JOIN inspections i ON tl.serial_number = i.serial_number
WHERE i.process_step = 'Trimming'
ORDER BY tl.timestamp
```

---

### 5. Find High-Resistance Elements

**Advanced Query:**
```json
[
  {
    "connector": "AND",
    "filter": "class_name",
    "operator": "eq",
    "value": "PlatinumElement"
  },
  {
    "connector": "AND",
    "filter": "properties.resistance_at_0C",
    "operator": "gt",
    "value": "100.5"
  }
]
```

---

### 6. Get All Records from a Data Source

```bash
GET /organizations/1/projects/1/records?dataSourceId=5
```

---

### 7. Search Across Multiple Projects

```bash
GET /organizations/1/query/records?userQuery=Class%20A&projectIds=1,2,3
```

---

## Response Schemas

### RecordResponseDto

```typescript
{
  id: number                    // Database ID
  original_id: string           // Original source ID
  name: string                  // Record name
  description: string           // Description
  class_id: number             // Class ID
  class_name: string           // Class name
  properties: object           // Flexible JSON properties
  created_at: string           // ISO timestamp
  updated_at: string           // ISO timestamp
  archived: boolean            // Archive status
}
```

### EdgeResponseDto

```typescript
{
  id: number                    // Edge ID
  origin_id: number            // Origin record ID
  destination_id: number       // Destination record ID
  relationship_id: number      // Relationship type ID
  relationship_name: string    // Relationship name
  properties: object           // Edge properties
  created_at: string
  updated_at: string
}
```

### GraphResponse

```typescript
{
  nodes: RecordResponseDto[]   // All nodes in graph
  edges: EdgeResponseDto[]     // All edges in graph
}
```

---

## Authentication

All endpoints require authentication. Include token in Authorization header:

```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

---

## Error Responses

- `200 OK` - Success
- `401 Unauthorized` - Invalid or missing authentication token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Best Practices

1. **Use Pagination**: For large result sets, use `page` and `pageSize` parameters
2. **Filter Early**: Apply filters at query time rather than post-processing
3. **Use Graph Queries**: For relationship traversal, use graph endpoints instead of multiple queries
4. **Cache Results**: Cache frequently accessed data
5. **Use Advanced Queries**: For complex filtering, use the advanced query builder
6. **Index Properties**: Consider which properties you'll query frequently
7. **Use original_id**: When building edges, use original_id for easier mapping

---

## Query Performance Tips

- **Limit Depth**: Keep graph traversal depth to 2-3 levels when possible
- **Use Specific Filters**: More specific queries return faster
- **Paginate Results**: Don't fetch all records at once
- **Use Data Source Filtering**: Filter by dataSourceId when applicable
- **Archive Old Data**: Hide archived records with `hideArchived=true`

---

## Summary

DeepLynx provides multiple query patterns:

1. **Simple Queries**: Get records by ID, class, or data source
2. **Full Text Search**: Search across record content
3. **Advanced Queries**: Complex filtering with custom operators
4. **Edge Queries**: Find relationships between records
5. **Graph Traversal**: Navigate multi-level relationships
6. **Timeseries SQL**: Query time-series data with SQL
7. **Historical Queries**: Access historical versions of data

Choose the appropriate query method based on your use case:
- Direct ID lookup → Single record endpoint
- Text search → Full text search
- Complex filtering → Advanced query builder
- Relationships → Edge queries or graph traversal
- Time-series analysis → SQL queries
