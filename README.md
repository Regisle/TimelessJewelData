Data files are uint8 arrays (1 byte per node+seed), where array[node_id_INDEX * jewel_seed_Size + jewel_seed] = index_of_Change
node_id_INDEX is given by node_indices.csv
jewel_seed_Size is the number of seeds for a given jewel (note elegent hubris is divided by 20)
jewel_seed is self explanatory  (note elegent hubris is divided by 20)
index_of_Change is dependant on value, 
	additions are index_of_Change = _rid in alternate_passive_additions.json
	replacements are index_of_Change = _rid - 94 in alternate_passive_additions.json
	if no change is made to the node then index is 249

Glorious Vanity not included becouse its more complex (and around 50% larger than the other 4 combined)

The suggested method for parsing it is to convert it to the above described unit8 array, 
create a list of valid notables you want (by above index) (only do 1 jewel socket at a time)
create an array of weights, (most will be 0)
create an array of valid seeds
SEEK to a location, and input the value as the index into your weight_array to obtain the weight of the node, add this value to the value in your seed_array
once you have gone through all nodes/seeds, go through the list and remove any that fall below some chosen threshold
then sort the seed_array from largest to smallest