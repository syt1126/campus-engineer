"""Audit the public source inventory; does not inspect Unity caches or player data."""
from pathlib import Path
import hashlib, json, re

root=Path(__file__).resolve().parents[1]
excluded={'.git','Library','Temp','Obj','Logs','UserSettings','Builds','bin','obj','__pycache__'}
def source_files(folder):
    for f in folder.iterdir():
        if f.name in excluded: continue
        if f.is_dir(): yield from source_files(f)
        elif f.is_file(): yield f

failures=[]
for f in source_files(root):
    rel=f.relative_to(root).as_posix()
    if any(x in rel for x in ['Resources/SchoolAssets/','Resources/Characters/','Documentation/RetiredArt/']): failures.append(rel+': restricted source asset')
    if f.suffix.lower() in {'.alf','.ulf','.pfx','.keystore'} or f.name=='.env': failures.append(rel+': machine or credential file')
    if f.stat().st_size>95*1024*1024: failures.append(rel+': oversized Git blob')
    if f.suffix in {'.cs','.json','.md','.py','.yml','.asset','.unity'}:
        text=f.read_text(encoding='utf-8-sig',errors='replace')
        if re.search(r'[A-Za-z]:[/\\]Users[/\\]\d{3,}[/\\]',text): failures.append(rel+': local user path')
        if re.search(r'gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{50,}|sk-[A-Za-z0-9]{30,}',text): failures.append(rel+': possible credential')
for record in json.loads((root/'Documentation/ASSET_SOURCES.json').read_text(encoding='utf8')):
    f=root/record['path']
    if not f.is_file() or hashlib.sha256(f.read_bytes()).hexdigest()!=record['sha256']: failures.append(record['path']+': missing or changed licensed asset')
assert not failures,'\n'.join(failures)
print('PASS public source inventory, credential patterns, size limits and asset hashes')
