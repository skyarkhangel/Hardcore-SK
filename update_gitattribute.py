"""
Generate .gitattributes file for HSK mods.
Unused mods will be marked as export-ignore, 
so they won't be included in the exported zip file (or when using download zip).
"""

# pylint: disable=C0116,C0114
import os
import xml.etree.ElementTree as ET


folder_dict: dict[str, str] = {}


def get_modsconfig() -> list[str]:
    result = []
    with open("./ModsConfig.xml", "r", encoding="utf-8") as f:
        tree = ET.parse(f)
        root = tree.getroot()
        print(root.tag)
        for mod in root.find("activeMods").iter():
            result.append(mod.text.lower())
    return result

def is_hsk(path: str) -> bool:
    with os.scandir(path) as folder:
        for entry in folder:
            if entry.is_file() and entry.name == "HSK":
                #print(path  + " is a HSK mod.")
                return True
    return False


def build_dict():
    for entry in filter(lambda x: x.is_dir(), os.scandir("./Mods")):
        subdir_path = entry.path
        if is_hsk(subdir_path):
            for cur, dirs, _ in os.walk(subdir_path):
                for directory in dirs:
                    if 'About.xml' in os.listdir(os.path.join(cur, directory)):
                        path = os.path.join(cur, directory, 'About.xml')
                        #print(path)
                        root = ET.parse(path).getroot()
                        package_id = root.find("packageId").text.lower()
                        folder_dict[package_id] = entry.path
                break


def output(enabled: list[str]) -> None:
    with open(".gitattributes", "w", encoding='utf-8') as f:
        for k, v in folder_dict.items():
            print(f"{k} {v}")
            if k not in enabled:
                f.write(f"\"{v.removeprefix('./')}\" export-ignore\n")


def main():
    enabled = get_modsconfig()
    print(enabled)
    build_dict()
    output(enabled)


if __name__ == "__main__":
    main()
