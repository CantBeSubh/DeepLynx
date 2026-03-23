# DeepLynx API Data Ingestion Guide for RTD Dataset

## Overview

This guide documents all API endpoints and request body parameters required to ingest the RTD (Resistance Temperature Detector) manufacturing data into DeepLynx. The data includes:

- **raw_materials.csv** - Platinum raw materials
- **platinum_elements.csv** - Sensing elements
- **rtd_sensors.csv** - Complete sensor assemblies
- **inspections.csv** - Quality inspection data
- **timeseries_laser.csv** - Time-series measurement data
- **rtd_ontology.json** - Data model definition

## Prerequisites

Before ingesting data, you need:

1. **Organization ID** - The organization context
2. **Project ID** - A project within the organization
3. **Data Source ID** - A data source for tracking the origin of imported data
4. **Authentication Token** - OAuth token or API key

Base URL: `http://localhost:5000/api/v1/` (Docker environment)

---

## Data Ingestion Workflow

### Step 1: Create Classes (Metatypes)

Classes define the types of entities in your data model based on `rtd_ontology.json`.

#### 1.1 Create RawMaterial Class

**Endpoint:** `POST /organizations/{organizationId}/classes`

**Path Parameters:**
- `organizationId` (integer, required) - Organization ID

**Request Body:**
```json
{
  "name": "RawMaterial",
  "description": "Raw materials (platinum wire, ceramic, steel)",
  "uuid": null
}
```

**Response:** Returns `ClassResponseDto` with `id` field - save this as `rawMaterialClassId`

---

#### 1.2 Create PlatinumElement Class

**Endpoint:** `POST /organizations/{organizationId}/classes`

**Request Body:**
```json
{
  "name": "PlatinumElement",
  "description": "Core sensing element",
  "uuid": null
}
```

**Response:** Save the returned `id` as `platinumElementClassId`

---

#### 1.3 Create RTDSensor Class

**Endpoint:** `POST /organizations/{organizationId}/classes`

**Request Body:**
```json
{
  "name": "RTDSensor",
  "description": "Complete RTD assembly",
  "uuid": null
}
```

**Response:** Save the returned `id` as `rtdSensorClassId`

---

#### 1.4 Create Inspection Class (Optional - for inspections.csv)

**Endpoint:** `POST /organizations/{organizationId}/classes`

**Request Body:**
```json
{
  "name": "Inspection",
  "description": "Quality inspection records",
  "uuid": null
}
```

**Response:** Save the returned `id` as `inspectionClassId`

---

### Step 2: Create Relationships

Relationships define how classes are connected.

#### 2.1 Create "madeFrom" Relationship

**Endpoint:** `POST /organizations/{organizationId}/relationships`

**Path Parameters:**
- `organizationId` (integer, required)

**Request Body:**
```json
{
  "name": "madeFrom",
  "description": "Indicates what raw material an element is made from",
  "uuid": null,
  "origin_id": null,
  "destination_id": null
}
```

**Note:** `origin_id` and `destination_id` can be null when creating relationships at organization level. They will be specified when creating edges.

**Response:** Save the returned `id` as `madeFromRelationshipId`

---

#### 2.2 Create "assembledInto" Relationship

**Endpoint:** `POST /organizations/{organizationId}/relationships`

**Request Body:**
```json
{
  "name": "assembledInto",
  "description": "Indicates what sensor an element is assembled into",
  "uuid": null,
  "origin_id": null,
  "destination_id": null
}
```

**Response:** Save the returned `id` as `assembledIntoRelationshipId`

---

#### 2.3 Create "hasInspection" Relationship (Optional)

**Endpoint:** `POST /organizations/{organizationId}/relationships`

**Request Body:**
```json
{
  "name": "hasInspection",
  "description": "Links sensors to their inspection records",
  "uuid": null,
  "origin_id": null,
  "destination_id": null
}
```

**Response:** Save the returned `id` as `hasInspectionRelationshipId`

---

### Step 3: Create Records (Data Instances)

Records represent actual data instances of each class. Use bulk endpoints for efficiency.

#### 3.1 Create RawMaterial Records

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/records/bulk`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Query Parameters:**
- `dataSourceId` (integer, optional) - ID of the data source

**Request Body:** Array of CreateRecordRequestDto
```json
[
  {
    "name": "PT-001",
    "description": "Platinum raw material lot PT-001",
    "original_id": "PT-001",
    "class_name": "RawMaterial",
    "properties": {
      "material_id": "PT-001",
      "lot_number": "LOT-001",
      "supplier": "Heraeus",
      "purity_percent": 99.999
    }
  },
  {
    "name": "PT-002",
    "description": "Platinum raw material lot PT-002",
    "original_id": "PT-002",
    "class_name": "RawMaterial",
    "properties": {
      "material_id": "PT-002",
      "lot_number": "LOT-002",
      "supplier": "Heraeus",
      "purity_percent": 99.999
    }
  }
  // ... repeat for PT-003, PT-004, PT-005
]
```

**Field Descriptions:**
- `name` (string, required) - Human-readable name
- `description` (string, required) - Description of the record
- `original_id` (string, required) - Original identifier from source system (used for linking)
- `class_name` (string, optional) - Name of the class (alternative to class_id)
- `properties` (object, required) - JSON object with all properties from CSV columns

**Response:** Array of RecordResponseDto - save the mapping of `original_id` to `id`

---

#### 3.2 Create PlatinumElement Records

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/records/bulk`

**Request Body:** Array of 100 elements from platinum_elements.csv
```json
[
  {
    "name": "ELEM-00001",
    "description": "Platinum sensing element ELEM-00001",
    "original_id": "ELEM-00001",
    "class_name": "PlatinumElement",
    "properties": {
      "element_id": "ELEM-00001",
      "material_id": "PT-002",
      "resistance_at_0C": 100.03628683659085,
      "accuracy_class": "Class A"
    }
  },
  {
    "name": "ELEM-00002",
    "description": "Platinum sensing element ELEM-00002",
    "original_id": "ELEM-00002",
    "class_name": "PlatinumElement",
    "properties": {
      "element_id": "ELEM-00002",
      "material_id": "PT-005",
      "resistance_at_0C": 99.9787116183538,
      "accuracy_class": "Class A"
    }
  }
  // ... repeat for all 100 elements
]
```

---

#### 3.3 Create RTDSensor Records

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/records/bulk`

**Request Body:** Array of 100 sensors from rtd_sensors.csv
```json
[
  {
    "name": "RTD-2026-00001",
    "description": "RTD Sensor RTD-2026-00001",
    "original_id": "RTD-2026-00001",
    "class_name": "RTDSensor",
    "properties": {
      "serial_number": "RTD-2026-00001",
      "element_id": "ELEM-00001",
      "model_number": "0068P41AAZAZ",
      "ai_decision": "ACCEPT",
      "human_decision": "ACCEPT"
    }
  },
  {
    "name": "RTD-2026-00002",
    "description": "RTD Sensor RTD-2026-00002",
    "original_id": "RTD-2026-00002",
    "class_name": "RTDSensor",
    "properties": {
      "serial_number": "RTD-2026-00002",
      "element_id": "ELEM-00002",
      "model_number": "0068P41AAZAZ",
      "ai_decision": "ACCEPT",
      "human_decision": "ACCEPT"
    }
  }
  // ... repeat for all 100 sensors
]
```

---

#### 3.4 Create Inspection Records

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/records/bulk`

**Request Body:** Array of 400 inspections from inspections.csv (4 per sensor)
```json
[
  {
    "name": "INSP-RTD-2026-00001-0",
    "description": "Deposition inspection for RTD-2026-00001",
    "original_id": "INSP-RTD-2026-00001-0",
    "class_name": "Inspection",
    "properties": {
      "inspection_id": "INSP-RTD-2026-00001-0",
      "serial_number": "RTD-2026-00001",
      "process_step": "Deposition",
      "measured_value": 99.08796981507257,
      "pass_fail": "PASS"
    }
  },
  {
    "name": "INSP-RTD-2026-00001-1",
    "description": "Trimming inspection for RTD-2026-00001",
    "original_id": "INSP-RTD-2026-00001-1",
    "class_name": "Inspection",
    "properties": {
      "inspection_id": "INSP-RTD-2026-00001-1",
      "serial_number": "RTD-2026-00001",
      "process_step": "Trimming",
      "measured_value": 100.43205391679567,
      "pass_fail": "FAIL"
    }
  }
  // ... repeat for all 400 inspections
]
```

---

### Step 4: Create Edges (Relationships Between Records)

Edges connect record instances based on the relationships defined earlier.

#### 4.1 Create "madeFrom" Edges

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/edges/bulk`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)

**Query Parameters:**
- `dataSourceId` (integer, optional)

**Request Body:** Array of CreateEdgeRequestDto
```json
[
  {
    "origin_oid": "ELEM-00001",
    "destination_oid": "PT-002",
    "relationship_name": "madeFrom"
  },
  {
    "origin_oid": "ELEM-00002",
    "destination_oid": "PT-005",
    "relationship_name": "madeFrom"
  }
  // ... repeat for all 100 elements
]
```

**Field Descriptions:**
- `origin_oid` (string) - The `original_id` of the origin record (PlatinumElement)
- `destination_oid` (string) - The `original_id` of the destination record (RawMaterial)
- `relationship_name` (string) - Name of the relationship to use
- Alternatively, use `origin_id`, `destination_id`, and `relationship_id` with database IDs

---

#### 4.2 Create "assembledInto" Edges

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/edges/bulk`

**Request Body:**
```json
[
  {
    "origin_oid": "ELEM-00001",
    "destination_oid": "RTD-2026-00001",
    "relationship_name": "assembledInto"
  },
  {
    "origin_oid": "ELEM-00002",
    "destination_oid": "RTD-2026-00002",
    "relationship_name": "assembledInto"
  }
  // ... repeat for all 100 elements
]
```

---

#### 4.3 Create "hasInspection" Edges

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/edges/bulk`

**Request Body:**
```json
[
  {
    "origin_oid": "RTD-2026-00001",
    "destination_oid": "INSP-RTD-2026-00001-0",
    "relationship_name": "hasInspection"
  },
  {
    "origin_oid": "RTD-2026-00001",
    "destination_oid": "INSP-RTD-2026-00001-1",
    "relationship_name": "hasInspection"
  },
  {
    "origin_oid": "RTD-2026-00001",
    "destination_oid": "INSP-RTD-2026-00001-2",
    "relationship_name": "hasInspection"
  },
  {
    "origin_oid": "RTD-2026-00001",
    "destination_oid": "INSP-RTD-2026-00001-3",
    "relationship_name": "hasInspection"
  }
  // ... repeat for all 400 inspections
]
```

---

### Step 5: Upload Timeseries Data

For large timeseries files like `timeseries_laser.csv` (671KB), use the append endpoint.

#### 5.1 Append Timeseries File

**Endpoint:** `PATCH /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/append`

**Path Parameters:**
- `organizationId` (integer, required)
- `projectId` (integer, required)
- `dataSourceId` (integer, required)

**Query Parameters:**
- `tableName` (string, optional) - Name of the DuckDB table (e.g., "timeseries_laser")

**Request Body:** multipart/form-data
```
Content-Type: multipart/form-data

file: <binary content of timeseries_laser.csv>
```

**cURL Example:**
```bash
curl -X PATCH \
  "http://localhost:5000/api/v1/organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/append?tableName=timeseries_laser" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@timeseries_laser.csv"
```

---

#### 5.2 Alternative: Chunked Upload for Large Files

For very large files, use the chunked upload workflow:

**Step 1: Start Upload**

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/upload/start`

**Request Body:**
```json
{
  "fileName": "timeseries_laser.csv",
  "fileSize": 687616,
  "tableName": "timeseries_laser"
}
```

**Response:** Returns `{ "uploadId": "uuid-string" }`

---

**Step 2: Upload Chunks**

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/upload/chunk`

**Request Body:** multipart/form-data
```
Content-Type: multipart/form-data

uploadId: <uploadId from step 1>
chunkNumber: 0
chunk: <binary chunk data>
```

Repeat for each chunk (increment chunkNumber: 0, 1, 2, ...)

---

**Step 3: Complete Upload**

**Endpoint:** `POST /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/upload/complete`

**Request Body:**
```json
{
  "uploadId": "uuid-from-step-1",
  "totalChunks": 5
}
```

---

## Summary of API Calls

### Order of Operations:

1. **Create Classes** (3-4 calls)
   - RawMaterial
   - PlatinumElement
   - RTDSensor
   - Inspection (optional)

2. **Create Relationships** (2-3 calls)
   - madeFrom
   - assembledInto
   - hasInspection (optional)

3. **Create Records** (4 bulk calls)
   - 5 RawMaterial records
   - 100 PlatinumElement records
   - 100 RTDSensor records
   - 400 Inspection records

4. **Create Edges** (3 bulk calls)
   - 100 madeFrom edges
   - 100 assembledInto edges
   - 400 hasInspection edges

5. **Upload Timeseries** (1 call)
   - timeseries_laser.csv

**Total API Calls: ~13-15 calls** (using bulk operations)

---

## Authentication

All endpoints require authentication. Include the token in the Authorization header:

```
Authorization: Bearer YOUR_ACCESS_TOKEN
```

Or use OAuth client credentials:
1. GET `/oauth/authorize` with client_id and redirect_uri
2. POST `/oauth/exchange` with code, client_id, and client_secret

---

## Error Handling

All endpoints return standard HTTP status codes:
- `200 OK` - Success
- `401 Unauthorized` - Invalid or missing authentication token
- `403 Forbidden` - Insufficient permissions
- `500 Internal Server Error` - Server error

---

## Additional Notes

1. **Bulk Operations**: Use bulk endpoints (`/bulk`) whenever possible to reduce API calls and improve performance.

2. **Original ID vs Database ID**:
   - `original_id` is your source system identifier (e.g., "ELEM-00001")
   - `id` is the database-generated ID returned by DeepLynx
   - Use `original_id` in edges for easier mapping

3. **Class Reference**: Records can reference classes by either:
   - `class_id` (integer) - Database ID
   - `class_name` (string) - Class name

4. **Properties Object**: The `properties` field in CreateRecordRequestDto is a flexible JSON object that should contain all the data from your CSV columns.

5. **Data Source**: Create a data source first to track where your data came from. This is optional but recommended for data lineage.

6. **Project Setup**: Ensure you have an organization and project created before starting the ingestion process.

---

## Example Python Script Structure

```python
import requests
import pandas as pd
import json

BASE_URL = "http://localhost:5000/api/v1"
ORG_ID = 1
PROJECT_ID = 1
DATA_SOURCE_ID = 1
TOKEN = "your-access-token"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

# Step 1: Create Classes
def create_classes():
    classes = [
        {"name": "RawMaterial", "description": "Raw materials"},
        {"name": "PlatinumElement", "description": "Core sensing element"},
        {"name": "RTDSensor", "description": "Complete RTD assembly"},
        {"name": "Inspection", "description": "Quality inspection records"}
    ]

    class_ids = {}
    for cls in classes:
        response = requests.post(
            f"{BASE_URL}/organizations/{ORG_ID}/classes",
            headers=headers,
            json=cls
        )
        class_ids[cls["name"]] = response.json()["id"]

    return class_ids

# Step 2: Create Relationships
def create_relationships():
    relationships = [
        {"name": "madeFrom", "description": "Made from relationship"},
        {"name": "assembledInto", "description": "Assembled into relationship"},
        {"name": "hasInspection", "description": "Has inspection relationship"}
    ]

    rel_ids = {}
    for rel in relationships:
        response = requests.post(
            f"{BASE_URL}/organizations/{ORG_ID}/relationships",
            headers=headers,
            json=rel
        )
        rel_ids[rel["name"]] = response.json()["id"]

    return rel_ids

# Step 3: Create Records from CSV
def create_records_from_csv(csv_file, class_name):
    df = pd.read_csv(csv_file)

    records = []
    for _, row in df.iterrows():
        record = {
            "name": row[0],  # First column as name
            "description": f"{class_name} record",
            "original_id": str(row[0]),
            "class_name": class_name,
            "properties": row.to_dict()
        }
        records.append(record)

    response = requests.post(
        f"{BASE_URL}/organizations/{ORG_ID}/projects/{PROJECT_ID}/records/bulk",
        headers=headers,
        json=records,
        params={"dataSourceId": DATA_SOURCE_ID}
    )

    return response.json()

# Step 4: Create Edges
def create_edges(edges_data, relationship_name):
    edges = []
    for origin, destination in edges_data:
        edge = {
            "origin_oid": origin,
            "destination_oid": destination,
            "relationship_name": relationship_name
        }
        edges.append(edge)

    response = requests.post(
        f"{BASE_URL}/organizations/{ORG_ID}/projects/{PROJECT_ID}/edges/bulk",
        headers=headers,
        json=edges,
        params={"dataSourceId": DATA_SOURCE_ID}
    )

    return response.json()

# Step 5: Upload Timeseries
def upload_timeseries(file_path, table_name):
    with open(file_path, 'rb') as f:
        files = {'file': f}
        response = requests.patch(
            f"{BASE_URL}/organizations/{ORG_ID}/projects/{PROJECT_ID}/datasources/{DATA_SOURCE_ID}/timeseries/append",
            headers={"Authorization": f"Bearer {TOKEN}"},
            params={"tableName": table_name},
            files=files
        )

    return response.json()

# Main execution
if __name__ == "__main__":
    # 1. Create ontology
    class_ids = create_classes()
    rel_ids = create_relationships()

    # 2. Create records
    raw_materials = create_records_from_csv("raw_materials.csv", "RawMaterial")
    elements = create_records_from_csv("platinum_elements.csv", "PlatinumElement")
    sensors = create_records_from_csv("rtd_sensors.csv", "RTDSensor")
    inspections = create_records_from_csv("inspections.csv", "Inspection")

    # 3. Create edges (relationships)
    # ... build edge data from CSVs and create

    # 4. Upload timeseries
    upload_timeseries("timeseries_laser.csv", "timeseries_laser")

    print("Data ingestion complete!")
```

---

## Querying Data After Ingestion

After ingesting data, you can query it using:

**Query Records:**
```
GET /organizations/{organizationId}/query/records
POST /organizations/{organizationId}/query/records/advanced
```

**Query Timeseries:**
```
POST /organizations/{organizationId}/projects/{projectId}/datasources/{dataSourceId}/timeseries/query
```

---

This guide provides a complete reference for ingesting the RTD dataset into DeepLynx using the API.
