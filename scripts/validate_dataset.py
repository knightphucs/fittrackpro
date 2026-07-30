import os
import hashlib
from pathlib import Path
from PIL import Image
from collections import defaultdict
 
TRAINING_DATA_DIR = Path(os.environ.get(
    "TRAINING_DATA_PATH",
    Path(__file__).parent.parent.parent / "fittrackpro-data" / "training-data"
))
MIN_WIDTH  = 100
MIN_HEIGHT = 100
 
 
def validate_and_clean():
    print("🔍 Validating training dataset...\n")
 
    total_valid   = 0
    total_removed = 0
    duplicates    = 0
    category_stats = {}
 
    for category_dir in sorted(TRAINING_DATA_DIR.iterdir()):
        if not category_dir.is_dir():
            continue
 
        label = category_dir.name
        valid_count   = 0
        removed_count = 0
        hashes: set[str] = set()
 
        for img_path in list(category_dir.glob("*.jpg")) + list(category_dir.glob("*.png")):
            remove_reason = None
 
            try:
                content = img_path.read_bytes()
 
                # 1. Duplicate check
                h = hashlib.md5(content).hexdigest()
                if h in hashes:
                    remove_reason = "duplicate"
                    duplicates += 1
                else:
                    hashes.add(h)
 
                # 2. Valid image check
                if remove_reason is None:
                    img = Image.open(img_path)
                    w, h_px = img.size
 
                    if w < MIN_WIDTH or h_px < MIN_HEIGHT:
                        remove_reason = f"too_small ({w}x{h_px})"
 
                    elif img.mode not in ("RGB", "RGBA", "L"):
                        remove_reason = f"bad_mode ({img.mode})"
 
            except Exception as e:
                remove_reason = f"corrupted ({str(e)[:30]})"
 
            if remove_reason:
                print(f"  ❌ Removed {img_path.name} [{label}]: {remove_reason}")
                img_path.unlink()
                removed_count += 1
            else:
                valid_count += 1
 
        category_stats[label] = valid_count
        total_valid   += valid_count
        total_removed += removed_count
 
    # ── Report ──
    print("\n" + "=" * 55)
    print("📊 DATASET VALIDATION REPORT")
    print("=" * 55)
    print(f"  {'Category':<25} {'Count':>6}  {'Status'}")
    print(f"  {'-'*25} {'-'*6}  {'-'*10}")
 
    for label, count in sorted(category_stats.items()):
        if count >= 60:
            status = "✅ Ready"
        elif count >= 30:
            status = "⚠️  Low"
        else:
            status = "❌ Insufficient"
        print(f"  {label:<25} {count:>6}  {status}")
 
    print(f"\n  Total valid images : {total_valid}")
    print(f"  Removed (bad/dup)  : {total_removed}")
    print(f"  Duplicates found   : {duplicates}")
 
    # Imbalance check
    if category_stats:
        max_count = max(category_stats.values())
        min_count = min(category_stats.values())
        ratio = max_count / max(min_count, 1)
        if ratio > 3:
            print(f"\n  ⚠️  Class imbalance detected! Max/Min ratio = {ratio:.1f}x")
            print(f"     Nên thêm ảnh cho categories ít ảnh để cân bằng.")
 
    ready = sum(1 for c in category_stats.values() if c >= 50)
    print(f"\n  Categories ready: {ready}/{len(category_stats)}")
    print("=" * 55)
 
    if ready == len(category_stats):
        print("\n✅ Dataset clean và sẵn sàng train!")
    else:
        lacking = [l for l, c in category_stats.items() if c < 30]
        if lacking:
            print(f"\n❌ Cần thêm ảnh: {', '.join(lacking)}")
        else:
            print("\n⚠️  Dataset đủ dùng nhưng nên thêm ảnh để tăng accuracy.")
 
    return total_valid, category_stats
 
 
if __name__ == "__main__":
    validate_and_clean()