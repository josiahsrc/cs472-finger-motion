# This builder will build a dataset which uses vectors and magnitudes for the 
# finger joints instead of raw points.
import setup

import pandas as pd
import numpy as np
import sys
import data_utils as dutils

N_FEATURES = 10
N_LABELS = 4

argc = len(sys.argv)
argv = sys.argv

if argc != 3:
    print('Usage: python databuilder_raw_and_deltas.py <path_to_raw_dataset.csv> <path_to_output_dataset.csv>')
    print('')
    print('Example: python databuilder_raw_and_deltas.py data/walk00_raw_points.csv data/walk00_raw_and_deltas.csv')
    exit(1)
    
arg_idataset = argv[1]
arg_odataset = argv[2]
    
data = pd.read_csv(arg_idataset).values
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

RES_FEATURES = 20
RES_LABELS = 4
result = np.empty((0, RES_FEATURES + RES_LABELS))

prev_x = None

assert X.shape[0] == y.shape[0]
for i in range(X.shape[0]):
    curr_x, curr_t = X[i, :], y[i, :]
    
    if prev_x is None:
        prev_x = np.copy(curr_x)
        continue
    
    prev_pt_grn = prev_x[0:2]
    prev_pt_blu = prev_x[2:4]
    prev_pt_prp = prev_x[4:6]
    prev_pt_org = prev_x[6:8]
    prev_pt_pnk = prev_x[8:10]
    
    pt_grn = curr_x[0:2]
    pt_blu = curr_x[2:4]
    pt_prp = curr_x[4:6]
    pt_org = curr_x[6:8]
    pt_pnk = curr_x[8:10]
    
    # Directional finger distance vectors
    diff_grn = pt_grn - prev_pt_grn
    diff_blu = pt_blu - prev_pt_blu
    diff_prp = pt_prp - prev_pt_prp
    diff_org = pt_org - prev_pt_org
    diff_pnk = pt_pnk - prev_pt_pnk
    
    # Row to append
    row = np.array([
        diff_grn[0], diff_grn[1],
        diff_blu[0], diff_blu[1],
        diff_prp[0], diff_prp[1],
        diff_org[0], diff_org[1],
        diff_pnk[0], diff_pnk[1],
        
        pt_grn[0] - pt_grn[0], pt_grn[1] - pt_grn[1],
        pt_blu[0] - pt_grn[0], pt_blu[1] - pt_grn[1],
        pt_prp[0] - pt_grn[0], pt_prp[1] - pt_grn[1],
        pt_org[0] - pt_grn[0], pt_org[1] - pt_grn[1],
        pt_pnk[0] - pt_grn[0], pt_pnk[1] - pt_grn[1],
        
        curr_t[0],
        curr_t[1],
        curr_t[2],
        curr_t[3],
    ]).reshape(1, -1)
    
    result = np.concatenate((result, row), axis=0)
    prev_x = curr_x

dutils.save_np_array_to_csv(result, arg_odataset)
