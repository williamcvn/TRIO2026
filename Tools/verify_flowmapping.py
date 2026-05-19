import sqlite3
conn = sqlite3.connect(r'D:\TRIO2026\Database\trio240plus_main.db')

cols = conn.execute('PRAGMA table_info(FlowMapping)').fetchall()
print('=== FlowMapping Columns ===')
for c in cols:
    print(f'  {c[1]:25s} {c[2]:10s} NOT_NULL={c[3]}  PK={c[5]}')

print()
rows = conn.execute('SELECT * FROM FlowMapping ORDER BY Id').fetchall()
print(f'=== FlowMapping Data ({len(rows)} rows) ===')
header = f'  {"Id":>3s}  {"FlowCode":10s}  {"BuiltInFlowName":35s}  {"Elut":5s} {"Load":6s} {"Sample":12s} {"Layout":14s} {"ExtTime":>7s}'
print(header)
print('  ' + '-' * 100)
for r in rows:
    print(f'  {r[0]:3d}  {r[1]:10s}  {r[2]:35s}  {str(r[3]):5s} {str(r[4]):6s} {str(r[5]):12s} {str(r[6]):14s} {str(r[7]):>7s}')

conn.close()
