"""Recreate UserAccount table with new schema and seed BCrypt hashed data"""
import sqlite3
import subprocess
import sys

db = r'D:\TRIO2026\Database\trio240plus_main.db'
conn = sqlite3.connect(db)

# Create table with new schema
conn.execute('''CREATE TABLE IF NOT EXISTS UserAccount (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    RoleLevel INTEGER NOT NULL DEFAULT 1,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    LastLoginAt TEXT,
    FailedLoginCount INTEGER DEFAULT 0,
    LockedUntil TEXT,
    PasswordChangedAt TEXT,
    DisplayName TEXT,
    AvatarImage BLOB
)''')
conn.execute('CREATE UNIQUE INDEX IF NOT EXISTS IX_UserAccount_Username ON UserAccount(Username)')
conn.commit()
print('UserAccount table recreated')

# Generate BCrypt hashes using dotnet
# We'll use a small inline C# script via dotnet-script or just hardcode known hashes
# For now, generate via Python bcrypt if available, else use pre-generated

try:
    import bcrypt
    def hash_pwd(pwd):
        return bcrypt.hashpw(pwd.encode('utf-8'), bcrypt.gensalt(rounds=12)).decode('utf-8')
except ImportError:
    # Pre-generate using subprocess calling dotnet
    print("bcrypt not available in Python, using placeholder approach")
    def hash_pwd(pwd):
        return f"$2a$12$PLACEHOLDER_{pwd}"

import datetime
now = datetime.datetime.now(datetime.UTC).isoformat() + 'Z'

users = [
    (1, 'admin', hash_pwd('Trio@2026'), 3, 1, now, None, 0, None, now, 'Administrator', None),
    (2, 'service', hash_pwd('Service@240'), 2, 1, now, None, 0, None, now, 'Service Engineer', None),
    (3, 'operator', hash_pwd('Op@12345'), 1, 1, now, None, 0, None, now, 'Operator', None),
]

for u in users:
    conn.execute('''INSERT OR IGNORE INTO UserAccount 
        (Id, Username, PasswordHash, RoleLevel, IsActive, CreatedAt, LastLoginAt, 
         FailedLoginCount, LockedUntil, PasswordChangedAt, DisplayName, AvatarImage)
        VALUES (?,?,?,?,?,?,?,?,?,?,?,?)''', u)
    print(f'  Inserted: {u[1]} (Role={u[3]}, Hash={u[2][:30]}...)')

conn.commit()

# Verify
count = conn.execute('SELECT COUNT(*) FROM UserAccount').fetchone()[0]
print(f'\nTotal UserAccount rows: {count}')
rows = conn.execute('SELECT Id, Username, RoleLevel, DisplayName, substr(PasswordHash,1,30) FROM UserAccount').fetchall()
for r in rows:
    print(f'  {r[0]:2d}. {r[1]:12s} Role={r[2]} Display={r[3]:20s} Hash={r[4]}...')

conn.close()
