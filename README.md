# Using the data files

### Parsing Brutal Restraint, Elegant Hubris, Lethal Pride, and Militant Faith

Data files are uint8 arrays (1 byte per node+seed) in a pure binary format, where array\[node_id_INDEX \* jewel_seed_Size + jewel_seed_offset\] = index_of_Change

	node_id_INDEX is given by Node_Indices.csv
	jewel_seed_Size is the number of seeds for a given jewel (note elegant hubris seeds are divided by 20)
	jewel_seed_offset is the value above the minimum  (note elegant hubris seeds are divided by 20)

A list of which nodes are in range of what jewel socket can be found in Jewel_Node_Link.json

Also note that Node_Indices.csv contains indices for all modifiable nodes, but the non-Glorious Vanity jewels will only have data for notables (indices 0 to 390). Modifications to non-notables for these jewels are constant and thus have been omitted from their data files to save space.

index_of_Change is dependant on value, 

	additions are index_of_Change = _rid in alternate_passive_additions.json
	replacements are index_of_Change - 94 = _rid in alternate_passive_skills.json

Non-Glorious Vanity jewels are relatively simple to parse with this definition:

The suggested method for parsing non-GV is to:
- convert/load the entire file into a unit8 array, 
- create a list of valid notables you want (by above index) (only do 1 jewel socket at a time)
- create an array of weights, (most will be 0)
- create an array of valid seeds
- SEEK to a location in the uint8 array, and input the value as the index into your weight_array to obtain the weight of the node, add this value to the value in your seed_array
- once you have gone through all nodes/seeds, go through the list and remove any that fall below some chosen threshold
- then sort the seed_array from largest to smallest

It is also possible to SEEK directly to the byte in the file that holds the desired information, but for most use cases, doing so offers little to no benefit.

### Parsing Glorious Vanity

Glorious Vanity, having variable stat replacments for * *all* * nodes as well as multiple stats per notable with rolls on those stats, is a much larger file (hence compressed for github), and is more complex. Its parsing method is similar to the others, but requires a fair few tweaks:

like before:

	node_id_INDEX is given by Node_Indices.csv
	jewel_seed_Size is the number of seeds for a given jewel
	jewel_seed_offset is the value above the minimum

	additions are index_of_Change = _rid in alternate_passive_additions.json
	replacements are index_of_Change - 94 = _rid in alternate_passive_skills.json

and data files are in a pure binary format with one byte per piece of information; however, with glorious vanity, each node requires multiple pieces of information.


For a jewel, each node can have multiple changes, and each change comes with 1 or 2 associated stat values (roll value for roll range), as such there is a header section at the start of the file with the size of the amount of data each jewel holds for a given node.
- eg if it has 4 stats it has an 8 in the header (4 stats with 1 roll each)
- if it has 1 stat with 2 rolls it has a 3 in the header

To know the length of the header, we have the additional definition:

	nodeCount is the number of nodes in the Node_Indices.csv (currently 1678)

Because each node has more than 1 value associated with it, the recommended method for parsing it is a header array and a 2d array of data as follows:
- create a header of size: nodeCount \* maxSeedIndex
- create a variable length 2d array for Data. the first coordinate, similar to the other jewels, will be the index of the node index \* maxSeedIndex + seed index, but since each stat needs multiple bytes, the value at that coordinate will be an array of bytes instead of just a single byte
- load in the data an array of size equal to the value in header\[i\] into data\[i\] which gives you the full 2d array
- when you iterate over it like in the non-GV method you can then access specific elements to check if its the change you want, or use the values for weighted sums
- eg for 1 stat (header\[i\]==2) its, data\[i\]\[0\] to check the 0th change, and data\[i\]\[1\] to get the value of the 0th change, where "i" is the same index formula as the non-GV version, but can access the non-notable indices in Node_Indices.csv
- note that its all the stats then all the rolls, not stat, roll, stat, roll, eg for 3 stats its \[0\]stat1, \[1\]stat2, \[2\]stat3, \[3\]roll1, \[4\]roll2, \[5\]roll3
- theres only 4 cases: 1 stat 1 roll, 1 stat 2 rolls, 3 stats 3 rolls, 4 stats 4 rolls

Additionally, note that glorious vanity always replaces its nodes. Only Might of the Vaal and Legacy of the Vaal will ever have "additions" on them, and even those additions are really just replacements for the original notable's stats. Process them accordingly.

### Examples

a basic example in C# of parsing both kinds of jewels as part of a weighted search has been provided by OxidisedGearz and can be found in examples folder

#### examples of how to load data files into byte arrays by @zao:

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

#### example of grabbing a single node:

take a random node, lets say lethal pride, Lava Lash, seed 10116 (as it ends up easier), this gives you an index of 0 + 116, the byte at that value is 52 (a "4" in ascii) which corresponds with "karui_notable_add_burning_damage", which is what it is ![](https://cdn.discordapp.com/attachments/175290321695932416/993077938847219722/unknown.png)




# Generating the data files

Datafiles are generated using the DatafileGenerator (a Visual Studio project, C#).
It's built on top of a timeless jewel simulator, so the core file-format logic is in `Program.cs` while the rest models the PRNG and parses JSON data.

It will need an alternate passive additions json, an alternate passive skills json, and the most recent skill tree json. You'll also have to tell it where to output and whether you want the compressed or uncompressed files.    

Running it will output all 11 jewel datafiles, 1 lua file, and 1 csv file (note that compressed files larger than 5MB are split into multiple parts due to limitations within Path of Building).

## Abyss timeless jewels (versions 7-11)

The Abyss jewels introduced in 3.29 cannot use the original notable-by-seed layout:

- Versions 7-10 select 60 graph entries with a weighted walk starting at the jewel socket. Their output therefore depends on both the seed and socket.
- Version 11 transforms nodes on the path from the socket to the character start. The path itself is deliberately not stored because Path of Building already calculates it. Its extra ascendancy selection is stored per seed and ascendancy.

All integer fields below are little-endian. Stat rolls are signed 16-bit integers; negative rolls must not be read as unsigned values.

### Common header

| Field | Type | Description |
| --- | --- | --- |
| magic | 4 bytes | `ABYS` for versions 7-10 or `ABYN` for version 11 |
| formatVersion | uint8 | Currently 1 |
| jewelType | uint8 | Alternate tree version, 7 through 11 |
| seedMinimum | uint16 | First stored seed |
| seedMaximum | uint16 | Last stored seed |
| seedIncrement | uint16 | Seed step |

### `ABYS` socket-dependent body (versions 7-10)

The common header is followed by `socketCount` (uint8), `abyssSize` (uint8), and `socketCount` graph IDs (uint16). The socket IDs are sorted numerically and include all 21 sockets on the base passive tree, including the six Large Jewel Sockets.

Records then appear socket-major and seed-minor. For every socket and seed:

1. Read `affectedNodeCount` (uint8).
2. For each affected node, read its graph ID (uint16), then one modification record as described below.

Nodes which consume a walk selection but cannot be transformed are omitted from `affectedNodeCount`.

### `ABYN` path-dependent body (version 11)

The common header is followed by `nodeCount` (uint16) and `nodeCount` sorted graph IDs (uint16). Modification records then appear node-major and seed-minor. Use PoB's socket-to-character-start path to choose which node records apply.

After the node records is the ASCII marker `ASCS`, followed by `ascendancyCount` (uint16). Each ascendancy block contains:

1. UTF-8 name length (uint8) and name bytes.
2. For every seed, selected node count (uint8) followed by that many graph IDs (uint16).

### Modification records

Each modification begins with `componentCount` (uint8). Each component contains:

| Field | Type | Description |
| --- | --- | --- |
| componentType | uint8 | 1 = replacement, 2 = addition |
| localId | uint8 | Jewel-local change ID |
| statCount | uint8 | Number of following rolls |
| statRolls | int16[] | One signed value per stat |

`NodeIndexMapping.lua` contains `localIdToGlobalId[jewelType]`, which maps `localId` back to the `_rid`-based global IDs used by `AlternatePassiveAdditions.json` and `AlternatePassiveSkills.json`.

### Focused inspection and range export

The generator has non-interactive commands for checking observed game examples without generating every seed:

```text
DataFileGenerator --inspect-abyss <additions.json> <skills.json> <tree.json> <jewelType> <seed> <socketOrNodeId>
DataFileGenerator --inspect-abyss-ascendancy <additions.json> <skills.json> <tree.json> <seed> <ascendancyName>
DataFileGenerator --export-abyss-range <additions.json> <skills.json> <tree.json> <outputFile> <jewelType> <seedMin> <seedMax> [compressed]
```

For versions 7-10, `socketOrNodeId` is a jewel socket and inspection prints every affected node. For version 11 it is a single path or ascendancy node whose transformation should be inspected. Supplying `compressed` writes the range as the zlib stream conventionally given a `.zip` extension by PoB.
