# DeepLynx Setup Guide for Project Managers

**Welcome!** This guide will help you (or your Claude Code agent) get DeepLynx running on your computer. It's written in plain language, so you can either follow along yourself or copy instructions to your AI agent.

## What is DeepLynx?

DeepLynx is a system that tracks manufacturing data and shows how different parts and materials are connected to each other. Think of it like a smart database that remembers not just the data, but also the relationships between items.

### What You'll Have After Setup

Once DeepLynx is running, you'll be able to:
- **Track products** from raw materials to finished goods
- **See relationships** between materials, parts, and sensors
- **Store quality data** like inspections and test results
- **Analyze measurements** like temperature readings over time
- **Query everything** using a web interface or AI agents

For example, if a sensor fails, you can trace it back to see which raw material batch it came from, all the manufacturing steps, and any quality issues along the way.

---

## Prerequisites

Before starting, make sure you have:
- **Docker Desktop** installed and running
- **Python 3** installed (for data import scripts)
- **A web browser** (Chrome, Firefox, Safari, etc.)

If you're not sure, ask your Claude Code agent: *"Check if I have Docker and Python installed"*

---

## For Your Claude Code Agent

> **Note to Project Managers:** The sections below are written so you can copy them and give them to your Claude Code agent. The agent will handle all the technical details while you verify that things are working.

---

## Task 1: Initial Setup

**Ask your agent:** *"Set up DeepLynx on my machine using Docker"*

### What Your Agent Will Do

Your agent will:
1. Check that Docker is installed and running
2. Navigate to the DeepLynx directory
3. Look for and handle any certificate issues (INL certificates are already in place)
4. Check if port 5000 is available (this is where DeepLynx will run)
5. Set up the Docker containers for the database, API, and web interface

### Success Check

Your agent should report that:
- All files and certificates are in place
- No port conflicts exist
- The system is ready to start

### If Something Goes Wrong

**Port 5000 is already in use:**
- This usually happens on macOS because AirPlay uses port 5000
- Ask your agent: *"Disable AirPlay on macOS or change DeepLynx to use port 5095"*
- **To disable AirPlay manually:** Open System Settings → General → AirDrop & Handoff → Turn off "AirPlay Receiver"

**Certificate errors:**
- The project includes INL certificates that should already be configured
- Your agent can verify they're in place
- If errors persist, ask your agent: *"Check the certificate configuration in the Dockerfiles"*

**Docker is not running:**
- Open Docker Desktop on your computer
- Wait for it to finish starting (you'll see a green icon)
- Then ask your agent to try again

### Technical Details for Your Agent

> **Agent:** You can find complete technical setup instructions in `README.md`. Key points:
> - The project uses Docker Compose with multi-stage builds
> - Certificate file `CAINLROOT_B64.crt` should exist in the repository root
> - The `docker-compose.yaml` file defines all services (UI, API, Docs, Database, MCP)
> - Port mappings: UI (3000), API (5000), Docs (3001), MCP (43656), Postgres (5432)
> - Recent fixes addressed INL certificate issues - certificates are now copied locally instead of downloaded

---

## Task 2: Start DeepLynx

**Ask your agent:** *"Start the DeepLynx services"*

### What Your Agent Will Do

Your agent will:
1. Run Docker Compose to start all DeepLynx services
2. Wait for services to become healthy
3. Verify that all 5 services are running:
   - **nx-postgres** (Database)
   - **server** (API Backend)
   - **ui** (Web Interface)
   - **docs** (API Documentation)
   - **mcp** (Model Context Protocol server)

This might take 1-3 minutes the first time as Docker downloads and builds everything.

### Success Check

Once your agent says the services are running, you can check for yourself:

1. **Open your web browser** and go to these URLs:
   - http://localhost:5000 - Should show the "Scalar" API documentation interface
   - http://localhost:3000 - Should show the DeepLynx web UI
   - http://localhost:3001 - Should show the documentation site

2. **What you should see:**
   - At http://localhost:5000: A clean, modern API documentation page with the DeepLynx logo
   - At http://localhost:3000: The DeepLynx web interface (may require login if authentication is enabled)
   - At http://localhost:3001: Documentation about using DeepLynx

If all three URLs load successfully, **congratulations!** DeepLynx is running.

### If Something Goes Wrong

**Services won't start:**
- Ask your agent: *"Check the Docker logs to see what's wrong"*
- Common issue: Database initialization takes time. Wait 30 seconds and try again.

**Browser shows "Can't connect" or "Site can't be reached":**
- Verify Docker Desktop shows all containers are running (green status)
- Ask your agent: *"Check if the DeepLynx containers are all running"*
- Wait a minute - sometimes services take time to fully start

**One service is running but others aren't:**
- Ask your agent: *"Restart the Docker Compose services"*
- This will clean up and restart everything

### Technical Details for Your Agent

> **Agent:** Use `docker compose up` or `docker compose up -d` for background mode. Monitor with `docker compose ps` and `docker compose logs`. The services have health checks defined. The entrypoint script in the server container handles database migrations automatically. If you need to rebuild after code changes, use `docker compose up --build`.

---

## Task 3: Load Demo Data

**Ask your agent:** *"Load the RTD manufacturing demo data into DeepLynx"*

### What Your Agent Will Do

Your agent will:
1. Create an Organization and Project in DeepLynx
2. Generate API credentials (key and secret)
3. Update the import script with those credentials
4. Run the Python import script that loads:
   - **5 raw material batches** (platinum wire)
   - **100 platinum sensing elements**
   - **100 RTD sensors** (complete assemblies)
   - **100 quality inspections**
   - **~10,000 time-series measurements** (laser welding data)
5. Create relationships showing which materials were used to make which sensors

This process takes about 2-5 minutes depending on your computer.

### Success Check

Your agent should report something like:
```
✅ Created Organization (ID: 1)
✅ Created Project (ID: 1)
✅ Generated API credentials
✅ Imported 5 raw materials
✅ Imported 100 platinum elements
✅ Imported 100 RTD sensors
✅ Imported 100 inspections
✅ Created 200 relationship edges
✅ Uploaded 10000 timeseries measurements
🎉 Import complete!
```

### Verify It Worked

1. Go to http://localhost:5000 in your browser
2. Navigate to **Query** → **Search Records** (in the left menu)
3. Search for: `RTD-2026-00001`
4. You should see a sensor record with properties like:
   - Serial number: RTD-2026-00001
   - Model number
   - AI decision (ACCEPT or REJECT)
   - Human decision

If you can find this sensor, **the data loaded successfully!**

### If Something Goes Wrong

**"Authentication failed" or "Invalid credentials":**
- The import script needs valid API credentials
- Ask your agent: *"Generate new API credentials and update the import script"*

**"Can't find the import script":**
- The script is at `rtd/import_to_deeplynx.py`
- Ask your agent: *"Navigate to the rtd directory and check if the import script exists"*

**"Python module not found" or "requests module missing":**
- Ask your agent: *"Install the Python requests library"*
- The agent will run: `pip install requests` or `pip3 install requests`

**Import runs but no data appears:**
- Check that the Organization ID and Project ID in the script are correct (usually both are `1`)
- Ask your agent: *"Verify the organization and project IDs in the import script"*

### Technical Details for Your Agent

> **Agent:** The detailed step-by-step process is in `rtd/GETTING_STARTED.md`. Key steps:
> 1. Use Scalar UI (http://localhost:5000) or cURL to create organization and project
> 2. Generate API token via POST to `/api/v1/organizations/{orgId}/projects/{projectId}/tokens`
> 3. Update `rtd/import_to_deeplynx.py` with the credentials (lines 10-15)
> 4. Ensure Python requests library is installed: `pip install requests`
> 5. Run: `cd rtd && python import_to_deeplynx.py`
> 6. The script uses the ontology in `rtd/rtd_ontology.json` to create classes and relationships
> 7. CSV files in the rtd/ directory contain the actual data
> 8. Timeseries data goes into a DuckDB table accessible via SQL queries

---

## Task 4: Generate Synthetic Data

**Ask your agent:** *"Generate additional synthetic RTD sensor data"*

### What Your Agent Will Do

Your agent will:
1. Run the synthetic data generation script
2. Create additional RTD sensors with:
   - Random material assignments
   - Realistic resistance measurements
   - Some sensors marked as defective (about 10%)
   - Quality inspection records
   - Time-series laser trimming data
3. Export the data to a SQLite database or CSV files
4. Optionally import it into DeepLynx

### Success Check

Your agent should report:
```
✅ Generated 100 RTDs in rtd_data.db
```

Or if importing into DeepLynx:
```
✅ Generated 100 new sensors
✅ Imported into DeepLynx
```

### Customization Options

If you want to customize the synthetic data, you can tell your agent:
- *"Generate 50 sensors instead of 100"*
- *"Make 20% of sensors defective instead of 10%"*
- *"Use different material batch IDs"*

Your agent can modify the generation script parameters to match your needs.

### Verify It Worked

1. Go to http://localhost:5000
2. Search for records with class "RTDSensor"
3. You should see more sensors than before (100 from demo + however many you generated)

### If Something Goes Wrong

**"Script not found":**
- The generation script is at `rtd/generate_synthetic_rtds.py`
- Ask your agent: *"Check if the synthetic data generation script exists"*

**"Database error" or "Table already exists":**
- The script creates a new SQLite database
- Ask your agent: *"Delete the old rtd_data.db file and try again"*

### Technical Details for Your Agent

> **Agent:** The generation script is at `rtd/generate_synthetic_rtds.py`. It:
> - Uses Python's random library to generate realistic values
> - Creates SQLite database with tables matching the DeepLynx schema
> - Generates correlated data (e.g., defective sensors have different resistance patterns)
> - Can be modified to change: number of sensors, defect rate, batch IDs, measurement ranges
> - To modify parameters, edit the script before running
> - To import generated data into DeepLynx, you can either:
>   1. Export to CSV and use the import script, or
>   2. Write a custom import script that reads from the SQLite database
> - The script is at approximately 50 lines and easy to customize

---

## Verify Everything Works

Here's a simple checklist you can follow:

### ✅ DeepLynx Services

- [ ] http://localhost:5000 loads (API documentation)
- [ ] http://localhost:3000 loads (Web UI)
- [ ] http://localhost:3001 loads (Documentation)

### ✅ Demo Data

- [ ] Can search for "RTD-2026-00001" and find a sensor
- [ ] Can search for "PT-001" and find a raw material
- [ ] Can see relationships between materials and sensors

### ✅ Synthetic Data (if generated)

- [ ] Additional sensors appear in search results
- [ ] Total sensor count increased

### Quick Test Query

Ask your agent to test the system with this query:

**Agent:** *"Search for all RTD sensors that were marked as REJECT"*

This will verify that:
- The search functionality works
- Data is properly indexed
- Relationships are correctly established

---

## What's Next?

Now that DeepLynx is running with data, you can:

### Explore the Data

**Using the Scalar Interface (http://localhost:5000):**
1. Browse all available API endpoints in the left menu
2. Try different queries:
   - Search for specific sensors
   - Get sensor details
   - Find relationships between parts
   - Query time-series measurements

**Using the Web UI (http://localhost:3000):**
- View data in a more visual format
- Create custom dashboards
- Manage users and permissions (if authentication is enabled)

### Connect the Gordian AI Agent

DeepLynx can be queried by AI agents like Gordian. If you want to set that up, ask your agent:

*"Help me connect the Gordian AI agent to this DeepLynx instance"*

Your agent will need to configure:
- DEEPLYNX_BASE_URL=http://localhost:5000/api/v1
- DEEPLYNX_ORGANIZATION_ID=1
- DEEPLYNX_PROJECT_ID=1
- Your API key and secret

Once connected, you can ask Gordian questions like:
- "Show me all sensors made from platinum batch PT-001"
- "Which sensors failed quality inspection?"
- "What's the average resistance for Class A elements?"
- "Trace the manufacturing lineage of sensor RTD-2026-00050"

### Learn More

- **Query Guide:** `rtd/DEEPLYNX_QUERY_GUIDE.md` - Advanced search patterns
- **Example Queries:** `rtd/EXAMPLE_QUERIES.md` - Sample queries you can try
- **API Reference:** `rtd/API_INGESTION_GUIDE.md` - Complete API documentation
- **Developer Guide:** `README.md` - For developers who want to modify the code

---

## Quick Reference for Your Agent

> **Agent:** This section contains key information for executing the tasks above.

### Important File Paths

| File | Purpose |
|------|---------|
| `docker-compose.yaml` | Defines all DeepLynx services |
| `README.md` | Technical setup guide for developers |
| `rtd/GETTING_STARTED.md` | Detailed data loading guide |
| `rtd/import_to_deeplynx.py` | Script to import demo data |
| `rtd/generate_synthetic_rtds.py` | Script to generate test data |
| `rtd/rtd_ontology.json` | Data model definition |
| `rtd/*.csv` | Demo data files |
| `.env_sample` | Environment variable template |

### Common Commands

| Task | Command |
|------|---------|
| Start DeepLynx | `docker compose up` |
| Start in background | `docker compose up -d` |
| Stop DeepLynx | `docker compose down` |
| View logs | `docker compose logs` |
| Rebuild after changes | `docker compose up --build` |
| Check service status | `docker compose ps` |
| Import demo data | `cd rtd && python import_to_deeplynx.py` |
| Generate synthetic data | `cd rtd && python generate_synthetic_rtds.py` |

### Important URLs

| Service | URL | Purpose |
|---------|-----|---------|
| API Docs | http://localhost:5000 | Interactive API documentation (Scalar) |
| Web UI | http://localhost:3000 | DeepLynx web interface |
| Documentation | http://localhost:3001 | User documentation |
| MCP Server | http://localhost:43656 | Model Context Protocol server |
| Database | localhost:5432 | PostgreSQL (internal use) |

### Environment Variables (for import script)

```python
DEEPLYNX_URL = "http://localhost:5000/api/v1"
ORG_ID = 1
PROJECT_ID = 1
API_KEY = "your-api-key-here"
API_SECRET = "your-api-secret-here"
```

### DeepLynx Data Model

**Classes (Entity Types):**
- RawMaterial - Platinum wire batches
- PlatinumElement - Sensing elements
- RTDSensor - Complete RTD assemblies
- Inspection - Quality inspection records

**Relationships:**
- madeFrom: PlatinumElement → RawMaterial
- assembledInto: PlatinumElement → RTDSensor
- inspectedBy: RTDSensor → Inspection

**Timeseries Tables:**
- timeseries_laser - Laser welding measurements

---

## Troubleshooting

### Port Conflicts

**Problem:** "Port 5000 is already in use"

**Solution:**
- On macOS: Disable AirPlay Receiver in System Settings → General → AirDrop & Handoff
- Or change DeepLynx to use port 5095 by editing `docker-compose.yaml`

**How to check what's using port 5000:**
Ask your agent: *"Check what process is using port 5000"*
(Agent will run: `lsof -i :5000` on macOS/Linux or `netstat -ano | findstr :5000` on Windows)

### Certificate Issues

**Problem:** "Certificate errors during Docker build"

**Solution:**
- The INL certificate file should already be at the repository root
- The recent fix changed Dockerfiles to copy the certificate locally instead of downloading it
- If errors persist, ask your agent: *"Verify the CAINLROOT_B64.crt file exists"*

### Docker Issues

**Problem:** "Docker containers won't start"

**Common causes:**
1. Docker Desktop is not running - Open Docker Desktop
2. Not enough resources - Increase Docker memory limit to 4GB+ in Docker settings
3. Old containers are in the way - Ask your agent: *"Clean up old Docker containers"*

**To completely reset:**
```
docker compose down -v
docker system prune -af
docker compose up --build
```

### Python/Import Issues

**Problem:** "ModuleNotFoundError: No module named 'requests'"

**Solution:**
Ask your agent: *"Install the Python requests library"*
(Agent will run: `pip install requests` or `pip3 install requests`)

**Problem:** "Import script fails with authentication error"

**Solution:**
1. Verify DeepLynx is running
2. Check organization ID and project ID are correct
3. Generate new API credentials
4. Update the import script with new credentials

### Data Issues

**Problem:** "Data imported but can't find records"

**Solutions:**
1. Verify import completed successfully (check for ✅ messages)
2. Make sure you're searching in the correct organization/project
3. Check the search query is correct (e.g., "RTD-2026-00001")
4. Ask your agent: *"Verify the database contains data"*

**Problem:** "Too much data / Want to start fresh"

**Solution:**
To reset the database:
```
docker compose down -v
docker compose up
```
Then re-run the import script.

---

## FAQ

### Do I need to know how to code?

No! This guide is designed so you can either ask your Claude Code agent to handle everything, or follow along with simple checks in your web browser.

### How long does setup take?

- First time: 5-15 minutes (Docker needs to download and build images)
- After that: 30-60 seconds to start services
- Data import: 2-5 minutes

### Is this safe to run on my computer?

Yes. DeepLynx runs entirely in Docker containers on your local machine. It doesn't send any data externally and will only use the ports specified (3000, 5000, 5001, 43656).

### Can I run this alongside other projects?

Yes, as long as those projects don't use the same ports. If you have port conflicts, you can change DeepLynx's ports in `docker-compose.yaml`.

### What if I need to stop DeepLynx?

Ask your agent: *"Stop the DeepLynx services"*

Or do it yourself:
1. Open Terminal
2. Navigate to the DeepLynx directory
3. Run: `docker compose down`

### Can I access this from other computers?

By default, DeepLynx only runs on your computer (localhost). To make it accessible on your network, you'll need to:
1. Ask your agent: *"Configure DeepLynx to be accessible on my local network"*
2. Update firewall settings to allow incoming connections
3. Share the URLs (using your computer's IP instead of localhost)

### Where is my data stored?

Your data is stored in Docker volumes on your computer. Even if you stop the containers, the data persists. To completely remove data, ask your agent: *"Delete the DeepLynx Docker volumes"*

---

## Getting Help

### Ask Your Agent

Your Claude Code agent has access to all the technical documentation and can help with:
- Troubleshooting errors
- Modifying configurations
- Running custom queries
- Generating reports
- Explaining technical concepts

### Documentation

- This file: `start_here.md` - You are here!
- Technical setup: `README.md`
- Data loading: `rtd/GETTING_STARTED.md`
- API reference: `rtd/API_INGESTION_GUIDE.md`
- Query guide: `rtd/DEEPLYNX_QUERY_GUIDE.md`

### Support

For bugs or feature requests:
- **INL users:** Use INL's Jira instance
- **External users:** Create an issue on GitHub

---

**That's it!** You now have everything you need to get DeepLynx running with demo data. Remember, when in doubt, just ask your Claude Code agent for help - it has access to all the technical details and can handle the complex parts for you.

Happy exploring! 🚀
