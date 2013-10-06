GameOfLife
==========

http://en.wikipedia.org/wiki/Conway%27s_Game_of_Life


Contrasting different implementations

Indexed Array
List
HashSet
ConcurrentBag
ConcurrentQueue
ConcurrentStack



TODO

-- Run on GPU http://tomasp.net/blog/accelerator-life-game.aspx/

NB:// Extensions use FirstOrDefault rather than SingleOrDefault for speed (will exit collection iteration once found rather that continuing) - maybe switch to indexes if performance is a problem
