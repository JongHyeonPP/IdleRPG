import os, re, codecs, sys
sys.stdout.reconfigure(encoding='utf-8')

print("=== 스킬 설명 목록 ===\n")
skill_dir = r'Assets\08.ScriptableObject\Skill\SkillData\Player'
for f in sorted(os.listdir(skill_dir)):
    if f.endswith('.asset'):
        path = os.path.join(skill_dir, f)
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
            name = re.search(r'skillName:\s*(.+)', content)
            simple = re.search(r'simple:\s*(.+)', content)
            complex_m = re.search(r'complex:\s*"([^"]+)"', content)
            
            name_v = name.group(1).strip() if name else '-'
            simple_v = simple.group(1).strip() if simple and simple.group(1).strip() else '(없음)'
            
            if complex_m:
                raw = complex_m.group(1)
                try:
                    complex_v = codecs.decode(raw, 'unicode_escape')
                except:
                    complex_v = raw
            else:
                complex_v = '(없음)'
            
            print(f"[{f.replace('.asset','')}] {name_v}")
            print(f"  설명: {complex_v}")
            print()
