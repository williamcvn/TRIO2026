"""
從實際 .db 檔案提取完整 Schema 資訊，輸出 JSON 格式供文件生成使用。
"""
import sqlite3, os, json

db_dir = r'D:\TRIO2026\Database'
result = {}

for db_file in sorted(os.listdir(db_dir)):
    if not db_file.endswith('.db'):
        continue
    db_info = {'tables': {}}
    conn = sqlite3.connect(os.path.join(db_dir, db_file))
    
    tables = conn.execute("SELECT name FROM sqlite_master WHERE type='table' AND name != 'sqlite_sequence' ORDER BY name").fetchall()
    for (tname,) in tables:
        tinfo = {'columns': [], 'indexes': [], 'foreign_keys': [], 'row_count': 0}
        
        # Columns
        cols = conn.execute(f'PRAGMA table_info({tname})').fetchall()
        for c in cols:
            tinfo['columns'].append({
                'cid': c[0], 'name': c[1], 'type': c[2],
                'not_null': bool(c[3]), 'default': c[4], 'pk': bool(c[5])
            })
        
        # Indexes
        idxs = conn.execute(f'PRAGMA index_list({tname})').fetchall()
        for idx in idxs:
            idx_cols = conn.execute(f'PRAGMA index_info({idx[1]})').fetchall()
            tinfo['indexes'].append({
                'name': idx[1], 'unique': bool(idx[2]),
                'columns': [ic[2] for ic in idx_cols]
            })
        
        # Foreign keys
        fks = conn.execute(f'PRAGMA foreign_key_list({tname})').fetchall()
        for fk in fks:
            tinfo['foreign_keys'].append({
                'from': fk[3], 'to_table': fk[2], 'to_column': fk[4],
                'on_delete': fk[6], 'on_update': fk[5]
            })
        
        # Row count
        tinfo['row_count'] = conn.execute(f'SELECT COUNT(*) FROM {tname}').fetchone()[0]
        
        db_info['tables'][tname] = tinfo
    
    # Journal mode
    db_info['journal_mode'] = conn.execute('PRAGMA journal_mode').fetchone()[0]
    db_info['foreign_keys_enabled'] = conn.execute('PRAGMA foreign_keys').fetchone()[0]
    
    result[db_file] = db_info
    conn.close()

# Output
out_path = r'D:\TRIO2026\DocumentsAndInformation\DatabaseDocs\_schema_dump.json'
with open(out_path, 'w', encoding='utf-8') as f:
    json.dump(result, f, ensure_ascii=False, indent=2)
print(f'Schema dump saved to {out_path}')
print(f'Total databases: {len(result)}')
for db, info in result.items():
    print(f'  {db}: {len(info["tables"])} tables, journal={info["journal_mode"]}')
