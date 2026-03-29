import os
import json
import requests
from pathlib import Path
from tabulate import tabulate
 
# ─────────────────────────────────────────────────────────
# CONFIG — thay đổi nếu khác
# ─────────────────────────────────────────────────────────
API_BASE   = "http://localhost:5000"
EMAIL      = "admin@fittrackpro.com"
PASSWORD   = "Admin@123456"
 
TEST_DATA_DIR = Path("scripts/training-data")  # dùng lại ảnh training để smoke test
 
 
# ─────────────────────────────────────────────────────────
# 1. Login → lấy JWT token
# ─────────────────────────────────────────────────────────
def get_token() -> str:
    resp = requests.post(f"{API_BASE}/api/auth/login", json={
        "email": EMAIL, "password": PASSWORD
    })
    resp.raise_for_status()
    token = resp.json()["accessToken"]
    print(f"✅ Login OK. Token: {token[:30]}...")
    return token
 
 
# ─────────────────────────────────────────────────────────
# 2. Test 1 ảnh
# ─────────────────────────────────────────────────────────
def recognize_image(token: str, image_path: Path) -> dict:
    with open(image_path, "rb") as f:
        files = {"image": (image_path.name, f, "image/jpeg")}
        headers = {"Authorization": f"Bearer {token}"}
        resp = requests.post(
            f"{API_BASE}/api/foods/recognize",
            files=files,
            headers=headers,
            timeout=30
        )
 
    if resp.status_code == 503:
        return {"error": "Model chưa sẵn sàng (503)"}
    if resp.status_code != 200:
        return {"error": f"HTTP {resp.status_code}: {resp.text[:100]}"}
 
    return resp.json()
 
 
# ─────────────────────────────────────────────────────────
# 3. Smoke test — lấy 1 ảnh mỗi category
# ─────────────────────────────────────────────────────────
def run_smoke_test(token: str):
    print("\n📸 Running smoke test (1 ảnh/category)...\n")
 
    results = []
    correct = 0
    total   = 0
 
    for category_dir in sorted(TEST_DATA_DIR.iterdir()):
        if not category_dir.is_dir():
            continue
 
        expected_label = category_dir.name
        images = list(category_dir.glob("*.jpg"))
        if not images:
            continue
 
        # Lấy ảnh ở giữa list (không phải ảnh đầu/cuối)
        test_image = images[len(images) // 2]
 
        result = recognize_image(token, test_image)
 
        if "error" in result:
            results.append([expected_label, "ERROR", result["error"], "—", "❌"])
            total += 1
            continue
 
        predicted  = result.get("predictedLabel", "?")
        confidence = result.get("confidence", 0)
        is_correct = predicted == expected_label
        if is_correct:
            correct += 1
        total += 1
 
        results.append([
            expected_label,
            predicted,
            f"{confidence:.1f}%",
            "✅" if is_correct else "❌",
            result.get("actionHint", "")[:40]
        ])
 
    # In bảng kết quả
    headers = ["Expected", "Predicted", "Confidence", "Correct", "Hint"]
    print(tabulate(results, headers=headers, tablefmt="rounded_outline"))
 
    accuracy = (correct / total * 100) if total > 0 else 0
    print(f"\n📊 Smoke Test Accuracy: {correct}/{total} = {accuracy:.1f}%")
 
    if accuracy >= 85:
        print("✅ Model đạt target accuracy >= 85%!")
    elif accuracy >= 70:
        print("⚠️  Model chấp nhận được nhưng nên train thêm.")
    else:
        print("❌ Model accuracy thấp. Cần thêm ảnh training và retrain.")
 
    return accuracy
 
 
# ─────────────────────────────────────────────────────────
# 4. Stress test — test N ảnh ngẫu nhiên mỗi category
# ─────────────────────────────────────────────────────────
def run_accuracy_test(token: str, samples_per_category: int = 5):
    print(f"\n🎯 Running accuracy test ({samples_per_category} ảnh/category)...\n")
 
    import random
    correct = 0
    total   = 0
    confusion: dict[str, dict[str, int]] = {}
 
    for category_dir in sorted(TEST_DATA_DIR.iterdir()):
        if not category_dir.is_dir():
            continue
 
        expected = category_dir.name
        images   = list(category_dir.glob("*.jpg"))
        if len(images) < samples_per_category:
            continue
 
        sample = random.sample(images, samples_per_category)
        confusion[expected] = {}
 
        for img_path in sample:
            result = recognize_image(token, img_path)
            if "error" in result:
                continue
 
            predicted = result.get("predictedLabel", "unknown")
            confusion[expected][predicted] = confusion[expected].get(predicted, 0) + 1
 
            if predicted == expected:
                correct += 1
            total += 1
 
    overall_acc = (correct / total * 100) if total > 0 else 0
    print(f"Overall Accuracy: {correct}/{total} = {overall_acc:.1f}%\n")
 
    # Top misclassifications
    print("🔄 Top Misclassifications:")
    misclass = []
    for true_label, preds in confusion.items():
        for pred_label, count in preds.items():
            if pred_label != true_label and count > 0:
                misclass.append([true_label, pred_label, count])
 
    misclass.sort(key=lambda x: -x[2])
    if misclass:
        print(tabulate(misclass[:10], headers=["True", "Predicted", "Count"],
                       tablefmt="simple"))
    else:
        print("  Không có misclassification!")
 
    return overall_acc
 
 
# ─────────────────────────────────────────────────────────
# 5. Test với ảnh tùy chỉnh
# ─────────────────────────────────────────────────────────
def test_single_image(token: str, image_path: str):
    print(f"\n🖼️  Testing: {image_path}")
    result = recognize_image(token, Path(image_path))
 
    if "error" in result:
        print(f"❌ Error: {result['error']}")
        return
 
    print(f"\n  Predicted : {result.get('displayName')} ({result.get('predictedLabel')})")
    print(f"  Confidence: {result.get('confidence', 0):.1f}%")
    print(f"  Confident : {'✅ Yes' if result.get('isConfident') else '⚠️  No'}")
    print(f"  Hint      : {result.get('actionHint')}")
 
    if result.get("matchedFood"):
        food = result["matchedFood"]
        print(f"\n  📊 Matched Food:")
        print(f"    Name    : {food.get('nameVi')} / {food.get('name')}")
        print(f"    Calories: {food.get('caloriesPerServing')} kcal")
        print(f"    Protein : {food.get('protein')}g")
        print(f"    Carbs   : {food.get('carbs')}g")
        print(f"    Fat     : {food.get('fat')}g")
 
    print("\n  Top 3 Candidates:")
    for i, c in enumerate(result.get("topCandidates", []), 1):
        calories_suffix = f"({c.get('calories')} kcal)" if c.get("calories") else ""
        print(
            f"    {i}. {c.get('displayName'):<20} {c.get('confidence', 0):.1f}%"
            f"  {calories_suffix}"
        )
 
 
# ─────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────
if __name__ == "__main__":
    import sys
 
    print("🍜 FitTrack Pro — Food Recognition Tester")
    print("=" * 50)
 
    token = get_token()
 
    if len(sys.argv) > 1:
        # python test_recognition.py path/to/image.jpg
        test_single_image(token, sys.argv[1])
    else:
        # Default: chạy smoke test
        run_smoke_test(token)
 
        # Chạy thêm accuracy test nếu có đủ data
        total_dirs = sum(1 for d in TEST_DATA_DIR.iterdir() if d.is_dir())
        if total_dirs >= 5:
            run_accuracy_test(token, samples_per_category=5)
