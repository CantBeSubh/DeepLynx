# DeepLynx Agent Query Examples

This document contains example questions you can ask the agent to query the RTD manufacturing database.

## Basic Record Searches

### Finding Specific Items
- "Find sensor RTD-2026-00001"
- "Show me platinum element ELEM-00042"
- "Get details for raw material PT-003"
- "Search for inspection INS-00150"

### Searching by Keywords
- "Find all RTD sensors"
- "Show me platinum elements"
- "List raw materials"
- "Find inspections"

### Searching by Identifiers
- "Search for items from batch LOT-001"
- "Find all sensors with serial numbers starting with RTD-2026"
- "Show me elements from 2026"

## Quality and Inspection Queries

### Failed Items
- "Show me all failed inspections"
- "Find sensors that failed quality checks"
- "Which inspections have pass_fail status as FAIL?"
- "List all failed resistance calibrations"

### Specific Inspection Types
- "Show me all laser welding inspections"
- "Find resistance calibration results"
- "List mechanical assembly inspections"
- "Get all final quality check results"

### AI vs Human Decisions
- "Find sensors where AI and human decisions disagreed"
- "Show me items where AI rejected but human accepted"
- "List sensors with AI decision REJECT"
- "Find cases where human overrode AI decision"

## Material and Component Queries

### Material Properties
- "Show me materials with purity above 99.99%"
- "Find platinum from supplier Heraeus"
- "List materials from lot number LOT-002"
- "Show me the purest platinum batches"

### Element Specifications
- "Find Class A platinum elements"
- "Show me elements with resistance at 0C above 100 ohms"
- "List Class B accuracy elements"
- "Find high-resistance platinum elements"

### Sensor Models
- "Show me all sensors with model number 0068P41AAZAZ"
- "Find sensors by model type"
- "List different sensor models in the database"

## Lineage and Traceability

### Upstream Traceability (What Was Used?)
- "What raw material was sensor RTD-2026-00001 made from?"
- "Trace the origin of element ELEM-00050"
- "Show me the complete lineage for sensor RTD-2026-00025"
- "Which raw material batch was used for element ELEM-00010?"

### Downstream Traceability (What Was Made?)
- "Which sensors were made from raw material PT-001?"
- "Show me all sensors using platinum from lot LOT-003"
- "What elements were created from material PT-002?"
- "Which sensors contain element ELEM-00075?"

### Full Supply Chain
- "Trace the full manufacturing path from raw material PT-001 to final sensors"
- "Show me the complete supply chain for sensor RTD-2026-00050"
- "Map the lineage from raw platinum to finished sensor RTD-2026-00005"

## Relationship Queries

### Direct Connections
- "What is sensor RTD-2026-00010 made from?"
- "Show me all inspections for sensor RTD-2026-00001"
- "Which element was assembled into sensor RTD-2026-00030?"
- "List all relationships for element ELEM-00020"

### Graph Traversal
- "Show me all connected records for raw material PT-001"
- "Traverse the graph starting from sensor RTD-2026-00015"
- "Find all downstream items from element ELEM-00005"
- "Show me the complete graph for material PT-004"

## Impact Analysis

### Quality Issues
- "If raw material PT-001 is defective, which sensors are affected?"
- "Show me all sensors that could be impacted by element ELEM-00025"
- "Which final products used material from lot LOT-002?"
- "Find all sensors derived from a failed inspection batch"

### Batch Tracking
- "Show me everything made from supplier Heraeus materials"
- "Find all products from lot number LOT-001"
- "Which sensors share the same raw material source?"
- "List all items in the same production batch as RTD-2026-00010"

### Root Cause Analysis
- "If sensor RTD-2026-00040 failed, what could be the root cause in its supply chain?"
- "Trace back all components of failed sensor RTD-2026-00015"
- "Show me the inspection history for a failed sensor"

## Timeseries Data Queries

### Laser Welding Analysis
- "Show me laser power measurements for sensor RTD-2026-00001"
- "What was the average laser power during welding of RTD-2026-00010?"
- "Find all laser measurements where power exceeded 150W"
- "Show me the laser welding timeline for a specific sensor"

### Process Monitoring
- "Get timeseries data for laser welding process"
- "Show me temperature readings during sensor assembly"
- "Find anomalies in laser power data"
- "What were the laser settings when sensor RTD-2026-00005 was manufactured?"

### Quality Correlation
- "Compare laser power data for passed vs failed sensors"
- "Show me timeseries for sensors that failed inspection"
- "Did laser power fluctuations correlate with inspection failures?"

## Statistical and Aggregate Queries

### Counts and Summaries
- "How many sensors are in the database?"
- "Count all failed inspections"
- "How many Class A elements do we have?"
- "Show me statistics on inspection pass rates"

### Supplier Analysis
- "Which supplier provides the most materials?"
- "Show me all materials grouped by supplier"
- "Compare purity levels across suppliers"

### Model Performance
- "What percentage of sensors passed final inspection?"
- "Compare AI vs human decision accuracy"
- "Show me the failure rate by process step"
- "Which sensor model has the highest quality rate?"

## Complex Multi-Step Queries

### Combined Criteria
- "Find Class A elements from Heraeus supplier with resistance above 100 ohms"
- "Show me failed sensors that used platinum with purity below 99.99%"
- "List sensors where AI rejected but passed final inspection"

### Cross-Domain Analysis
- "Which high-purity materials resulted in the most sensor failures?"
- "Do Class A elements have better quality outcomes than Class B?"
- "Compare inspection results for different supplier materials"

### Comprehensive Reports
- "Give me a complete quality report for sensor RTD-2026-00001"
- "Show me everything: materials, elements, sensors, and inspections for lot LOT-001"
- "Create a traceability report from raw material PT-001 to all final products"

## Data Exploration

### Discovery
- "What types of records are in the database?"
- "Show me some example sensors"
- "What relationships exist between records?"
- "Give me an overview of the manufacturing data"

### Schema Understanding
- "What properties do RTD sensors have?"
- "Show me the structure of inspection records"
- "What fields are available for platinum elements?"
- "What information is stored about raw materials?"

## Edge Cases and Specific Scenarios

### Missing or Incomplete Data
- "Find sensors without inspection records"
- "Show me elements that weren't assembled into sensors"
- "List any orphaned records"

### Specific Time Periods
- "Show me sensors manufactured in 2026"
- "Find inspections from a specific date range"
- "What was produced using batch LOT-001?"

### Boundary Conditions
- "Find the highest purity platinum material"
- "Show me the sensor with the most inspections"
- "Which element has the highest resistance?"
- "Find materials from the earliest lot number"

## Advanced Graph Queries

### Multi-Hop Traversal
- "Show me all nodes within 2 hops of sensor RTD-2026-00001"
- "Traverse 3 levels deep from raw material PT-001"
- "Find all records connected within 4 relationships"

### Path Finding
- "Find the path from raw material PT-001 to sensor RTD-2026-00050"
- "Show me all possible paths from element ELEM-00010 to inspections"
- "Trace any route from supplier Heraeus to failed inspections"

### Network Analysis
- "Which raw material has the most downstream connections?"
- "Show me the most connected element"
- "Find hubs in the manufacturing graph"

## Notes

- All queries are natural language - you don't need exact syntax
- The agent will choose the appropriate DeepLynx tool based on your question
- For complex queries, the agent may break them into multiple steps
- If a query fails with advanced filters, the agent will fall back to simpler search methods
- Results are limited to the first 20 items to avoid overwhelming output - refine your search if needed
