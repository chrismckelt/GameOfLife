GameOfLife
==========

http://en.wikipedia.org/wiki/Conway%27s_Game_of_Life


Performace improvement ideas.

-- Use arrays rather than a list to store the matrix (as iterating through lists is expensive vs going straight to the index)
-- Concurrently walk the cell matrix & find alive neighbours
-- Upon cell birth, count and store alive neighbours for next round optimisation
-- Investigate fastest way in c# - BitArray/Shift/better cache optimisation
-- http://tomasp.net/blog/accelerator-life-game.aspx/

NB:// Extensions use FirstOrDefault rather than SingleOrDefault for speed (will exit collection iteration once found rather that continuing) - maybe switch to indexes if performance is a problem
