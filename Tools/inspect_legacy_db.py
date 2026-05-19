"""檢查舊系統 userinfo.db 的結構與內容"""
import sqlite3

db_path = '//vmware-host/Shared Folders/[TRIO] 專案/TRIO240 source code/上位机-Trio-PC_3_7/Trio-PC_3_7/userinfo.db'
conn = sqlite3.connect(db_path)

print(f'=== userinfo.db ===')
print(f'File: {db_path}')
print()

# Journal mode
jm = conn.execute('PRAGMA journal_mode').fetchone()[0]
print(f'Journal Mode: {jm}')

# Tables
tables = conn.execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name").fetchall()
print(f'Tables: {len(tables)}')
print()

for (tname,) in tables:
    count = conn.execute(f'SELECT COUNT(*) FROM [{tname}]').fetchone()[0]
    print(f'--- Table: {tname} ({count} rows) ---')
    
    # Columns
    cols = conn.execute(f'PRAGMA table_info([{tname}])').fetchall()
    col_names = [c[1] for c in cols]
    print(f'  Columns: {", ".join(col_names)}')
    for c in cols:
        print(f'    {c[0]:2d}. {c[1]:20s} {c[2]:10s} NOT_NULL={c[3]} PK={c[5]}')
    
    # Indexes
    idxs = conn.execute(f'PRAGMA index_list([{tname}])').fetchall()
    if idxs:
        print(f'  Indexes:')
        for idx in idxs:
            idx_cols = conn.execute(f'PRAGMA index_info([{idx[1]}])').fetchall()
            print(f'    {idx[1]} unique={idx[2]} cols={[ic[2] for ic in idx_cols]}')
    
    # Data (mask passwords)
    if count > 0 and count <= 50:
        print(f'  Data:')
        rows = conn.execute(f'SELECT * FROM [{tname}]').fetchall()
        for r in rows:
            display = []
            for i, v in enumerate(r):
                cn = col_names[i].lower()
                if ('pass' in cn or 'pwd' in cn or 'secret' in cn) and v:
                    display.append('***MASKED***')
                else:
                    display.append(str(v)[:60] if v else 'NULL')
            print(f'    {display}')
    print()

conn.close()
