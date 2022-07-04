Data files are uint8 arrays (1 byte per node+seed) in a pure binary format, where array\[node_id_INDEX \* jewel_seed_Size + jewel_seed_offset\] = index_of_Change

	node_id_INDEX is given by node_indices.csv
	jewel_seed_Size is the number of seeds for a given jewel (note elegant hubris seeds are divided by 20)
	jewel_seed_offset is the value above the minimum  (note elegant hubris seeds are divided by 20)

index_of_Change is dependant on value, 

	additions are index_of_Change = _rid in alternate_passive_additions.json
	replacements are index_of_Change - 94 = _rid in alternate_passive_skills.json
	if no change is made to the node then index is 249

Glorious Vanity is significantly more complex, and is described below

The suggested method for parsing non-GV is to :
- convert/load it to the above described unit8 array, 
- create a list of valid notables you want (by above index) (only do 1 jewel socket at a time)
- create an array of weights, (most will be 0)
- create an array of valid seeds
- SEEK to a location, and input the value as the index into your weight_array to obtain the weight of the node, add this value to the value in your seed_array
- once you have gone through all nodes/seeds, go through the list and remove any that fall below some chosen threshold
- then sort the seed_array from largest to smallest

Glorious Vanity has its own node indices as it also include all the small nodes, it has a nodecount of 1678, as such it is much larger, and thus is compressed for github

next, each node can have multiple changes, and each change comes with an associated stat value (roll value for roll range), as such there is a header section first with the size of each, size of 1 for each stat/value
- eg if it has 4 stats it has an 8 in the header (4 stats with 1 roll each)
- if it has 1 stat with 2 rolls it has a 3 in the header
 
because each node has more than 1 value associated with it, the recommended method for parsing it is a 2d array, to create it:
- create a header of size: nodeCount \* maxSeedIndex
- create a variable length 2d array for Data. the first coordinate, similar to the other jewels, will be the index of the node index \* maxSeedIndex + seed index, but since each stat needs multiple bytes, the value at that coordinate will be an array of bytes instead of just a single byte
- load in the data an array of size equal to the value in header\[i\] into data\[i\] which gives you the full 2d array
- when you iterate over it like in the non-GV method you can then access specific elements to check if its the change you want, or use the values for weighted sums
- eg for 1 stat (header\[i\]==2) its, data\[i\]\[0\] to check the 0th change, and data\[i\]\[1\] to get the value of the 0th change, where "i" is the same index formula as the non-GV version, but uses the GV indices csv
- note that its all the stats then all the rolls, not stat, roll, stat, roll, eg for 3 stats its \[0\]stat1, \[1\]stat2, \[2\]stat3, \[3\]roll1, \[4\]roll2, \[5\]roll3
- theres only 4 cases, 1 stat 1 roll, 1 stat 2 rolls, 3 stats 3 rolls, 4 stats 4 rolls

basic example of parsing them in c# provided by Oxidisedgearz in examples folder

list of which nodes are in range of what jewel socket can be found in Jewel_Node_Link.json

example of how to load it by @zao

Python
```python
lut = pathlib.Path('Militant Faith').read_bytes()
```

C++
```c++
std::ifstream is("Militant Faith", std::ios::binary);
auto file_size = is.seekg(0, std::ios::end); // or use std::filesystem::file_size on a path
is.seekg(0, std::ios::beg);
std::vector<uint8_t> lut(file_size);
is.read((char*)lut.data(), lut.size());
```

example of grabbing a single node

take a random node, lets say lethal pride, Lava Lash, seed 10116 (as it ends up easier), this gives you an index of 0 + 116, the byte at that value is 52 (a "4" in ascii) which corresponds with "karui_notable_add_burning_damage", which is what it is ![](https://cdn.discordapp.com/attachments/175290321695932416/993077938847219722/unknown.png)
