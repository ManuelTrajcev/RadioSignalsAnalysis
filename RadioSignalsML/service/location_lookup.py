from __future__ import annotations

import json
from pathlib import Path
from typing import Dict, Mapping


def load_location_lookup(path: Path) -> Mapping[str, Dict[str, str]]:
    if not path.exists():
        return {}
    with path.open("r", encoding="utf-8") as fh:
        data = json.load(fh)
    # normalise keys to plain strings so lookups from ints succeed
    return {str(k): v for k, v in data.items() if isinstance(v, dict)}
