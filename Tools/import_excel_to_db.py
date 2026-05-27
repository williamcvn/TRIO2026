"""
Excel → data.db 匯入工具
將舊 TRIO 系統的 Excel 報告匯入 TRIO2026 的 data.db

來源: \\vmware-host\Shared Folders\[TRIO] 專案\機台產出的excel報告\trio_data\
目標: D:\TRIO2026\Database\data.db

製作者: Office of William
"""

import openpyxl
import sqlite3
import os
import sys
import json
from datetime import datetime

# 設定路徑
EXCEL_DIR = r"\\vmware-host\Shared Folders\[TRIO] 專案\機台產出的excel報告\trio_data"
DB_PATH = r"D:\TRIO2026\Database\data.db"


def parse_excel(filepath: str) -> dict:
    """解析一個 Excel 報告檔案，返回結構化資料"""
    wb = openpyxl.load_workbook(filepath, read_only=True, data_only=True)
    ws = wb['Sheet1']
    
    # 讀取所有儲存格為 dict {(row, col): value}
    cells = {}
    for row in ws.iter_rows(values_only=False):
        for c in row:
            if c.value is not None:
                cells[(c.row, c.column)] = c.value
    wb.close()
    
    # 判斷報告類型
    title = str(cells.get((1, 1), '')).strip()
    report_type = 'IntelliPlex' if 'IntelliPlex' in title else 'Custom'
    
    # 取得檔名中的時間戳作為 RunId
    basename = os.path.splitext(os.path.basename(filepath))[0]
    
    # 解析 Header 欄位 (key-value pairs in column A-B)
    header = {}
    for r in range(3, 20):
        key = str(cells.get((r, 1), '')).strip()
        val = str(cells.get((r, 2), '')).strip()
        if key:
            header[key] = val
    
    # 實驗日期
    experiment_date = header.get('Experiment Date', '')
    
    # 共通欄位
    record = {
        'RunId': basename,
        'ReportType': report_type,
        'FlowName': report_type,
        'ExperimentDate': experiment_date,
        'ExtractionProgram': header.get('Extraction Program', None),
        'ExtractionKitLotNo': header.get('Extraction Kit Lot. No.', None),
        'ExtractionSampleVolume': header.get('Extraction Sample Volume', None),
        'ElutionVolume': header.get('Elution Volume', None),
        'PcrPlateId': header.get('PCR Plate ID', None),
        'PcrTotalNucleicAcidInput': header.get('PCR Total Nucleic Acid Input', None),
        'S1AdValue': header.get('S1 A/D Value', None),
        'S2AdValue': header.get('S2 A/D Value', None),
        'OperatorUsername': 'legacy_import',
        'OperatorDisplayName': 'Legacy Import',
        'SoftwareVersion': 'TRIO Legacy',
        'Status': 'Completed',
    }
    
    # IntelliPlex 專屬欄位
    if report_type == 'IntelliPlex':
        record['IntelliPlexKit1Name'] = header.get('IntelliPlex Kit 1 Product Name', None)
        record['IntelliPlexKit1LotNo'] = header.get('IntelliPlex Kit 1 Lot No.', None)
        record['IntelliPlexKit2Name'] = header.get('IntelliPlex Kit 2 Product Name', None)
        record['IntelliPlexKit2LotNo'] = header.get('IntelliPlex Kit 2 Lot No.', None)
    
    # Custom 專屬欄位
    if report_type == 'Custom':
        record['FunctionModulesSelected'] = header.get('Function Modules Selected', None)
        
        # PCR Setup JSON (Rxn1-4)
        pcr_setup = {}
        for r in range(10, 16):
            key = str(cells.get((r, 1), '')).strip()
            if key and key != 'Custom PCR Setup':
                vals = {}
                for rxn_idx, col in enumerate([2, 3, 4, 5], 1):
                    v = str(cells.get((r, col), '')).strip()
                    if v:
                        vals[f'Rxn{rxn_idx}'] = v
                if vals:
                    pcr_setup[key] = vals
        if pcr_setup:
            record['CustomPcrSetupJson'] = json.dumps(pcr_setup, ensure_ascii=False)
    
    # 解析樣本資料
    # 找到 "Sample Position" 標頭行
    data_start_row = None
    for r in range(18, 30):
        val = str(cells.get((r, 1), '')).strip()
        if val == 'Sample Position':
            data_start_row = r + 2  # +1 for sub-header, +1 for first data row
            break
    
    samples = []
    if data_start_row:
        for r in range(data_start_row, data_start_row + 30):
            pos_val = str(cells.get((r, 1), '')).strip()
            if not pos_val:
                break
            
            conc_raw = str(cells.get((r, 2), '')).strip()
            utilized = str(cells.get((r, 3), '')).strip()
            
            sample = {
                'SamplePosition': pos_val if pos_val not in ('NC', 'PC') else None,
                'ConcentrationDisplay': conc_raw if conc_raw else None,
                'UtilizedElutedVolume': None,
            }
            
            # 嘗試解析數值
            try:
                sample['Concentration'] = float(conc_raw)
            except (ValueError, TypeError):
                sample['Concentration'] = None
            
            try:
                sample['UtilizedElutedVolume'] = float(utilized)
            except (ValueError, TypeError):
                pass
            
            if report_type == 'IntelliPlex':
                sample['PcrWellKit1'] = str(cells.get((r, 4), '')).strip() or None
                sample['PcrWellKit2'] = str(cells.get((r, 5), '')).strip() or None
                sample['SampleId'] = str(cells.get((r, 6), '')).strip() or None
                sample['ElutionTubeId'] = str(cells.get((r, 7), '')).strip() or None
            else:  # Custom
                sample['PcrWellKit1'] = str(cells.get((r, 4), '')).strip() or None
                sample['PcrWellKit2'] = str(cells.get((r, 5), '')).strip() or None
                sample['PcrWellRxn3'] = str(cells.get((r, 6), '')).strip() or None
                sample['PcrWellRxn4'] = str(cells.get((r, 7), '')).strip() or None
                sample['SampleId'] = str(cells.get((r, 8), '')).strip() or None
                sample['ElutionTubeId'] = str(cells.get((r, 9), '')).strip() or None
            
            # 特殊位置標記（NC=Negative Control, PC=Positive Control）
            if pos_val in ('NC', 'PC'):
                sample['SampleBarcode'] = pos_val
            
            samples.append(sample)
    
    record['SampleCount'] = len(samples)
    
    # 從檔名解析時間
    try:
        dt = datetime.strptime(basename, '%Y%m%d_%H%M%S')
        record['StartTime'] = dt.isoformat()
        record['EndTime'] = dt.isoformat()
    except ValueError:
        record['StartTime'] = basename
    
    return record, samples


def import_to_db(db_path: str, records: list):
    """將解析好的資料寫入 SQLite"""
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    
    imported = 0
    skipped = 0
    
    for record, samples in records:
        # 檢查是否已存在
        cursor.execute("SELECT Id FROM TestRecord WHERE RunId = ?", (record['RunId'],))
        if cursor.fetchone():
            print(f"  [SKIP] {record['RunId']} already exists")
            skipped += 1
            continue
        
        # 插入 TestRecord
        cols = [k for k in record.keys() if record[k] is not None]
        placeholders = ', '.join(['?' for _ in cols])
        col_names = ', '.join(cols)
        values = [record[k] for k in cols]
        
        cursor.execute(f"INSERT INTO TestRecord ({col_names}) VALUES ({placeholders})", values)
        test_record_id = cursor.lastrowid
        
        # 插入 SampleResult
        now = datetime.utcnow().isoformat()
        for sample in samples:
            sample['TestRecordId'] = test_record_id
            sample['CreatedAt'] = now
            
            s_cols = [k for k in sample.keys() if sample[k] is not None]
            s_placeholders = ', '.join(['?' for _ in s_cols])
            s_col_names = ', '.join(s_cols)
            s_values = [sample[k] for k in s_cols]
            
            cursor.execute(f"INSERT INTO SampleResult ({s_col_names}) VALUES ({s_placeholders})", s_values)
        
        print(f"  [OK] {record['RunId']}: {record['ReportType']}, {len(samples)} samples")
        imported += 1
    
    conn.commit()
    conn.close()
    
    return imported, skipped


def main():
    print("=" * 60)
    print("TRIO2026 Excel → data.db 匯入工具")
    print("=" * 60)
    
    # 列出所有 Excel 檔案
    if not os.path.exists(EXCEL_DIR):
        print(f"ERROR: 來源目錄不存在: {EXCEL_DIR}")
        sys.exit(1)
    
    files = sorted([f for f in os.listdir(EXCEL_DIR) if f.endswith('.xlsx') and not f.startswith('~$')])
    print(f"\n找到 {len(files)} 個 Excel 報告")
    
    if not os.path.exists(DB_PATH):
        print(f"ERROR: data.db 不存在: {DB_PATH}")
        print("請先執行 DbInitializer 建立 data.db")
        sys.exit(1)
    
    # 解析所有 Excel
    print(f"\n正在解析 Excel 報告...")
    records = []
    errors = []
    for fname in files:
        try:
            path = os.path.join(EXCEL_DIR, fname)
            record, samples = parse_excel(path)
            records.append((record, samples))
        except Exception as e:
            errors.append((fname, str(e)))
            print(f"  [ERROR] {fname}: {e}")
    
    print(f"\n成功解析: {len(records)}, 失敗: {len(errors)}")
    
    # 匯入 DB
    print(f"\n正在匯入 data.db...")
    imported, skipped = import_to_db(DB_PATH, records)
    
    print(f"\n{'=' * 60}")
    print(f"匯入完成!")
    print(f"  成功: {imported}")
    print(f"  跳過: {skipped} (已存在)")
    print(f"  失敗: {len(errors)}")
    if errors:
        print(f"\n失敗列表:")
        for fname, err in errors:
            print(f"  - {fname}: {err}")


if __name__ == "__main__":
    main()
