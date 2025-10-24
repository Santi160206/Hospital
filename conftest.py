import os
import sys

# Ensure `backend/` is on sys.path when running tests from the repo root.
ROOT = os.path.dirname(__file__)
BACKEND_PATH = os.path.join(ROOT, 'backend')
if BACKEND_PATH not in sys.path:
    sys.path.insert(0, BACKEND_PATH)
