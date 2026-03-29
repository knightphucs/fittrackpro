import os
import time
import hashlib
import requests
from pathlib import Path
from tqdm import tqdm
from PIL import Image
from io import BytesIO
from ddgs import DDGS
 
# ─────────────────────────────────────────────────────────
# CONFIG
# ─────────────────────────────────────────────────────────
OUTPUT_DIR   = Path("scripts/training-data")
IMAGES_PER_CATEGORY = 70       # Download 70, sau khi filter còn ~60
MIN_IMAGE_SIZE = (100, 100)    # Loại ảnh quá nhỏ
MAX_FILE_SIZE_MB = 5
DELAY_BETWEEN_REQUESTS = 0.5  # Tránh bị rate limit
 
# ─────────────────────────────────────────────────────────
# Mapping: folder_label → search queries (tiếng Việt + Anh)
# Nhiều query → đa dạng hơn
# ─────────────────────────────────────────────────────────
FOOD_CATEGORIES = {
    "pho_bo": [
        "phở bò",
        "phở bò tô",
        "Vietnamese beef pho noodle soup",
        "phở bò hà nội",
    ],
    "com_tam": [
        "cơm tấm sườn bì chả",
        "Vietnamese broken rice com tam",
        "cơm tấm sài gòn",
    ],
    "banh_mi": [
        "bánh mì thịt việt nam",
        "Vietnamese banh mi sandwich",
        "bánh mì pate chả lụa",
    ],
    "bun_bo_hue": [
        "bún bò huế tô",
        "Vietnamese spicy beef noodle bun bo hue",
    ],
    "goi_cuon": [
        "gỏi cuốn",
        "gỏi cuốn tôm thịt",
        "gỏi cuốn Việt Nam",
        "Vietnamese fresh spring rolls goi cuon",
    ],
    "ca_hoi": [
        "cá hồi",
        "cá hồi nướng",
        "Vietnamese boiled salmon ca hoi",
    ],
    "banh_xeo": [
        "bánh xèo",
        "bánh xèo giòn tôm thịt",
        "Vietnamese sizzling crepe banh xeo",
    ],
    "trung_ga": [
        "trứng gà",
        "trứng gà luộc",
        "trứng gà hấp",
        "Vietnamese boiled chicken egg trung ga",
    ],
    "lau_ca": [
        "lẩu cá",
        "lẩu cá thác lác",
        "lẩu cá hải sản",
        "Vietnamese fish hot pot lau ca",
    ],
    "ca_phe_sua_da": [
        "cà phê sữa đá ly",
        "Vietnamese iced coffee ca phe sua da",
    ],
    "tra_sua": [
        "milk tea",
        "trà sữa trân châu",
        "trà sữa trân châu ly",
        "Vietnamese bubble tea milk tea",
    ],
    "banh_bao": [
        "bánh bao nhân thịt",
        "Vietnamese steamed bun banh bao",
    ],
    "bun_cha": [
        "bún chả hà nội",
        "bún chả hà nội bát",
        "Vietnamese bun cha noodle pork",
    ],
    "uc_ga": [
        "chicken breast",
        "ức gà",
        "ức gà luộc",
        "ức gà xay",
        "Vietnamese boiled chicken breast uc ga",
    ],
    "hu_tieu": [
        "hủ tiếu",
        "hủ tiếu nam vang",
        "hủ tiếu nam vang tô",
        "Vietnamese hu tieu noodle soup",
    ],
    "pho_ga": [
        "phở gà tô",
        "Vietnamese chicken pho pho ga",
    ],
    "com_ga": [
        "cơm gà hội an đĩa",
        "Vietnamese chicken rice com ga",
    ],
    "banh_canh": [
        "bánh canh cua tô",
        "Vietnamese thick noodle soup banh canh",
    ],
    "mi_xao": [
        "mì xào",
        "mì xào bò",
        "mì xào hải sản",
        "mì xào giòn",
        "Vietnamese stir-fried noodles mi xao",
    ],
    "banh_flan": [
        "bánh flan caramel",
        "Vietnamese flan caramel dessert",
    ],
}
 
 
# ─────────────────────────────────────────────────────────
# DOWNLOADER
# ─────────────────────────────────────────────────────────
def is_valid_image(content: bytes) -> bool:
    """Kiểm tra bytes có phải ảnh hợp lệ không."""
    try:
        img = Image.open(BytesIO(content))
        w, h = img.size
        if w < MIN_IMAGE_SIZE[0] or h < MIN_IMAGE_SIZE[1]:
            return False
        # Chấp nhận RGB, RGBA, L (grayscale)
        if img.mode not in ("RGB", "RGBA", "L"):
            img = img.convert("RGB")
        return True
    except Exception:
        return False
 
 
def normalize_image(content: bytes, save_path: Path) -> bool:
    """Convert về JPEG chuẩn, loại bỏ EXIF."""
    try:
        img = Image.open(BytesIO(content)).convert("RGB")
        # Resize nếu quá lớn (tăng tốc training)
        if img.width > 1024 or img.height > 1024:
            img.thumbnail((1024, 1024), Image.LANCZOS)
        img.save(save_path, "JPEG", quality=90, optimize=True)
        return True
    except Exception:
        return False
 
 
def download_category(label: str, queries: list[str], output_dir: Path, target_count: int):
    """Download ảnh cho 1 category."""
    category_dir = output_dir / label
    category_dir.mkdir(parents=True, exist_ok=True)
 
    existing = list(category_dir.glob("*.jpg"))
    if len(existing) >= target_count:
        print(f"  ✅ {label}: đã có {len(existing)} ảnh, bỏ qua.")
        return
 
    downloaded = len(existing)
    seen_hashes: set[str] = set()
 
    # Hash các ảnh đã có để tránh duplicate
    for f in existing:
        seen_hashes.add(hashlib.md5(f.read_bytes()).hexdigest())
 
    with DDGS() as ddgs:
        for query in queries:
            if downloaded >= target_count:
                break
 
            try:
                results = list(ddgs.images(
                    query,
                    max_results=target_count * 2,  # lấy dư để filter
                    type_image="photo",
                    size="Medium",
                ))
            except Exception as e:
                print(f"  ⚠️  Query '{query}' thất bại: {e}")
                continue
 
            for result in results:
                if downloaded >= target_count:
                    break
 
                url = result.get("image", "")
                if not url:
                    continue
 
                try:
                    resp = requests.get(url, timeout=8,
                                        headers={"User-Agent": "Mozilla/5.0"})
                    if resp.status_code != 200:
                        continue
 
                    content = resp.content
 
                    # Kiểm tra kích thước file
                    if len(content) > MAX_FILE_SIZE_MB * 1024 * 1024:
                        continue
 
                    # Kiểm tra ảnh hợp lệ
                    if not is_valid_image(content):
                        continue
 
                    # Tránh duplicate
                    img_hash = hashlib.md5(content).hexdigest()
                    if img_hash in seen_hashes:
                        continue
                    seen_hashes.add(img_hash)
 
                    # Lưu ảnh
                    save_path = category_dir / f"{downloaded + 1:03d}.jpg"
                    if normalize_image(content, save_path):
                        downloaded += 1
 
                    time.sleep(DELAY_BETWEEN_REQUESTS)
 
                except Exception:
                    continue  # Bỏ qua ảnh lỗi, tiếp tục
 
    print(f"  📦 {label}: {downloaded}/{target_count} ảnh")
 
 
# ─────────────────────────────────────────────────────────
# VALIDATION REPORT
# ─────────────────────────────────────────────────────────
def print_dataset_report(output_dir: Path):
    """In báo cáo thống kê dataset."""
    print("\n" + "=" * 50)
    print("📊 DATASET REPORT")
    print("=" * 50)
 
    total = 0
    categories_ready = 0
    categories_warning = []
 
    for category in FOOD_CATEGORIES:
        cat_dir = output_dir / category
        count = len(list(cat_dir.glob("*.jpg"))) if cat_dir.exists() else 0
        total += count
 
        status = "✅" if count >= 50 else ("⚠️ " if count >= 30 else "❌")
        if count >= 50:
            categories_ready += 1
        elif count < 30:
            categories_warning.append(category)
 
        print(f"  {status} {category:<25} {count:>3} ảnh")
 
    print(f"\n  Tổng ảnh:    {total}")
    print(f"  Ready (≥50): {categories_ready}/{len(FOOD_CATEGORIES)}")
 
    if categories_warning:
        print(f"\n  ⚠️  Cần thêm ảnh: {', '.join(categories_warning)}")
 
    if categories_ready == len(FOOD_CATEGORIES):
        print("\n  🚀 Dataset sẵn sàng để train!")
    else:
        print("\n  ℹ️  Có thể train với data hiện tại, accuracy sẽ thấp hơn.")
 
    print("=" * 50)
 
 
# ─────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────
def main():
    print("🍜 FitTrack Pro — Training Data Downloader")
    print(f"   Output: {OUTPUT_DIR.resolve()}")
    print(f"   Target: {IMAGES_PER_CATEGORY} ảnh/category × {len(FOOD_CATEGORIES)} categories\n")
 
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
 
    for label, queries in tqdm(FOOD_CATEGORIES.items(), desc="Downloading categories"):
        print(f"\n⬇️  Đang download: {label}")
        download_category(label, queries, OUTPUT_DIR, IMAGES_PER_CATEGORY)
 
    print_dataset_report(OUTPUT_DIR)
    print("\n✅ Xong! Chạy POST /api/foods/train-model để train.")
 
 
if __name__ == "__main__":
    main()
