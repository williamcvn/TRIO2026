"""
TRIO2026 FlowDefinition + FlowStep Seed Data — .flow Parser
==============================================================
解析 .flow 檔案，將流程定義寫入 trio240plus_main.db:
  - FlowDefinition: 流程名稱、步驟數
  - FlowStep: 每個步驟的指令、參數、群組

.flow 檔案格式:
  - ##GroupName  → 群組開始標記
  - **GroupName  → 群組結束標記  
  - stepOrder,commandId,crc,arg0,arg1,arg2,arg3,arg4,[stringArg]
  - 當 commandId in (22,31,32,33) 時，arg1~arg4 可能是字串參數
"""

import sqlite3
import os
import glob
import datetime
import re

FLOW_DIRS = [
    '//vmware-host/Shared Folders/[TRIO] 專案/TRIO240 source code/上位机-Trio-PC_3_7/Trio-PC_3_7/flow',
    '//vmware-host/Shared Folders/[TRIO] 專案/TRIO240 source code/上位机-Trio-PC_3_7/Trio-PC_3_7',
]
DB_PATH = r'D:\TRIO2026\Database\trio240plus_main.db'

# 字串指令 ID（對應 F_OPTFG=22, 31=suck file, 32=mix file, 33=PCR temp）
STRING_COMMAND_IDS = {22, 31, 32, 33}


def parse_flow_file(filepath):
    """
    解析 .flow 檔案。
    回傳: (flow_name, steps_list)
    steps_list: [(step_order, command_id, crc, arg0..arg4, string_arg, group_name, group_depth)]
    """
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()
    except UnicodeDecodeError:
        with open(filepath, 'r', encoding='gbk') as f:
            lines = f.readlines()

    flow_name = os.path.splitext(os.path.basename(filepath))[0]
    steps = []
    group_stack = []  # 群組名稱堆疊

    for line in lines:
        line = line.strip()
        if not line:
            continue

        # 群組開始: ##GroupName
        if line.startswith('##'):
            group_name = line[2:].strip()
            group_stack.append(group_name)
            # 第一行可能也是 flow_name（如 ##Dilution Program 4）
            if len(steps) == 0 and len(group_stack) == 1:
                # 用檔案內的群組名稱作為 flow_name（如果更有意義的話）
                if group_name and not group_name.startswith('\ufeff'):
                    flow_name = group_name
            continue

        # 群組結束: **GroupName
        if line.startswith('**'):
            if group_stack:
                group_stack.pop()
            continue

        # 解析步驟行: stepOrder,commandId,crc,arg0,arg1,arg2,arg3,arg4,[stringArg]
        parts = line.rstrip(', ').split(',')
        if len(parts) < 8:
            continue

        try:
            step_order = int(parts[0].strip())
            command_id = int(parts[1].strip())
            crc = int(parts[2].strip())
        except (ValueError, IndexError):
            continue

        # 判斷是否為字串指令
        string_arg = None
        args = [0.0] * 5

        if command_id in STRING_COMMAND_IDS:
            # 字串指令：arg0 是數值，arg1~ 可能是字串
            try:
                args[0] = float(parts[3].strip()) if parts[3].strip() else 0.0
            except ValueError:
                args[0] = 0.0
            # 字串參數（拼接 parts[4] 開始）
            str_parts = [p.strip() for p in parts[4:8] if p.strip()]
            string_arg = ','.join(str_parts) if str_parts else None
        else:
            for i in range(5):
                idx = 3 + i
                if idx < len(parts):
                    try:
                        args[i] = float(parts[idx].strip()) if parts[idx].strip() else 0.0
                    except ValueError:
                        args[i] = 0.0

        current_group = group_stack[-1] if group_stack else None
        group_depth = len(group_stack)

        steps.append({
            'step_order': step_order,
            'command_id': command_id,
            'crc': crc,
            'arg0': args[0],
            'arg1': args[1],
            'arg2': args[2],
            'arg3': args[3],
            'arg4': args[4],
            'string_arg': string_arg,
            'group_name': current_group,
            'group_depth': group_depth,
        })

    return flow_name, steps


def main():
    now = datetime.datetime.now(datetime.UTC).isoformat() + 'Z'
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()

    # 檢查是否已有資料
    count = cursor.execute('SELECT COUNT(*) FROM FlowDefinition').fetchone()[0]
    if count > 0:
        print(f'FlowDefinition already has {count} rows, skipping.')
        conn.close()
        return

    # 收集所有 .flow 檔案（去重）
    flow_files = {}
    for flow_dir in FLOW_DIRS:
        if not os.path.exists(flow_dir):
            print(f'  [SKIP] {flow_dir} does not exist')
            continue
        for fname in os.listdir(flow_dir):
            if fname.lower().endswith('.flow'):
                fp = os.path.join(flow_dir, fname)
                if fname not in flow_files:
                    flow_files[fname] = fp

    print(f'Found {len(flow_files)} unique .flow files')

    total_flows = 0
    total_steps = 0
    seen_names = set()

    for basename, filepath in sorted(flow_files.items()):
        flow_name, steps = parse_flow_file(filepath)
        if not steps:
            print(f'  [SKIP] {basename} (0 steps)')
            continue

        # 去重：若 flow_name 已存在，使用檔名（去掉 .flow）作為唯一名稱
        if flow_name in seen_names:
            flow_name = os.path.splitext(basename)[0]
        if flow_name in seen_names:
            flow_name = f'{flow_name}_{len(seen_names)}'
        seen_names.add(flow_name)

        # 插入 FlowDefinition
        cursor.execute('''
            INSERT INTO FlowDefinition (FlowName, Description, TotalSteps, Version, IsActive, CreatedAt, ModifiedAt, ModifiedBy)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ''', (flow_name, f'Imported from {basename}', len(steps), '1.0', 1, now, now, 'seed_import'))

        flow_def_id = cursor.lastrowid

        # 插入 FlowStep
        for step in steps:
            cursor.execute('''
                INSERT INTO FlowStep (FlowDefinitionId, StepOrder, CommandId, Crc, Arg0, Arg1, Arg2, Arg3, Arg4, StringArg, GroupName, GroupDepth)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ''', (
                flow_def_id,
                step['step_order'],
                step['command_id'],
                step['crc'],
                step['arg0'], step['arg1'], step['arg2'], step['arg3'], step['arg4'],
                step['string_arg'],
                step['group_name'],
                step['group_depth'],
            ))

        total_flows += 1
        total_steps += len(steps)
        print(f'  {basename:45s} -> {flow_name:35s} ({len(steps):4d} steps)')

    conn.commit()
    conn.close()
    print(f'\nTotal: {total_flows} FlowDefinitions, {total_steps} FlowSteps inserted')


if __name__ == '__main__':
    print('=== TRIO2026 FlowDefinition + FlowStep Import ===')
    print(f'Target: {DB_PATH}')
    print()
    main()
