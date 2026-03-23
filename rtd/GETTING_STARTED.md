# RTD Demo Data - Getting Started Guide

This guide walks you through setting up DeepLynx and loading the RTD (Resistance Temperature Detector) manufacturing demo data.

## What You'll Get

After completing this guide, you'll have:
- A working DeepLynx instance with manufacturing data
- 100 RTD sensors with full lineage tracking
- Relationships showing raw materials → elements → sensors
- Quality inspection data
- Time-series laser welding measurements

## Prerequisites

1. Docker and Docker Compose installed
2. DeepLynx running at `http://localhost:5000` (or `http://localhost:5095`)
3. Python 3 with `requests` library

---

## Step 1: Start DeepLynx

```bash
cd /Users/subhr/Documents/github/DeepLynx
docker compose up
```

Wait for all services to start. You should see logs indicating the server is ready.

---

## Step 2: Access the API Documentation

Open your browser to:
- **Scalar API Docs**: http://localhost:5000 (or http://localhost:5095)

This interactive documentation lets you test all API endpoints.

---

## Step 3: Create Organization and Project

### Option A: Using Scalar UI (Recommended for First Time)

1. Go to http://localhost:5000
2. Navigate to **Organization** → **Create Organization**
3. Click "Test Request" with body:
   ```json
   {
     "name": "Demo Organization",
     "description": "RTD Manufacturing Demo"
   }
   ```
4. Save the returned `id` (should be `1`)

5. Navigate to **Project** → **Create Project**
6. Use organizationId `1` in the path parameter
7. Click "Test Request" with body:
   ```json
   {
     "name": "RTD Manufacturing",
     "description": "RTD sensor manufacturing and quality tracking"
   }
   ```
8. Save the returned `id` (should be `1`)

### Option B: Using cURL

```bash
# Create Organization
curl -X POST http://localhost:5000/api/v1/organizations \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Demo Organization",
    "description": "RTD Manufacturing Demo"
  }'

# Create Project (use the organization ID from above)
curl -X POST http://localhost:5000/api/v1/organizations/1/projects \
  -H "Content-Type: application/json" \
  -d '{
    "name": "RTD Manufacturing",
    "description": "RTD sensor manufacturing and quality tracking"
  }'
```

---

## Step 4: Generate API Key

### Using Scalar UI:

1. In Scalar, go to **Token** → **Create Token**
2. Use organizationId `1` and projectId `1` in path parameters
3. Click "Test Request" with body:
   ```json
   {
     "name": "RTD Import Key",
     "expirationMinutes": 43200
   }
   ```
4. Save the returned `key` and `secret`

### Using cURL:

```bash
curl -X POST http://localhost:5000/api/v1/organizations/1/projects/1/tokens \
  -H "Content-Type: application/json" \
  -d '{
    "name": "RTD Import Key",
    "expirationMinutes": 43200
  }'
```

---

## Step 5: Update Import Script

Edit `rtd/import_to_deeplynx.py`:

```python
# Update these lines (around line 10-13):
DEEPLYNX_URL = "http://localhost:5000/api/v1"  # or 5095 if you changed port
ORG_ID = 1  # From Step 3
PROJECT_ID = 1  # From Step 3

# Paste your API credentials from Step 4:
API_KEY = "your-api-key-here"
API_SECRET = "your-api-secret-here"
```

---

## Step 6: Install Python Dependencies

```bash
cd /Users/subhr/Documents/github/DeepLynx/rtd
pip install requests
```

Or if you use Python 3 explicitly:

```bash
pip3 install requests
```

---

## Step 7: Run the Import Script

```bash
python import_to_deeplynx.py
```

Or:

```bash
python3 import_to_deeplynx.py
```

### What the Script Does:

1. **Authenticates** with DeepLynx
2. **Creates a Data Source** named "RTD Dataset"
3. **Creates Classes** (RawMaterial, PlatinumElement, RTDSensor, Inspection)
4. **Creates Relationships** (madeFrom, assembledInto, inspectedBy)
5. **Imports Records** from CSV files:
   - 5 raw materials
   - 100 platinum elements
   - 100 RTD sensors
   - 100 inspections
6. **Creates Edges** linking records together
7. **Uploads Timeseries Data** (laser measurements)

### Expected Output:

```
🔐 Authenticating...
✅ Authentication successful

📊 Creating/getting data source...
✅ Found existing data source: ID 1

📋 Creating classes...
✅ Class created: RawMaterial (ID: 1)
✅ Class created: PlatinumElement (ID: 2)
✅ Class created: RTDSensor (ID: 3)
✅ Class created: Inspection (ID: 4)

🔗 Creating relationships...
✅ Relationship created: madeFrom (ID: 1)
✅ Relationship created: assembledInto (ID: 2)
✅ Relationship created: inspectedBy (ID: 3)

📤 Importing raw_materials.csv...
✅ Imported 5 raw materials

📤 Importing platinum_elements.csv...
✅ Imported 100 platinum elements

📤 Importing rtd_sensors.csv...
✅ Imported 100 RTD sensors

📤 Importing inspections.csv...
✅ Imported 100 inspections

🔗 Creating edges (relationships)...
✅ Created 200 edges

📊 Uploading timeseries data...
✅ Uploaded 10000 timeseries measurements

🎉 Import complete!
```

---

## Step 8: Verify the Data

### Check Records in Scalar:

1. Go to **Query** → **Search Records**
2. Try searching for:
   - `RTD-2026-00001` (a sensor)
   - `PT-001` (raw material)
   - `ELEM-00001` (element)

### Check Relationships:

1. Go to **Record** → **Get Record Edges**
2. Use a record ID to see its connections

### Query Timeseries:

1. Go to **Timeseries** → **Query Timeseries**
2. Use SQL like:
   ```sql
   SELECT * FROM timeseries_laser LIMIT 10
   ```

---

## Step 9: Connect Gordian (Optional)

If you want to use the Gordian AI agent with this data:

1. Start the Gordian API
2. Set environment variables:
   ```bash
   export DEEPLYNX_BASE_URL=http://localhost:5000/api/v1
   export DEEPLYNX_ORGANIZATION_ID=1
   export DEEPLYNX_PROJECT_ID=1
   export DEEPLYNX_API_KEY=your-api-key
   export DEEPLYNX_API_SECRET=your-api-secret
   export DEEPLYNX_DATA_SOURCE_ID=1
   ```

Now the AI agent can query your manufacturing data!

---

## Troubleshooting

### Import Script Fails with Authentication Error
- Check your API key and secret are correct
- Verify the DEEPLYNX_URL matches your running instance
- Make sure DeepLynx is running (`docker compose ps`)

### "Data Source Already Exists"
- This is fine! The script will reuse the existing data source

### Port Conflicts (5000 already in use)
- Disable AirPlay Receiver in macOS System Settings
- Or change DeepLynx port to 5095 in docker-compose.yaml

### Import Runs But No Data Appears
- Check the script output for errors
- Verify organization ID and project ID are correct
- Check DeepLynx logs: `docker compose logs server`

---

## Next Steps

- **Explore the data** using Scalar API documentation
- **Try example queries** in `EXAMPLE_QUERIES.md`
- **Connect Gordian AI** to ask questions about the manufacturing data
- **Read the detailed guide** in `API_INGESTION_GUIDE.md` for more API details
- **Query guide** in `DEEPLYNX_QUERY_GUIDE.md` for advanced searches

---

## Data Structure

### Classes (Entity Types)
- **RawMaterial** - Platinum wire batches (5 records)
- **PlatinumElement** - Sensing elements (100 records)
- **RTDSensor** - Complete sensors (100 records)
- **Inspection** - Quality checks (100 records)

### Relationships
- **madeFrom**: PlatinumElement → RawMaterial
- **assembledInto**: PlatinumElement → RTDSensor
- **inspectedBy**: RTDSensor → Inspection

### Properties

**RawMaterial:**
- material_id (e.g., "PT-001")
- lot_number
- supplier
- purity_percent

**PlatinumElement:**
- element_id (e.g., "ELEM-00001")
- resistance_at_0C
- accuracy_class ("Class A" or "Class B")

**RTDSensor:**
- serial_number (e.g., "RTD-2026-00001")
- model_number
- ai_decision (ACCEPT/REJECT)
- human_decision (ACCEPT/REJECT)

**Inspection:**
- inspection_id
- pass_fail (PASS/FAIL)
- notes

### Timeseries Data
- Table: `timeseries_laser`
- ~10,000 laser welding measurements per sensor
- Columns: timestamp, serial_number, laser_measurement

---

## Example Questions for Gordian AI

Once connected, try asking:
- "Show me all sensors made from platinum batch PT-001"
- "Find sensors that failed quality inspection"
- "What's the average resistance for Class A elements?"
- "Trace the full lineage of sensor RTD-2026-00050"
- "Which raw material batches produced the most failures?"
- "Show me the laser measurements for RTD-2026-00001"

---

## Files in This Directory

- **GETTING_STARTED.md** (this file) - Quick start guide
- **API_INGESTION_GUIDE.md** - Detailed API endpoint reference
- **DEEPLYNX_QUERY_GUIDE.md** - Advanced query patterns
- **EXAMPLE_QUERIES.md** - Example search queries
- **import_to_deeplynx.py** - Automated import script
- **generate_synthetic_rtds.py** - Generate sample data
- **export_to_csv.py** - Export data from SQLite
- **rtd_ontology.json** - Data model definition
- **CSV files** - Sample manufacturing data
- **rtd_data.db** - SQLite database (for reference)

---

Happy exploring! 🚀
