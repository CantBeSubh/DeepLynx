import sqlite3
import csv

conn = sqlite3.connect('rtd_data.db')
tables = ['raw_materials', 'platinum_elements', 'rtd_sensors',
          'inspections', 'timeseries_laser']

for table in tables:
    with open(f'{table}.csv', 'w', newline='') as f:
        cursor = conn.execute(f"SELECT * FROM {table}")
        writer = csv.writer(f)
        writer.writerow([d[0] for d in cursor.description])
        writer.writerows(cursor)
    print(f"✅ Exported {table}.csv")