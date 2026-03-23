import sqlite3
import random
from datetime import datetime, timedelta

# Create database
conn = sqlite3.connect('rtd_data.db')
c = conn.cursor()

# Create tables (use schema from Part 5B)
# ... (paste SQL CREATE TABLE statements)

# Generate 5 material batches
for i in range(1, 6):
    c.execute("INSERT INTO raw_materials VALUES (?, ?, ?, ?)",
              (f"PT-{i:03d}", f"LOT-{i:03d}", "Heraeus", 99.999))

# Generate 100 RTDs
for i in range(1, 101):
    # Material
    material_id = f"PT-{random.randint(1,5):03d}"

    # Element
    element_id = f"ELEM-{i:05d}"
    resistance = 100.0 + random.gauss(0, 0.05)
    c.execute("INSERT INTO platinum_elements VALUES (?, ?, ?, ?)",
              (element_id, material_id, resistance, "Class A"))

    # RTD
    serial = f"RTD-2026-{i:05d}"
    is_defect = random.random() < 0.10
    true_quality = "REJECT" if is_defect else "ACCEPT"
    ai_decision = "REJECT" if is_defect and random.random() < 0.95 else "ACCEPT"

    c.execute("INSERT INTO rtd_sensors VALUES (?, ?, ?, ?, ?)",
              (serial, element_id, "0068P41AAZAZ", ai_decision, true_quality))

    # Inspections (4 per RTD)
    for step_id, step in enumerate(["Deposition", "Trimming", "Assembly", "Calibration"]):
        c.execute("INSERT INTO inspections VALUES (?, ?, ?, ?, ?)",
                  (f"INSP-{serial}-{step_id}", serial, step,
                   random.uniform(99, 101), random.choice(["PASS", "FAIL"])))

    # Time-series (100 points of laser trimming per RTD)
    for t in range(100):
        c.execute("INSERT INTO timeseries_laser VALUES (?, ?, ?, ?)",
                  ((datetime.now() + timedelta(seconds=t)).isoformat(),
                   serial, 100.0 + 1.5 * (1 - t/100), random.uniform(8, 12)))

conn.commit()
print("✅ Generated 100 RTDs in rtd_data.db")