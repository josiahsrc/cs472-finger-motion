# This databuilder tries to infer features from the raw datapoints to make the data more understandable.
# This dataset uses
#
# This databuilder derives the following features from the dataset
# - Normalized vectors
# - Magnitudes
# - Angles (between joints)
# - Points
# - Previous points
# - Differences

# TODO: Make features, angle, etc...

import pandas as pd
import numpy as np
import sys
import data_utils as dutils
import setup
import math

def rads_between(v1, v2):
    v1_u = v1 / np.linalg.norm(v1)
    v2_u = v2 / np.linalg.norm(v2)
    return np.arccos(np.clip(np.dot(v1_u, v2_u), -1.0, 1.0))

N_FEATURES = 10
N_LABELS = 4

argc = len(sys.argv)
argv = sys.argv

if argc != 3:
    print('Usage: python databuilder_big_hefty.py.py <path_to_raw_dataset.csv> <path_to_output_dataset.csv>')
    print('')
    print('Example: python databuilder_big_hefty.py.py data/walk00_raw_points.csv data/walk00_big_hefty.csv')
    exit(1)

arg_idataset = argv[1]
arg_odataset = argv[2]

data = pd.read_csv(arg_idataset).values
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

RES_FEATURES = 45
RES_LABELS = 4
result = np.empty((0, RES_FEATURES + RES_LABELS))

prev_x = None

# For each instance in the dataset
assert X.shape[0] == y.shape[0]
for i in range(X.shape[0]):
    curr_x, curr_t = X[i, :], y[i, :]

    if prev_x is None:
        prev_x = np.copy(curr_x)
        continue

    # FEATURE: Previous points (5x2=10)
    prev_pt_grn = prev_x[0:2]
    prev_pt_blu = prev_x[2:4] - prev_pt_grn
    prev_pt_prp = prev_x[4:6] - prev_pt_grn
    prev_pt_org = prev_x[6:8] - prev_pt_grn
    prev_pt_pnk = prev_x[8:10] - prev_pt_grn
    prev_pt_grn = np.array([0.0, 0.0])

    # FEATURE: Current points (5x2=10)
    curr_pt_grn = curr_x[0:2]
    curr_pt_blu = curr_x[2:4] - curr_pt_grn
    curr_pt_prp = curr_x[4:6] - curr_pt_grn
    curr_pt_org = curr_x[6:8] - curr_pt_grn
    curr_pt_pnk = curr_x[8:10] - curr_pt_grn
    curr_pt_grn = np.array([0.0, 0.0])

    # FEATURE: Directional finger distance vectors (5x2=10)
    diff_pt_grn = curr_pt_grn - prev_pt_grn
    diff_pt_blu = curr_pt_blu - prev_pt_blu
    diff_pt_prp = curr_pt_prp - prev_pt_prp
    diff_pt_org = curr_pt_org - prev_pt_org
    diff_pt_pnk = curr_pt_pnk - prev_pt_pnk

    # Difference vectors between the different joints
    curr_vec_grn2blu = curr_pt_blu - curr_pt_grn
    curr_vec_blu2org = curr_pt_org - curr_pt_blu
    curr_vec_grn2prp = curr_pt_prp - curr_pt_grn
    curr_vec_prp2pnk = curr_pt_pnk - curr_pt_prp

    # FEATURE: Magnitudes the difference vectors between the different joints (4x1=4)
    curr_mag_grn2blu = (curr_vec_grn2blu ** 2).sum() ** .5
    curr_mag_blu2org = (curr_vec_blu2org ** 2).sum() ** .5
    curr_mag_grn2prp = (curr_vec_grn2prp ** 2).sum() ** .5
    curr_mag_prp2pnk = (curr_vec_prp2pnk ** 2).sum() ** .5

    # FEATURE: Normalized difference vectors between the different joints (4x2=8)
    curr_nrm_grn2blu = curr_vec_grn2blu / curr_mag_grn2blu
    curr_nrm_blu2org = curr_vec_blu2org / curr_mag_blu2org
    curr_nrm_grn2prp = curr_vec_grn2prp / curr_mag_grn2prp
    curr_nrm_prp2pnk = curr_vec_prp2pnk / curr_mag_prp2pnk
    
    # FEATURE: Angle (in radians) between the joints (3x1=3)
    curr_angl_aknee = rads_between(curr_nrm_grn2blu, curr_nrm_blu2org)
    curr_angl_bknee = rads_between(curr_nrm_grn2prp, curr_nrm_prp2pnk)
    curr_angl_groin = rads_between(curr_nrm_grn2blu, curr_nrm_grn2prp)
    
    # Row to append
    row = np.array([
        prev_pt_grn[0], prev_pt_grn[1], 
        prev_pt_blu[0], prev_pt_blu[1],
        prev_pt_prp[0], prev_pt_prp[1],
        prev_pt_org[0], prev_pt_org[1],
        prev_pt_pnk[0], prev_pt_pnk[1],
        
        curr_pt_grn[0], curr_pt_grn[1], 
        curr_pt_blu[0], curr_pt_blu[1],
        curr_pt_prp[0], curr_pt_prp[1],
        curr_pt_org[0], curr_pt_org[1],
        curr_pt_pnk[0], curr_pt_pnk[1],
        
        diff_pt_grn[0], diff_pt_grn[1], 
        diff_pt_blu[0], diff_pt_blu[1],
        diff_pt_prp[0], diff_pt_prp[1],
        diff_pt_org[0], diff_pt_org[1],
        diff_pt_pnk[0], diff_pt_pnk[1],
        
        curr_mag_grn2blu,
        curr_mag_blu2org,
        curr_mag_grn2prp,
        curr_mag_prp2pnk,
        
        curr_nrm_grn2blu[0], curr_nrm_grn2blu[1], 
        curr_nrm_blu2org[0], curr_nrm_blu2org[1],
        curr_nrm_grn2prp[0], curr_nrm_grn2prp[1],
        curr_nrm_prp2pnk[0], curr_nrm_prp2pnk[1],
        
        curr_angl_aknee,
        curr_angl_bknee,
        curr_angl_groin,
        
        curr_t[0],
        curr_t[1],
        curr_t[2],
        curr_t[3],
    ]).reshape(1, -1)
    
    result = np.concatenate((result, row), axis=0)

    # Update the previous points
    prev_x = curr_x

dutils.save_np_array_to_csv(result, arg_odataset)
