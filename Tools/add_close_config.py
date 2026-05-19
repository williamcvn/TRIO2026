"""Add close method configuration to SystemConfig DB"""
import sqlite3
import datetime

conn = sqlite3.connect(r'D:\TRIO2026\Database\trio240plus_config.db')
now = datetime.datetime.now(datetime.UTC).isoformat() + 'Z'

configs = [
    ('app_close', 'app.close.button_enabled', '1', 'bool',
     'Enable close button (X) on main page top-right corner (0=disabled, 1=enabled)', now, 'DbInitializer'),
    ('app_close', 'app.close.esc_key_enabled', '1', 'bool',
     'Enable ESC key to close app from main page (0=disabled, 1=enabled)', now, 'DbInitializer'),
    ('app_close', 'app.close.alt_f4_enabled', '1', 'bool',
     'Enable Alt+F4 to close app from main page (0=disabled, 1=enabled)', now, 'DbInitializer'),
]

for c in configs:
    try:
        conn.execute('INSERT INTO SystemConfig (Category, Key, Value, DataType, Description, ModifiedAt, ModifiedBy) VALUES (?,?,?,?,?,?,?)', c)
        print(f'  Added: {c[1]} = {c[2]}')
    except Exception as e:
        print(f'  Skip (exists): {c[1]}')

conn.commit()
count = conn.execute("SELECT COUNT(*) FROM SystemConfig WHERE Category='app_close'").fetchone()[0]
print(f'\nTotal app_close configs: {count} rows')
conn.close()
