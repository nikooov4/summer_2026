from __future__ import annotations

import argparse
from pathlib import Path

import fitz


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()

    document = fitz.open(args.input)
    chunks = [f"PAGES={document.page_count}"]
    for index, page in enumerate(document, start=1):
        chunks.append(f"\n--- PAGE {index} ---\n")
        chunks.append(page.get_text("text"))

    args.output.write_text("".join(chunks), encoding="utf-8")


if __name__ == "__main__":
    main()
