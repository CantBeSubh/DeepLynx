import requests
import csv
import json
import sys
from typing import Dict, List, Any

# ========================================
# Configuration
# ========================================
DEEPLYNX_URL = "http://localhost:5000/api/v1"
ORG_ID = 1
PROJECT_ID = 1

# API Credentials
API_KEY = "3-N5DStDAcxm1vwBk5qIgBLZTh2POBlS1iKHq7q5W__X8dova54Hd06wObTh_b28WaXpvQDx72CLj1i4Y0Z0rQ"
API_SECRET = "GwHOzUqsnXkHQ1NPRQn62srAwH5WGEhN_BBzhWE3cpMT4HAuCZDeLnVW-k48umIarNPIE05Ex5T-SFFu5htQrg"

# Global variables
JWT_TOKEN = None
DATA_SOURCE_ID = None
RECORD_ID_MAP = {}  # Maps original_id -> database id

# ========================================
# Helper Functions
# ========================================


def authenticate() -> str:
    """Generate JWT token from API key and secret."""
    print("🔐 Authenticating...")

    response = requests.post(
        f"{DEEPLYNX_URL}/oauth/tokens",
        json={"apiKey": API_KEY, "apiSecret": API_SECRET, "expirationMinutes": 120},
    )

    if response.status_code != 200:
        print(f"❌ Authentication failed: {response.status_code}")
        print(response.text)
        sys.exit(1)

    # The API returns the JWT token as plain text, not JSON
    token = response.text.strip()
    print(f"✅ Authentication successful")
    return token


def api_request(method: str, endpoint: str, **kwargs) -> requests.Response:
    """Make authenticated API request with error handling."""
    headers = kwargs.pop("headers", {})
    headers["Authorization"] = f"Bearer {JWT_TOKEN}"

    if "json" in kwargs:
        headers["Content-Type"] = "application/json"

    url = f"{DEEPLYNX_URL}{endpoint}"
    response = requests.request(method, url, headers=headers, **kwargs)

    if response.status_code not in [200, 201]:
        print(f"❌ API Error ({method} {endpoint}): {response.status_code}")
        print(response.text)

    return response


# ========================================
# Step 0: Create Data Source
# ========================================


def create_data_source() -> int:
    """Create or get existing data source for tracking data origin."""
    print("\n📊 Creating/getting data source...")

    # First, try to get existing data sources
    response = api_request("GET", f"/projects/{PROJECT_ID}/datasources")

    if response.status_code == 200:
        data_sources = response.json()
        # Check if "RTD Dataset" already exists
        for ds in data_sources:
            if ds.get("name") == "RTD Dataset":
                print(f"✅ Found existing data source: ID {ds['id']}")
                return ds["id"]

    # If not found, create a new one
    response = api_request(
        "POST",
        f"/projects/{PROJECT_ID}/datasources",
        json={
            "name": "RTD Dataset",
            "description": "RTD manufacturing data including raw materials, elements, sensors, inspections, and timeseries",
            "type": "standard",
        },
    )

    if response.status_code in [200, 201]:
        data_source_id = response.json().get("id")
        print(f"✅ Data source created: ID {data_source_id}")
        return data_source_id
    else:
        print("❌ Failed to create data source")
        sys.exit(1)


# ========================================
# Step 1: Create Classes
# ========================================


def create_classes() -> Dict[str, int]:
    """Create all classes (formerly metatypes) for the RTD data model."""
    print("\n🏗️  Creating classes...")

    classes = [
        {
            "name": "RawMaterial",
            "description": "Raw materials (platinum wire, ceramic, steel)",
            "uuid": None,
        },
        {
            "name": "PlatinumElement",
            "description": "Core sensing element",
            "uuid": None,
        },
        {"name": "RTDSensor", "description": "Complete RTD assembly", "uuid": None},
        {
            "name": "Inspection",
            "description": "Quality inspection records",
            "uuid": None,
        },
    ]

    response = api_request(
        "POST", f"/organizations/{ORG_ID}/classes/bulk", json=classes
    )

    class_ids = {}
    if response.status_code in [200, 201]:
        results = response.json()
        for cls in results:
            class_ids[cls["name"]] = cls["id"]
            print(f"  ✓ Created class: {cls['name']} (ID: {cls['id']})")
    else:
        print("❌ Failed to create classes")
        sys.exit(1)

    return class_ids


# ========================================
# Step 2: Create Relationships
# ========================================


def create_relationships() -> Dict[str, int]:
    """Create relationships between classes."""
    print("\n🔗 Creating relationships...")

    relationships = [
        {
            "name": "madeFrom",
            "description": "Indicates what raw material an element is made from",
            "uuid": None,
            "origin_id": None,
            "destination_id": None,
        },
        {
            "name": "assembledInto",
            "description": "Indicates what sensor an element is assembled into",
            "uuid": None,
            "origin_id": None,
            "destination_id": None,
        },
        {
            "name": "hasInspection",
            "description": "Links sensors to their inspection records",
            "uuid": None,
            "origin_id": None,
            "destination_id": None,
        },
    ]

    response = api_request(
        "POST", f"/organizations/{ORG_ID}/relationships/bulk", json=relationships
    )

    rel_ids = {}
    if response.status_code in [200, 201]:
        results = response.json()
        for rel in results:
            rel_ids[rel["name"]] = rel["id"]
            print(f"  ✓ Created relationship: {rel['name']} (ID: {rel['id']})")
    else:
        print("❌ Failed to create relationships")
        sys.exit(1)

    return rel_ids


# ========================================
# Step 3: Create Records from CSV Files
# ========================================


def create_raw_material_records() -> List[Dict]:
    """Import raw materials from CSV."""
    print("\n📦 Importing raw materials...")

    records = []
    with open("raw_materials.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            record = {
                "name": row["material_id"],
                "description": f"Platinum raw material lot {row['material_id']}",
                "original_id": row["material_id"],
                "class_name": "RawMaterial",
                "properties": {
                    "material_id": row["material_id"],
                    "lot_number": row["lot_number"],
                    "supplier": row["supplier"],
                    "purity_percent": float(row["purity_percent"]),
                },
            }
            records.append(record)

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/records/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=records,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        # Store mapping of originalId -> database id
        for record in results:
            RECORD_ID_MAP[record["originalId"]] = record["id"]
        print(f"  ✓ Created {len(results)} raw material records")
        return results
    else:
        print("❌ Failed to create raw material records")
        sys.exit(1)


def create_platinum_element_records() -> List[Dict]:
    """Import platinum elements from CSV."""
    print("\n⚡ Importing platinum elements...")

    records = []
    with open("platinum_elements.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            record = {
                "name": row["element_id"],
                "description": f"Platinum sensing element {row['element_id']}",
                "original_id": row["element_id"],
                "class_name": "PlatinumElement",
                "properties": {
                    "element_id": row["element_id"],
                    "material_id": row["material_id"],
                    "resistance_at_0C": float(row["resistance_at_0C"]),
                    "accuracy_class": row["accuracy_class"],
                },
            }
            records.append(record)

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/records/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=records,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        # Store mapping of originalId -> database id
        for record in results:
            RECORD_ID_MAP[record["originalId"]] = record["id"]
        print(f"  ✓ Created {len(results)} platinum element records")
        return results
    else:
        print("❌ Failed to create platinum element records")
        sys.exit(1)


def create_rtd_sensor_records() -> List[Dict]:
    """Import RTD sensors from CSV."""
    print("\n🔧 Importing RTD sensors...")

    records = []
    with open("rtd_sensors.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            record = {
                "name": row["serial_number"],
                "description": f"RTD Sensor {row['serial_number']}",
                "original_id": row["serial_number"],
                "class_name": "RTDSensor",
                "properties": {
                    "serial_number": row["serial_number"],
                    "element_id": row["element_id"],
                    "model_number": row["model_number"],
                    "ai_decision": row["ai_decision"],
                    "human_decision": row["human_decision"],
                },
            }
            records.append(record)

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/records/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=records,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        # Store mapping of originalId -> database id
        for record in results:
            RECORD_ID_MAP[record["originalId"]] = record["id"]
        print(f"  ✓ Created {len(results)} RTD sensor records")
        return results
    else:
        print("❌ Failed to create RTD sensor records")
        sys.exit(1)


def create_inspection_records() -> List[Dict]:
    """Import inspection records from CSV."""
    print("\n🔍 Importing inspection records...")

    records = []
    with open("inspections.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            record = {
                "name": row["inspection_id"],
                "description": f"{row['process_step']} inspection for {row['serial_number']}",
                "original_id": row["inspection_id"],
                "class_name": "Inspection",
                "properties": {
                    "inspection_id": row["inspection_id"],
                    "serial_number": row["serial_number"],
                    "process_step": row["process_step"],
                    "measured_value": float(row["measured_value"]),
                    "pass_fail": row["pass_fail"],
                },
            }
            records.append(record)

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/records/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=records,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        # Store mapping of originalId -> database id
        for record in results:
            RECORD_ID_MAP[record["originalId"]] = record["id"]
        print(f"  ✓ Created {len(results)} inspection records")
        return results
    else:
        print("❌ Failed to create inspection records")
        sys.exit(1)


# ========================================
# Step 4: Create Edges (Relationships)
# ========================================


def create_made_from_edges():
    """Create edges linking platinum elements to raw materials."""
    print("\n🔗 Creating 'madeFrom' edges...")

    edges = []
    with open("platinum_elements.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            # Look up database IDs from original IDs
            origin_id = RECORD_ID_MAP.get(row["element_id"])
            destination_id = RECORD_ID_MAP.get(row["material_id"])

            if origin_id and destination_id:
                edge = {
                    "origin_id": origin_id,
                    "destination_id": destination_id,
                    "relationship_name": "madeFrom",
                }
                edges.append(edge)
            else:
                print(
                    f"  ⚠️  Warning: Could not find IDs for {row['element_id']} -> {row['material_id']}"
                )

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/edges/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=edges,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        print(f"  ✓ Created {len(results)} 'madeFrom' edges")
    else:
        print("❌ Failed to create 'madeFrom' edges")


def create_assembled_into_edges():
    """Create edges linking platinum elements to RTD sensors."""
    print("\n🔗 Creating 'assembledInto' edges...")

    edges = []
    with open("rtd_sensors.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            # Look up database IDs from original IDs
            origin_id = RECORD_ID_MAP.get(row["element_id"])
            destination_id = RECORD_ID_MAP.get(row["serial_number"])

            if origin_id and destination_id:
                edge = {
                    "origin_id": origin_id,
                    "destination_id": destination_id,
                    "relationship_name": "assembledInto",
                }
                edges.append(edge)
            else:
                print(
                    f"  ⚠️  Warning: Could not find IDs for {row['element_id']} -> {row['serial_number']}"
                )

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/edges/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=edges,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        print(f"  ✓ Created {len(results)} 'assembledInto' edges")
    else:
        print("❌ Failed to create 'assembledInto' edges")


def create_has_inspection_edges():
    """Create edges linking RTD sensors to their inspection records."""
    print("\n🔗 Creating 'hasInspection' edges...")

    edges = []
    with open("inspections.csv", "r") as f:
        reader = csv.DictReader(f)
        for row in reader:
            # Look up database IDs from original IDs
            origin_id = RECORD_ID_MAP.get(row["serial_number"])
            destination_id = RECORD_ID_MAP.get(row["inspection_id"])

            if origin_id and destination_id:
                edge = {
                    "origin_id": origin_id,
                    "destination_id": destination_id,
                    "relationship_name": "hasInspection",
                }
                edges.append(edge)
            else:
                print(
                    f"  ⚠️  Warning: Could not find IDs for {row['serial_number']} -> {row['inspection_id']}"
                )

    response = api_request(
        "POST",
        f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/edges/bulk",
        params={"dataSourceId": DATA_SOURCE_ID},
        json=edges,
    )

    if response.status_code in [200, 201]:
        results = response.json()
        print(f"  ✓ Created {len(results)} 'hasInspection' edges")
    else:
        print("❌ Failed to create 'hasInspection' edges")


# ========================================
# Step 5: Upload Timeseries Data
# ========================================


def upload_timeseries():
    """Upload timeseries data from CSV file."""
    print("\n📈 Uploading timeseries data...")

    with open("timeseries_laser.csv", "rb") as f:
        files = {"file": ("timeseries_laser.csv", f, "text/csv")}

        # Use POST /timeseries/upload to create a new table and upload data
        response = api_request(
            "POST",
            f"/organizations/{ORG_ID}/projects/{PROJECT_ID}/datasources/{DATA_SOURCE_ID}/timeseries/upload",
            files=files,
        )

        if response.status_code in [200, 201]:
            print("  ✓ Timeseries data uploaded successfully")
        else:
            print("❌ Failed to upload timeseries data")


# ========================================
# Main Execution
# ========================================


def main():
    """Main execution function."""
    global JWT_TOKEN, DATA_SOURCE_ID

    print("=" * 60)
    print("DeepLynx RTD Dataset Import")
    print("=" * 60)

    # Step 0: Authenticate
    JWT_TOKEN = authenticate()

    # Step 1: Create Data Source
    DATA_SOURCE_ID = create_data_source()

    # Step 2: Create Classes
    class_ids = create_classes()

    # Step 3: Create Relationships
    rel_ids = create_relationships()

    # Step 4: Import Records
    raw_materials = create_raw_material_records()
    elements = create_platinum_element_records()
    sensors = create_rtd_sensor_records()
    inspections = create_inspection_records()

    # Step 5: Create Edges
    create_made_from_edges()
    create_assembled_into_edges()
    create_has_inspection_edges()

    # Step 6: Upload Timeseries
    upload_timeseries()

    print("\n" + "=" * 60)
    print("✅ Import complete!")
    print("=" * 60)
    print(f"\nSummary:")
    print(f"  - Organization ID: {ORG_ID}")
    print(f"  - Project ID: {PROJECT_ID}")
    print(f"  - Data Source ID: {DATA_SOURCE_ID}")
    print(f"  - Classes: {len(class_ids)}")
    print(f"  - Relationships: {len(rel_ids)}")
    print(f"  - Records imported from 5 CSV files")
    print(f"  - Edges created for all relationships")
    print(f"  - Timeseries data uploaded")


if __name__ == "__main__":
    main()
