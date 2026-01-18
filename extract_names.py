import os
import re
import codecs
import sys
sys.stdout.reconfigure(encoding='utf-8')

# Weapon names
print("=== 무기 이름 목록 ===")
weapon_dir = r'Assets\08.ScriptableObject\Weapon\WeaponData\Player'
for f in sorted(os.listdir(weapon_dir)):
    if f.endswith('.asset'):
        path = os.path.join(weapon_dir, f)
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
            # Decode unicode escapes
            match = re.search(r'_weaponName:\s*"?([^"\r\n]+)"?', content)
            if match:
                name = match.group(1)
                # Decode \uXXXX sequences
                decoded = codecs.decode(name, 'unicode_escape')
                print(f"{f.replace('.asset','')}: {decoded}")

print("\n=== 스킬 이름 목록 ===")
skill_dir = r'Assets\08.ScriptableObject\Skill\SkillData\Player'
for f in sorted(os.listdir(skill_dir)):
    if f.endswith('.asset'):
        path = os.path.join(skill_dir, f)
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
            match = re.search(r'skillName:\s*(.+)', content)
            if match:
                name = match.group(1).strip()
                print(f"{f.replace('.asset','')}: {name}")
