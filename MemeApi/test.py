import sys
import requests
from PIL import Image, ImageFont, ImageDraw
import io

def open_image_from_url(url):
    response = requests.get(url)
    return Image.open(io.BytesIO(response.content))

def draw_text(text, font, pos, max_size, drawer):
    multiline_text = split_line(text, font, max_size[0])
    margins = get_margins(multiline_text, font, max_size, drawer)
    pos = (pos[0] + margins[0], pos[1] + margins[1])

    # Draw shadow
    shadow_color = (0, 0, 0)
    for offset in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
        shadow_pos = (pos[0] + offset[0], pos[1] + offset[1])
        drawer.multiline_text(shadow_pos, multiline_text, font=font, fill=shadow_color, align="center")

    # Draw main text
    drawer.multiline_text(pos, multiline_text, font=font, fill=(255, 255, 255), align="center")

def split_line(text, font, width):
    result = ""
    while text:
        if get_text_width(text, font) < width:
            result += text
            break
        for i in range(len(text), 0, -1):
            if get_text_width(text[:i], font) < width:
                if ' ' not in text[:i]:
                    result += text[:i] + "-\n"
                    text = text[i:]
                else:
                    for l in range(i, 0, -1):
                        if text[l] == ' ':
                            result += text[:l] + "\n"
                            text = text[l + 1:]
                            break
                break
    if result.endswith("-\n"):
        result = result[:-2]
    return result

def get_text_width(text, font):
    bbox = font.getbbox(text)
    return bbox[2] - bbox[0]

def get_margins(text, font, max_size, drawer):
    bbox = drawer.textbbox((0, 0), text, font=font, spacing=4)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    width_margin = round((max_size[0] - text_width) / 2)
    height_margin = round((max_size[1] - text_height) / 2)
    return width_margin, height_margin

def get_font(size):
    try:
        return ImageFont.truetype("impact.ttf", size)
    except IOError:
        #print("⚠️ Could not load 'impact.ttf', using default font.")
        return ImageFont.load_default()

def render_text_on_image():
    img = open_image_from_url("https://memeapi-file-server.fra1.digitaloceanspaces.com/profilePic/default.jpg").convert('RGB')

    # Resize to 400x400 before drawing
    img = img.resize((400, 400), Image.LANCZOS)

    drawer = ImageDraw.Draw(img)
    font = get_font(32)

    draw_text("lakjlkwaldkjwlakjdlwkaj alwdkjaldkwjalkjdwlk ajwaldkjwaldkjwaldkj awdwakljlwadkdj lakwjdlak wjdlk jawldkjwalk djwalk jlkawjdlakwjd", font, (0, 25), (400, 50), drawer)
    draw_text("lakjlkwaldkjwlakjdlwkaj alwdkjaldkwjalkjdwlk ajwaldkjwaldkjwaldkj awdwakljlwadkdj lakwjdlak wjdlk jawldkjwalk djwalk jlkawjdlakwjd", font, (0, 325), (400, 50), drawer)

    img_bytes = io.BytesIO()
    img.save(img_bytes, format="PNG", optimize=True)
    
    with open("output.png", "wb") as f:
        f.write(img_bytes.getbuffer())
    img_bytes.seek(0)
    return img_bytes

bytes = render_text_on_image()
sys.stdout.buffer.write(bytes.getvalue())