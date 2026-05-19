"""驗證全部 Seed Data 最終結果"""
import sqlite3, os

db_dir = r'D:\TRIO2026\Database'
for db_file in sorted(os.listdir(db_dir)):
    if not db_file.endswith('.db'):
        continue
    print(f'\n=== {db_file} ===')
    conn = sqlite3.connect(os.path.join(db_dir, db_file))
    cursor = conn.execute("SELECT name FROM sqlite_master WHERE type='table' AND name != 'sqlite_sequence' ORDER BY name")
    tables = cursor.fetchall()
    for t in tables:
        count = conn.execute(f'SELECT COUNT(*) FROM {t[0]}').fetchone()[0]
        print(f'  {t[0]:25s} {count:>5d} rows')
    conn.close()

# SystemConfig category breakdown
print('\n=== SystemConfig by Category ===')
conn = sqlite3.connect(os.path.join(db_dir, 'trio240plus_config.db'))
rows = conn.execute('SELECT Category, COUNT(*) FROM SystemConfig GROUP BY Category ORDER BY COUNT(*) DESC').fetchall()
for r in rows:
    print(f'  {r[0]:20s} {r[1]:>5d}')
print(f'  {"TOTAL":20s} {sum(r[1] for r in rows):>5d}')
conn.close()
