# Findings

Some stuff to use in the report.

- The initial datasets (raw data) was unreliable because the points were arbitrary. Any attempts to learn patterns wouldn't be able to avg in on a valid solution.
- How do we orient the points? Should we orient them?
- We could have another color, to orient the hips

## Walk00 dataset (PRIOR TO THE TWO HIP POINTS, 6 POINTS TOTAL)

### Just deltas

Produced an MSE of MSE=92.05761116233987, but the outputs were in deltas (which didn't help solve the problem of predicting rotations, so can't be relied on)
- R^2=0.6188269695015016
- MSE=96.72722601181987

### Raw points

Didn't perform well at all
- R^2=-0.005388996265980911
- MSE=679.01143141584

### Raw and deltas

Performed ok, but still not reliable enough
- R^2=0.7488249636271167
- MSE=162.2432559778133

### Vectors an mags

Performed very poorly
- R^2=0.301314745537778
- MSE=411.8210197760544

### Big hefty

Started of on this one by using angles, points, prev points, deltas between prev and current points, normalized vectors of fingers, magnitudes. By doing this, got:
- R^2=0.6886454216590564
- MSE=195.74524579439026

