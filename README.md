Data files are uint8 arrays (1 byte per node+seed) in a pure binary format, where array[node_id_INDEX * jewel_seed_Size + jewel_seed_offset] = index_of_Change

	node_id_INDEX is given by node_indices.csv
	jewel_seed_Size is the number of seeds for a given jewel (note elegant hubris seeds are divided by 20)
	jewel_seed_offset is the value above the minimum  (note elegant hubris seeds are divided by 20)

index_of_Change is dependant on value, 

	additions are index_of_Change = _rid in alternate_passive_additions.json
	replacements are index_of_Change - 94 = _rid in alternate_passive_skills.json
	if no change is made to the node then index is 249

Glorious Vanity not included because it's more complex (and around 50% larger than the other 4 combined)

The suggested method for parsing it is to :
- convert/load it to the above described unit8 array, 
- create a list of valid notables you want (by above index) (only do 1 jewel socket at a time)
- create an array of weights, (most will be 0)
- create an array of valid seeds
- SEEK to a location, and input the value as the index into your weight_array to obtain the weight of the node, add this value to the value in your seed_array
- once you have gone through all nodes/seeds, go through the list and remove any that fall below some chosen threshold
- then sort the seed_array from largest to smallest


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
