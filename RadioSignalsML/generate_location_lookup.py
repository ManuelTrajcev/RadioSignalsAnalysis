import json
from collections import Counter
from pathlib import Path
from train_signal_models import load_and_clean


def pick_mode(values):
    filtered = [v for v in values if isinstance(v, str) and v.strip()]
    if not filtered:
        return None
    counts = Counter(filtered)
    return max(counts.items(), key=lambda kv: (kv[1], kv[0]))[0]


def build_lookup(excel_path: Path, out_path: Path) -> dict:
    data = load_and_clean(str(excel_path))
    mapping = {}
    for place_id, group in data.groupby('place_id'):
        if not isinstance(place_id, str) or not place_id.strip():
            continue
        municipality = pick_mode(group['municipality']) or pick_mode(group['municipality'].astype(str))
        settlement = pick_mode(group['settlement']) or pick_mode(group['settlement_raw'])
        mapping[str(place_id).strip()] = {
            'municipality': municipality,
            'settlement': settlement,
        }
    out_path.write_text(json.dumps(mapping, ensure_ascii=False, indent=2), encoding='utf-8')
    return mapping


if __name__ == '__main__':
    excel = Path('signal_data.xlsx')
    out = Path('artifacts/location_lookup.json')
    built = build_lookup(excel, out)
    print(f"Wrote {len(built)} entries to {out}")
