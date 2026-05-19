import sqlite3
import datetime

conn = sqlite3.connect(r'D:\TRIO2026\Database\trio240plus_config.db')
now = datetime.datetime.utcnow().isoformat() + 'Z'

configs = [
    ('login_ui', 'login.show_user_dropdown', '0', 'bool', 'Show user dropdown list on login page (0=disabled, 1=enabled)', now, 'DbInitializer'),
    ('login_ui', 'login.remember_password_enabled', '1', 'bool', 'Allow remember password feature (0=disabled, 1=enabled)', now, 'DbInitializer'),
    ('login_ui', 'login.max_failed_attempts', '5', 'int', 'Max consecutive failed login attempts before lockout', now, 'DbInitializer'),
    ('login_ui', 'login.lockout_minutes', '15', 'int', 'Account lockout duration in minutes', now, 'DbInitializer'),
    ('login_ui', 'login.session_timeout_minutes', '30', 'int', 'Session idle timeout in minutes (0=disabled)', now, 'DbInitializer'),
]

for c in configs:
    try:
        conn.execute('INSERT INTO SystemConfig (Category, Key, Value, DataType, Description, ModifiedAt, ModifiedBy) VALUES (?,?,?,?,?,?,?)', c)
        print(f'  Added: {c[1]} = {c[2]}')
    except Exception as e:
        print(f'  Skip (exists): {c[1]}')

conn.commit()
count = conn.execute("SELECT COUNT(*) FROM SystemConfig WHERE Category='login_ui'").fetchone()[0]
print(f'\nTotal login_ui configs: {count} rows')
conn.close()
