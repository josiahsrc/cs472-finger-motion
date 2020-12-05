# This databuilder 

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

def dot_between(v1, v2):
    v1_u = v1 / np.linalg.norm(v1)
    v2_u = v2 / np.linalg.norm(v2)
    return np.clip(np.dot(v1_u, v2_u), -1.0, 1.0)

# Files
N_FEATURES = 12
N_LABELS = 4
input_datasets = [
    'data/crouchraw_josiah_00.csv',   
    'data/crouchraw_josiah_01.csv',   
    'data/crouchraw_josiah_02.csv', 
    'data/crouchraw_josiah_03.csv',   
    'data/crouchraw_josiah_04.csv',   
    'data/crouchraw_josiah_05.csv',   
    'data/crouchraw_josiah_06.csv',   
]
output_dataset = 'data/bighefty2.csv'

# Concat all input files
data = np.empty((0, N_FEATURES + N_LABELS))
for i in range(len(input_datasets)):
    d = pd.read_csv(input_datasets[i]).values
    data = np.concatenate((data, d), axis=0)
    
# Prepare input data
X, y = dutils.prepare_data_imputed_norm(data[:, :N_FEATURES], data[:, N_FEATURES:])

RES_FEATURES = 58
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

    # FEATURE: Previous points (6x2=12)
    TEMP_PREV_ORIGIN = (prev_x[0:2] + prev_x[2:4]) / 2
    prev_pt_grL = prev_x[0:2]
    prev_pt_grR = prev_x[2:4]
    prev_pt_blu = prev_x[4:6] - TEMP_PREV_ORIGIN
    prev_pt_prp = prev_x[6:8] - TEMP_PREV_ORIGIN
    prev_pt_org = prev_x[8:10] - TEMP_PREV_ORIGIN
    prev_pt_pnk = prev_x[10:12] - TEMP_PREV_ORIGIN
    
    # FEATURE: Current points (6x2=12)
    TEMP_CURR_ORIGIN = (curr_x[0:2] + curr_x[2:4]) / 2
    curr_pt_grL = curr_x[0:2]
    curr_pt_grR = curr_x[2:4]
    curr_pt_blu = curr_x[4:6] - TEMP_CURR_ORIGIN
    curr_pt_prp = curr_x[6:8] - TEMP_CURR_ORIGIN
    curr_pt_org = curr_x[8:10] - TEMP_CURR_ORIGIN
    curr_pt_pnk = curr_x[10:12] - TEMP_CURR_ORIGIN
    
    # FEATURE: Directional finger distance vectors (6x2=12)
    diff_pt_grL = curr_pt_grL - prev_pt_grL
    diff_pt_grR = curr_pt_grR - prev_pt_grR
    diff_pt_blu = curr_pt_blu - prev_pt_blu
    diff_pt_prp = curr_pt_prp - prev_pt_prp
    diff_pt_org = curr_pt_org - prev_pt_org
    diff_pt_pnk = curr_pt_pnk - prev_pt_pnk
    
    # Difference vectors between the different joints
    curr_vec_grL2blu = curr_pt_blu - curr_pt_grL
    curr_vec_blu2org = curr_pt_org - curr_pt_blu
    curr_vec_grR2prp = curr_pt_prp - curr_pt_grR
    curr_vec_prp2pnk = curr_pt_pnk - curr_pt_prp
    curr_vec_grL2org = curr_pt_org - curr_pt_grL
    curr_vec_grR2pnk = curr_pt_pnk - curr_pt_grR

    # FEATURE: Magnitudes the difference vectors between the different joints (6x1=6)
    curr_mag_grL2blu = (curr_vec_grL2blu ** 2).sum() ** .5
    curr_mag_blu2org = (curr_vec_blu2org ** 2).sum() ** .5
    curr_mag_grR2prp = (curr_vec_grR2prp ** 2).sum() ** .5
    curr_mag_prp2pnk = (curr_vec_prp2pnk ** 2).sum() ** .5
    curr_mag_grL2org = (curr_vec_grL2org ** 2).sum() ** .5
    curr_mag_grR2pnk = (curr_vec_grR2pnk ** 2).sum() ** .5

    # FEATURE: Normalized difference vectors between the different joints (6x2=12)
    curr_nrm_grL2blu = curr_vec_grL2blu / curr_mag_grL2blu
    curr_nrm_blu2org = curr_vec_blu2org / curr_mag_blu2org
    curr_nrm_grR2prp = curr_vec_grR2prp / curr_mag_grR2prp
    curr_nrm_prp2pnk = curr_vec_prp2pnk / curr_mag_prp2pnk
    curr_nrm_grL2org = curr_vec_grL2org / curr_mag_grL2org
    curr_nrm_grR2pnk = curr_vec_grR2pnk / curr_mag_grR2pnk
    
    # FEATURE: Angle (in radians) between the joints (4x1=1)
    curr_dot_kneeL = dot_between(curr_nrm_grL2blu, curr_nrm_blu2org)
    curr_dot_kneeR = dot_between(curr_nrm_grR2prp, curr_nrm_prp2pnk)
    curr_angl_kneeL = rads_between(curr_nrm_grL2blu, curr_nrm_blu2org)
    curr_angl_kneeR = rads_between(curr_nrm_grR2prp, curr_nrm_prp2pnk)
    
    # Row to append
    row = np.array([
        curr_mag_grL2blu,
        curr_mag_blu2org,
        curr_mag_grR2prp,
        curr_mag_prp2pnk,
        curr_mag_grL2org,
        curr_mag_grR2pnk,
        
        curr_dot_kneeL,
        curr_dot_kneeR,
        curr_angl_kneeL,
        curr_angl_kneeR,
        
        curr_nrm_grL2blu[0], curr_nrm_grL2blu[1],
        curr_nrm_blu2org[0], curr_nrm_blu2org[1],
        curr_nrm_grR2prp[0], curr_nrm_grR2prp[1],
        curr_nrm_prp2pnk[0], curr_nrm_prp2pnk[1],
        curr_nrm_grL2org[0], curr_nrm_grL2org[1],
        curr_nrm_grR2pnk[0], curr_nrm_grR2pnk[1],
        
        prev_pt_grL[0], prev_pt_grL[1],
        prev_pt_grR[0], prev_pt_grR[1],
        prev_pt_blu[0], prev_pt_blu[1],
        prev_pt_prp[0], prev_pt_prp[1],
        prev_pt_org[0], prev_pt_org[1],
        prev_pt_pnk[0], prev_pt_pnk[1],
        
        curr_pt_grL[0], curr_pt_grL[1],
        curr_pt_grR[0], curr_pt_grR[1],
        curr_pt_blu[0], curr_pt_blu[1],
        curr_pt_prp[0], curr_pt_prp[1],
        curr_pt_org[0], curr_pt_org[1],
        curr_pt_pnk[0], curr_pt_pnk[1],
        
        diff_pt_grL[0], diff_pt_grL[1],
        diff_pt_grR[0], diff_pt_grR[1],
        diff_pt_blu[0], diff_pt_blu[1],
        diff_pt_prp[0], diff_pt_prp[1],
        diff_pt_org[0], diff_pt_org[1],
        diff_pt_pnk[0], diff_pt_pnk[1],
        
        curr_t[0],
        curr_t[1],
        curr_t[2],
        curr_t[3],
    ]).reshape(1, -1)
    
    result = np.concatenate((result, row), axis=0)

    # Update the previous points
    prev_x = curr_x

dutils.save_np_array_to_csv(result, output_dataset)
