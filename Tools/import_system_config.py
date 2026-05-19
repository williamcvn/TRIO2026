"""
TRIO2026 SystemConfig Seed Data — INI Parser
=============================================
解析 12 個 config/*.ini 檔案，將所有 key-value 對寫入
trio240plus_config.db 的 SystemConfig 表。

Category 對應規則（原 .ini 檔名 → Category 名稱）:
  motocfg.ini       → motor
  areaposcfg.ini    → area_position
  temperaturecfg.ini→ temperature
  opticsinfo.ini    → optics
  pipetteinfo.ini   → pipette
  trioinfo.ini      → trio_info
  tubecfg.ini       → tube
  cameracfg.ini     → camera
  syscfg.ini        → system
  maintenance.ini   → maintenance
  flowlist.ini      → flow_list
  flowinfo.ini      → flow_info

Key 格式: [Section].key （保留原始巢狀結構）
"""

import configparser
import sqlite3
import os
import datetime

CONFIG_DIR = r'\\vmware-host\Shared Folders\[TRIO] 專案\TRIO240 source code\上位机-Trio-PC_3_7\Trio-PC_3_7\config'
DB_PATH = r'D:\TRIO2026\Database\trio240plus_config.db'

# INI 檔名 → Category 名稱對應表
FILE_CATEGORY_MAP = {
    'motocfg.ini':        'motor',
    'areaposcfg.ini':     'area_position',
    'temperaturecfg.ini': 'temperature',
    'opticsinfo.ini':     'optics',
    'pipetteinfo.ini':    'pipette',
    'trioinfo.ini':       'trio_info',
    'tubecfg.ini':        'tube',
    'cameracfg.ini':      'camera',
    'syscfg.ini':         'system',
    'maintenance.ini':    'maintenance',
    'flowlist.ini':       'flow_list',
    'flowinfo.ini':       'flow_info',
}

def detect_data_type(value):
    """推斷值的型別"""
    if value is None or value.strip() == '':
        return 'string'
    v = value.strip().strip('"')
    # bool
    if v.lower() in ('true', 'false'):
        return 'bool'
    # int
    try:
        int(v)
        return 'int'
    except ValueError:
        pass
    # float
    try:
        float(v)
        return 'float'
    except ValueError:
        pass
    return 'string'

def parse_ini_file(filepath):
    """解析單一 INI 檔案，回傳 [(section, key, value, data_type)] 列表"""
    config = configparser.ConfigParser(interpolation=None)
    # 保留大小寫
    config.optionxform = str

    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            config.read_file(f)
    except UnicodeDecodeError:
        with open(filepath, 'r', encoding='gbk') as f:
            config.read_file(f)

    entries = []
    for section in config.sections():
        for key, value in config.items(section):
            # 移除外層引號
            clean_value = value.strip('"').strip("'") if value else value
            data_type = detect_data_type(clean_value)
            entries.append((section, key, clean_value, data_type))
    return entries

def main():
    now = datetime.datetime.utcnow().isoformat() + 'Z'
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()

    # 檢查是否已有資料
    count = cursor.execute('SELECT COUNT(*) FROM SystemConfig').fetchone()[0]
    if count > 0:
        print(f'SystemConfig 已有 {count} 筆資料，跳過植入')
        conn.close()
        return

    total = 0
    for ini_file, category in FILE_CATEGORY_MAP.items():
        filepath = os.path.join(CONFIG_DIR, ini_file)
        if not os.path.exists(filepath):
            print(f'  [SKIP] {ini_file} 不存在')
            continue

        entries = parse_ini_file(filepath)
        for section, key, value, data_type in entries:
            # Key 格式: [Section].key
            full_key = f'[{section}].{key}'
            cursor.execute('''
                INSERT INTO SystemConfig (Category, Key, Value, DataType, Description, ModifiedAt, ModifiedBy)
                VALUES (?, ?, ?, ?, ?, ?, ?)
            ''', (category, full_key, value, data_type, f'From {ini_file}', now, 'seed_import'))
            total += 1

        print(f'  {ini_file:25s} -> {category:20s} ({len(entries):4d} entries)')

    conn.commit()
    conn.close()
    print(f'\nTotal: {total} SystemConfig entries inserted')

if __name__ == '__main__':
    print('=== TRIO2026 SystemConfig Seed Data Import ===')
    print(f'Source: {CONFIG_DIR}')
    print(f'Target: {DB_PATH}')
    print()
    main()
