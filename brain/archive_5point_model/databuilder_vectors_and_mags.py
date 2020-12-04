# This builder will build a dataset which uses vectors and magnitudes for the 
# finger joints instead of raw points.

import pandas as pd
import numpy as np
import sys
import data_utils as dutils
import setup

N_FEATURES = 10
N_LABELS = 4

argc = len(sys.argv)
argv = sys.argv

if argc != 3:
    print('Usage: python databuilder_vectors_and_mags.py <path_to_raw_dataset.csv> <path_to_output_dataset.csv>')
    print('')
    print('Example: python databuilder_vectors_and_mags.py data/walk00_raw_points.csv data/walk00_vectors_and_mags.csv')
    exit(1)
    
arg_idataset = argv[1]
arg_odataset = argv[2]
    
data = pd.read_csv(arg_idataset).values
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

RES_FEATURES = 12
RES_LABELS = 4
result = np.empty((0, RES_FEATURES + RES_LABELS))

assert X.shape[0] == y.shape[0]
for i in range(X.shape[0]):
    x, t = X[i, :], y[i, :]
    
    pt_grn = x[0:2]
    pt_blu = x[2:4]
    pt_prp = x[4:6]
    pt_org = x[6:8]
    pt_pnk = x[8:10]
    
    # Directional finger distance vectors
    diff_grn2blu = pt_blu - pt_grn
    diff_blu2org = pt_org - pt_blu
    diff_grn2prp = pt_prp - pt_grn
    diff_prp2pnk = pt_pnk - pt_prp
    
    # Magnitudes of distances
    mag_grn2blu = (diff_grn2blu ** 2).sum() ** .5
    mag_blu2org = (diff_blu2org ** 2).sum() ** .5
    mag_grn2prp = (diff_grn2prp ** 2).sum() ** .5
    mag_prp2pnk = (diff_prp2pnk ** 2).sum() ** .5
    
    # Normalized distances
    nrm_grn2blu = diff_grn2blu / mag_grn2blu
    nrm_blu2org = diff_blu2org / mag_blu2org
    nrm_grn2prp = diff_grn2prp / mag_grn2prp
    nrm_prp2pnk = diff_prp2pnk / mag_prp2pnk
    
    # Row to append
    row = np.array([
        mag_grn2blu,
        mag_blu2org,
        mag_grn2prp,
        mag_prp2pnk,
        
        nrm_grn2blu[0], nrm_grn2blu[1],
        nrm_blu2org[0], nrm_blu2org[1],
        nrm_grn2prp[0], nrm_grn2prp[1],
        nrm_prp2pnk[0], nrm_prp2pnk[1],
        
        t[0],
        t[1],
        t[2],
        t[3],
    ]).reshape(1, -1)
    
    result = np.concatenate((result, row), axis=0)

dutils.save_np_array_to_csv(result, arg_odataset)