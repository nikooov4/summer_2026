from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
SOURCE = ROOT / "rendered_presentations"
FONT = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 18)

for directory in SOURCE.iterdir():
    if not directory.is_dir():
        continue
    slides = sorted(directory.glob("slide-*.png"))
    thumb_width, thumb_height = 400, 225
    columns = 4
    rows = (len(slides) + columns - 1) // columns
    sheet = Image.new("RGB", (columns * thumb_width, rows * (thumb_height + 30)), "white")
    draw = ImageDraw.Draw(sheet)
    for index, path in enumerate(slides):
        with Image.open(path) as image:
            image.thumbnail((thumb_width, thumb_height))
            x = (index % columns) * thumb_width
            y = (index // columns) * (thumb_height + 30)
            sheet.paste(image, (x, y))
            draw.text((x + 8, y + thumb_height + 4), f"Слайд {index + 1}", font=FONT, fill="#1D334A")
    sheet.save(ROOT / f"contact-{directory.name}.png")
