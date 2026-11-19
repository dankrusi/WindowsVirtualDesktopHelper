"""Generate an expanded English catalog for manual layout testing."""
import argparse
import pathlib
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("output", type=pathlib.Path, help="Temporary output directory")
args = parser.parse_args()
source = pathlib.Path(__file__).resolve().parents[1] / "locales" / "en-US.ftl"
accents = str.maketrans("aeiouAEIOU", "áëïôüÁËÏÔÜ")
lines = []
for line in source.read_text(encoding="utf-8").splitlines():
    if not line or line.startswith("#"):
        lines.append(line)
        continue
    key, sep, value = line.partition(" = ")
    if not sep:
        key, value = "", line
    # Preserve Fluent variables; current English catalog has no selectors.
    parts = re.split(r"(\{[^}]*\})", value)
    value = "".join(
        part if part.startswith("{") else part.translate(accents)
        for part in parts
    )
    if sep:
        value = "［" + value + " ·" * max(3, len(value) // 8) + "］"
    lines.append(key + sep + value)
args.output.mkdir(parents=True, exist_ok=True)
(args.output / "en-US.ftl").write_text("\n".join(lines) + "\n", encoding="utf-8")
print(args.output / "en-US.ftl")
