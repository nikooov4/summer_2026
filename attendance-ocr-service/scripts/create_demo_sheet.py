from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "examples" / "demo_attendance_sheet.png"


def main() -> None:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    image = Image.new("RGB", (800, 600), "white")
    draw = ImageDraw.Draw(image)
    font_title = _font(28)
    font = _font(22)
    small = _font(18)

    draw.rectangle((20, 20, 780, 580), outline=(24, 32, 42), width=2)
    draw.text((40, 32), "Attendance sheet", fill=(24, 32, 42), font=font_title)
    draw.text((40, 82), "Date: 2026-05-20", fill=(24, 32, 42), font=small)
    draw.text((300, 82), "Group: PI-101", fill=(24, 32, 42), font=small)
    draw.text((40, 118), "Subject: Software Engineering", fill=(24, 32, 42), font=small)

    rows = [
        ("1", "Ivan Smirnov"),
        ("2", "Maria Petrova"),
        ("3", "Sergey Orlov"),
    ]
    y = 180
    draw.line((40, y - 20, 740, y - 20), fill=(215, 221, 229), width=2)
    draw.text((50, y), "N", fill=(104, 115, 133), font=small)
    draw.text((110, y), "Student name", fill=(104, 115, 133), font=small)
    draw.text((560, y), "Signature", fill=(104, 115, 133), font=small)
    y += 40
    for number, name in rows:
        draw.line((40, y - 10, 740, y - 10), fill=(215, 221, 229), width=1)
        draw.text((50, y), number, fill=(24, 32, 42), font=font)
        draw.text((110, y), name, fill=(24, 32, 42), font=font)
        draw.line((560, y + 22, 720, y + 22), fill=(15, 118, 110), width=2)
        y += 62

    image.save(OUT, format="PNG")
    print(OUT)


def _font(size: int):
    for path in [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf",
    ]:
        if Path(path).exists():
            return ImageFont.truetype(path, size=size)
    return ImageFont.load_default()


if __name__ == "__main__":
    main()
