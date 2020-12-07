# This file allows python scripts in this directory to be able
# to import files in the parent directory (e.g. the tools).

import sys
import os

path = os.path.dirname(os.path.abspath(__file__))
sys.path.append(os.path.join(path, '../'))
