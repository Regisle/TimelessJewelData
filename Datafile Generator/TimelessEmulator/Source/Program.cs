using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using TimelessEmulator.Data;
using TimelessEmulator.Data.Models;
using TimelessEmulator.Game;

namespace TimelessEmulator;

public static class Program
{

    static Program()
    {

    }

    public static void Main(string[] arguments)
    {
        //most of this datafile generation is hacked together on top of a timeless jewel emulator
        //as such, forgive the scuff

        //all the graph ids of modifiable nodes
        int[] nodeArray = { 7092, 31153, 11016, 29061, 55993, 41251, 8198, 13164, 44347, 14665, 25367, 33082, 7082, 62042, 4336, 55166, 49415, 9976, 20018, 6113, 12407, 25682, 49571, 57953, 4940, 23038, 51559, 54667, 18990, 48282, 16754, 39761, 37639, 47427, 42917, 40645, 46127, 20966, 8500, 30547, 15163, 57266, 38023, 16703, 26712, 35288, 35260, 38989, 26481, 41190, 19103, 15046, 4177, 29106, 25332, 26270, 60472, 23027, 44202, 29353, 63282, 59928, 63723, 5233, 44922, 48813, 10851, 861, 60619, 20467, 5068, 18379, 39657, 24677, 61950, 27134, 44624, 31604, 25796, 26023, 864, 61388, 24858, 42900, 24157, 53757, 58831, 56355, 39023, 37501, 15678, 65159, 31520, 4300, 63635, 35910, 11190, 22472, 57167, 28076, 51108, 30302, 2219, 48822, 44429, 30380, 53118, 38508, 48109, 37326, 62429, 62363, 62017, 55676, 33287, 12247, 56359, 7285, 40766, 37999, 11730, 34173, 27575, 34171, 46636, 21929, 38836, 27137, 38906, 40508, 46897, 35958, 36949, 6712, 10031, 50570, 55854, 1572, 47421, 38538, 23760, 26564, 64210, 5916, 4713, 6245, 29049, 5935, 9392, 32932, 21974, 25831, 3644, 2957, 64221, 39678, 54629, 40776, 53018, 15973, 19679, 59070, 21389, 58218, 5743, 9386, 14057, 22285, 64395, 34327, 53945, 45486, 15716, 22728, 46672, 56094, 7444, 29199, 63422, 28311, 62108, 7374, 3533, 11088, 65108, 2550, 54396, 1550, 25168, 5430, 53987, 11792, 47426, 10594, 57992, 31257, 25531, 19884, 49772, 46842, 63398, 36452, 19919, 62767, 63138, 55743, 6967, 16756, 30251, 7440, 31931, 15064, 15631, 50422, 44967, 10221, 6446, 55649, 53793, 40653, 10575, 58402, 40291, 885, 37163, 24324, 59728, 62303, 43822, 2411, 35992, 5126, 14930, 48713, 7898, 35685, 32245, 50197, 34130, 14606, 10282, 39786, 35663, 64077, 60169, 2454, 38777, 6718, 31928, 61868, 10542, 31033, 37884, 32482, 59861, 65308, 64426, 62214, 1593, 20832, 41599, 55926, 1761, 36859, 50225, 24155, 15086, 55772, 30693, 33435, 44799, 58453, 21262, 12536, 54694, 57923, 46756, 17038, 38922, 56982, 53677, 24824, 9505, 42668, 43689, 54144, 57457, 13573, 31619, 39743, 61308, 40927, 15365, 46726, 38516, 31819, 6289, 48287, 5197, 64587, 14767, 65112, 14384, 25781, 13922, 19261, 64181, 49445, 224, 43833, 6764, 40366, 21048, 10555, 46471, 6359, 7335, 21413, 3167, 30160, 1252, 65456, 63963, 58763, 32455, 7153, 18901, 62319, 17735, 49254, 15599, 44529, 41472, 61471, 1203, 6237, 26866, 44908, 2913, 50862, 44606, 58449, 39768, 32739, 61198, 20551, 5152, 12795, 24383, 49412, 26523, 52502, 20987, 12246, 43061, 27323, 22577, 6770, 36915, 34880, 10904, 55563, 1609, 27119, 29797, 18009, 2092, 25175, 53791, 34400, 15438, 8426, 29034, 35436, 487, 5103, 37895, 14001, 36678, 42800, 22702, 53957, 24229, 4977, 27308, 54267, 23185, 43133, 9995, 12769, 34513, 63033, 41082, 33718, 20077, 39986, 42436, 7162, 9695, 42009, 7828, 40535, 30335, 32480, 15868, 36047, 63048, 50264, 4568, 420, 9294, 33923, 59005, 61217, 29005, 39631, 17251, 31758, 42837, 23456, 61050, 64024, 35053, 43514, 57839, 33740, 15405, 30439, 8879, 14419, 23881, 9511, 56029, 35556, 39916, 5462, 7364, 65273, 651, 9149, 43057, 3398, 11820, 12250, 38701, 18767, 48298, 14151, 27564, 6981, 36543, 35724, 17352, 33988, 13009, 5456, 19897, 43716, 34144, 52412, 4247, 17412, 23215, 54954, 4854, 14056, 34601, 45341, 6043, 50340, 60031, 5065, 22627, 11700, 50969, 20835, 59494, 16380, 2225, 63447, 39841, 20844, 62217, 56066, 54043, 30926, 63944, 34423, 48118, 25970, 65107, 60153, 16860, 18025, 46340, 31080, 29033, 51954, 29861, 63965, 20228, 61525, 50904, 47175, 31628, 22703, 24914, 10016, 1909, 8533, 11515, 35362, 56381, 49318, 23659, 367, 33864, 35706, 50029, 27301, 64816, 6884, 34959, 27140, 21330, 39648, 17821, 48556, 56671, 47949, 48971, 29629, 26620, 44362, 64401, 55558, 51748, 42760, 7938, 55332, 14021, 38664, 56460, 61636, 37647, 26294, 17833, 11568, 23199, 60887, 13273, 6741, 13498, 53292, 40841, 9194, 60398, 5591, 34478, 23690, 43374, 60648, 42981, 53802, 24256, 34906, 41026, 4270, 64355, 52655, 35035, 12379, 44183, 7555, 30733, 49178, 17569, 33779, 45366, 20142, 44562, 47251, 7388, 16775, 46910, 50858, 61602, 15451, 59306, 34207, 19609, 45803, 28878, 42637, 30030, 29543, 26096, 22062, 15081, 7688, 53558, 2355, 57900, 37800, 29547, 4378, 35507, 22356, 30691, 50515, 56370, 27605, 6139, 1340, 31875, 54657, 34157, 9432, 24865, 10490, 33479, 25714, 26960, 45680, 25237, 10835, 47312, 55866, 45317, 61262, 13714, 19939, 62021, 65034, 65167, 15073, 55485, 12702, 4397, 29292, 27718, 12809, 12888, 16954, 12783, 9261, 63976, 21958, 33755, 14936, 63027, 65400, 36200, 34973, 46111, 32901, 32314, 34591, 476, 44184, 38148, 12852, 4184, 44983, 34661, 44955, 58604, 30225, 1031, 1325, 29933, 31471, 24083, 18359, 21973, 34306, 24362, 29781, 60388, 38450, 5560, 51291, 26740, 38048, 55906, 37690, 48423, 6204, 13191, 7614, 1006, 38789, 61804, 14040, 18715, 31462, 10893, 13559, 8624, 11924, 60501, 34666, 43303, 30842, 36972, 43412, 57061, 22061, 5289, 34009, 34031, 19140, 7786, 55648, 46291, 544, 61039, 11431, 30825, 63150, 62662, 17674, 13782, 40867, 42911, 1696, 25732, 63933, 46578, 28330, 49109, 17566, 21435, 10073, 49147, 49416, 36542, 37569, 39211, 51856, 48513, 5612, 37403, 2474, 55804, 40075, 49534, 42485, 7918, 19782, 12236, 46730, 25324, 49538, 32431, 49588, 36121, 55647, 57362, 61264, 46469, 11645, 56716, 15117, 53279, 20528, 8302, 56158, 4367, 19635, 38900, 38805, 21075, 15711, 1957, 739, 18866, 53493, 27929, 59650, 6949, 19374, 55643, 2292, 48362, 27203, 27611, 40637, 9650, 60554, 32024, 25222, 11420, 44723, 1346, 16790, 21934, 36774, 17579, 33296, 11659, 11128, 8135, 57226, 57264, 54447, 18302, 25933, 48438, 50360, 28265, 54776, 39725, 50986, 47389, 35568, 59718, 50306, 56001, 16544, 60090, 14211, 42731, 22473, 14182, 13322, 3452, 37078, 22497, 8643, 63845, 15144, 55373, 2151, 48828, 62103, 58833, 47062, 7903, 20310, 45887, 36881, 33508, 35503, 43162, 39773, 23122, 19144, 57011, 65485, 32690, 64882, 23951, 57449, 16213, 58921, 18103, 37584, 49971, 4833, 23066, 11859, 25456, 3009, 24641, 22423, 4565, 32210, 21678, 62694, 45227, 24377, 56803, 11364, 43684, 59766, 38246, 60989, 63039, 35233, 51213, 36107, 50826, 18174, 43000, 12913, 10115, 49806, 63649, 17383, 58545, 14813, 52099, 14090, 4207, 22893, 24472, 53042, 16167, 10829, 35179, 60532, 9009, 49343, 36761, 1159, 43413, 3319, 12878, 44207, 17201, 48807, 41967, 17546, 21634, 40705, 36226, 40609, 12439, 22090, 5972, 36412, 53732, 21460, 8833, 57565, 2348, 44788, 60002, 52632, 36222, 47030, 30155, 34907, 28424, 45067, 3992, 49807, 37575, 918, 28221, 24643, 45456, 56153, 11688, 8544, 40126, 3359, 15400, 29185, 58803, 32059, 19501, 11551, 42795, 13753, 7503, 4432, 65203, 48514, 27163, 22315, 38772, 32053, 19730, 63067, 50472, 22972, 6580, 19287, 61875, 49379, 33545, 8948, 8930, 18402, 42041, 60204, 17814, 8348, 6534, 20812, 19506, 44103, 32091, 57736, 65097, 7594, 17849, 21602, 33911, 57615, 19711, 15167, 24872, 34506, 46092, 18033, 58541, 6633, 24772, 26456, 24721, 48823, 41047, 8566, 34510, 50041, 42720, 11651, 27659, 61982, 49047, 51517, 56807, 59016, 15021, 15437, 33725, 29937, 238, 11497, 30471, 34579, 40100, 51786, 28574, 13961, 62697, 26557, 17236, 7641, 62577, 37887, 17934, 29359, 63727, 29171, 30658, 31103, 25409, 9171, 39530, 36704, 54872, 1382, 9370, 44360, 6685, 29381, 48778, 51923, 5408, 12189, 5875, 9788, 42161, 26365, 12415, 7187, 36736, 31137, 35737, 47471, 20010, 37671, 34191, 60529, 33582, 46344, 29089, 36687, 930, 56855, 13231, 58198, 14209, 21170, 58603, 11784, 4105, 127, 32710, 38176, 49651, 41635, 27592, 44924, 55114, 49969, 55392, 53002, 4944, 55085, 18703, 55247, 63799, 61320, 18865, 56589, 15085, 54268, 56231, 2392, 13807, 33089, 49547, 6363, 44683, 45035, 39821, 50459, 27709, 28887, 38849, 22647, 64612, 32942, 64239, 22407, 63207, 19098, 19388, 23852, 32519, 26188, 52090, 27415, 38129, 36874, 51219, 13232, 9567, 5296, 33310, 43787, 29379, 52230, 37175, 36371, 11200, 25439, 16970, 63139, 23471, 52423, 40287, 570, 15027, 4036, 15228, 47306, 32345, 444, 52904, 2094, 21301, 465, 55307, 64241, 26471, 15837, 40743, 51801, 14745, 53573, 41476, 16243, 52031, 52848, 27879, 39521, 40840, 49779, 29552, 48878, 51524, 51146, 57746, 52095, 57044, 62490, 10511, 45838, 8640, 45272, 45033, 53324, 15344, 24496, 39861, 42006, 18661, 44102, 10153, 40135, 9262, 65093, 59370, 63795, 54142, 64265, 6799, 63194, 63639, 64501, 46136, 58649, 62069, 31583, 1600, 38344, 65033, 10017, 41250, 41536, 62712, 6542, 60803, 94, 29870, 56149, 720, 11334, 15549, 8620, 27325, 41595, 48477, 23507, 57248, 41689, 27276, 62831, 51220, 58968, 37663, 44988, 39524, 23439, 9015, 34625, 46106, 12068, 16851, 47743, 3469, 52522, 11716, 42744, 45608, 34678, 26528, 58271, 19069, 49605, 50382, 6616, 48093, 44824, 16743, 34560, 52157, 31371, 40751, 6233, 57199, 61327, 51404, 5237, 50338, 6538, 38662, 61306, 17421, 29104, 51440, 9355, 49978, 4011, 65210, 25178, 57240, 9373, 32117, 48099, 40132, 58854, 1891, 59009, 22618, 21033, 20546, 35894, 5823, 32227, 64509, 3863, 20167, 36877, 7136, 22488, 17788, 42133, 57259, 50690, 62094, 3634, 41819, 21835, 9877, 23438, 16602, 41137, 14157, 28758, 39443, 56276, 6615, 5022, 44339, 63439, 43608, 63921, 20852, 6785, 36585, 42649, 64709, 25058, 41380, 57819, 6654, 6913, 42964, 65224, 55571, 12143, 28859, 4656, 22217, 6383, 59866, 8938, 2715, 3187, 903, 30894, 18670, 15842, 53574, 26002, 7659, 25738, 25766, 32802, 16236, 30679, 5622, 25209, 30626, 6, 62795, 56090, 49408, 4219, 48698, 45593, 12801, 53114, 25067, 31315, 33783, 4502, 31973, 14674, 6250, 30767, 22535, 46965, 28754, 35283, 10763, 21575, 40644, 42686, 60949, 7920, 46289, 30338, 51212, 58069, 42623, 30205, 54338, 33903, 60737, 9769, 49515, 24067, 38520, 47321, 265, 56648, 60440, 5018, 45810, 13168, 11984, 58032, 45788, 2121, 28503, 12794, 30455, 28658, 14804, 41119, 11678, 44306, 8458, 52288, 31513, 52714, 60180, 1201, 45491, 7085, 41866, 60942, 36801, 19228, 3362, 529, 3656, 58244, 59252, 59606, 13885, 28012, 39718, 27283, 5616, 22266, 58210, 19008, 11689, 62970, 46904, 31508, 37504, 9786, 4573, 33558, 3309, 41989, 51420, 49481, 65053, 24050, 46408, 18769, 61653, 38190, 1822, 46896, 4973, 5129, 9864, 59482, 27962, 31501, 59556, 39814, 27190, 25770, 47362, 1767, 19210, 43316, 47065, 54868, 39938, 45360, 57030, 9535, 56814, 46694, 43457, 11850, 17608, 4481, 56646, 25511, 63843, 7112, 16882, 43491, 14400, 41420, 33196, 13202, 17171, 43385, 58474, 34763, 30110, 42907, 53013, 50150, 45436, 64878, 30969, 32477, 36281, 32555, 36858, 61981, 60405, 25757, 47507, 49929, 25789, 54452, 43328, 59699, 15852, 14292, 38348, 16079, 21758, 48614, 37619, 64235, 24133, 19858, 36221, 9206, 23616, 32176, 60259, 11018, 38995, 8012, 8001, 63228, 49308, 42804, 53213, 3314, 27656, 42443, 25411, 3537, 40362, 25260, 27788, 21693, 5613, 38539, 54713, 64769, 23449, 36849, 23886, 7263, 29454, 62744, 18770, 63251, 1461, 53456, 2959, 56186, 11162, 5802, 3424, 1405, 9469, 59220, 65502, 57080, 42104, 49621, 31585, 3676, 17790, 18707, 1427, 51235, 35851, 60592, 47484, 18182, 56295, 46277, 49900, 36287, 20807, 12412, 42632, 27444, 4750, 58288, 63618, 15290, 40409, 4546, 9055, 33777, 1655, 8027, 2260, 35334, 29549, 61689, 23334, 6797, 55414, 22261, 56174, 38864, 47422, 44134, 11489, 8920, 36490, 36225, 30745, 49568, 17908, 25775, 5629, 35384, 15614, 54791, 11811, 20127, 1648, 31359, 37785, 12824, 45827, 28753, 55380, 30319, 54574, 59605, 54974, 32432, 21297, 39665, 38149, 32514, 55750, 2185, 23912, 12948, 51881, 49459, 59151, 12720, 29856, 1698, 1568, 54354, 6108, 56509, 52213, 45329, 7069, 10843, 33374, 31222, 38947, 21228, 51953, 7609 };
        //all the modifiable notables
        string[] Notables_List = { "Acrimony", "Acuity", "Adamant", "Adder's Touch", "Adjacent Animosity", "Admonisher", "Aggressive Bastion", "Agility", "Alacrity", "Ambidexterity", "Amplify", "Ancestral Knowledge", "Annihilation", "Anointed Flesh", "Arcane Capacitor", "Arcane Chemistry", "Arcane Expanse", "Arcane Focus", "Arcane Guarding", "Arcane Potency", "Arcane Sanctuary", "Arcane Will", "Arcanist's Dominion", "Arcing Blows", "Arsonist", "Art of the Gladiator", "Ash, Frost and Storm", "Aspect of the Eagle", "Aspect of the Lynx", "Assassination", "Assured Strike", "Asylum", "Atrophy", "Avatar of the Hunt", "Backstabbing", "Ballistics", "Bannerman", "Barbarism", "Bastion Breaker", "Battle Rouse", "Beef", "Berserking", "Blacksmith's Clout", "Blade Barrier", "Blade Master", "Blade of Cunning", "Bladedancer", "Blast Radius", "Blast Waves", "Blood Drinker", "Blood Siphon", "Bloodless", "Bloodletting", "Blunt Trauma", "Bone Breaker", "Born to Fight", "Brand Equity", "Bravery", "Breath of Flames", "Breath of Lightning", "Breath of Rime", "Brinkmanship", "Brutal Blade", "Burning Brutality", "Butchery", "Cannibalistic Rite", "Careful Conservationist", "Carrion", "Champion of the Cause", "Charisma", "Claws of the Falcon", "Claws of the Hawk", "Claws of the Magpie", "Cleaving", "Clever Construction", "Clever Thief", "Cloth and Chain", "Coldhearted Calculation", "Combat Stamina", "Command of Steel", "Constitution", "Coordination", "Corruption", "Counterweight", "Courage", "Crackling Speed", "Cruel Preparation", "Crystal Skin", "Dark Arts", "Dazzling Strikes", "Deadly Draw", "Death Attunement", "Decay Ward", "Deep Breaths", "Deep Thoughts", "Deep Wisdom", "Defiance", "Deflection", "Depth Perception", "Dervish", "Destroyer", "Destructive Apparatus", "Devastating Devices", "Devotion", "Diamond Skin", "Dire Torment", "Dirty Techniques", "Disciple of the Forbidden", "Disciple of the Slaughter", "Disciple of the Unyielding", "Discipline and Training", "Discord Artisan", "Disemboweling", "Disintegration", "Dismembering", "Divine Fervour", "Divine Fury", "Divine Judgement", "Divine Wrath", "Doom Cast", "Dreamer", "Druidic Rite", "Dynamo", "Eagle Eye", "Efficient Explosives", "Elder Power", "Elemental Focus", "Endurance", "Enduring Bond", "Enigmatic Defence", "Enigmatic Reach", "Entrench", "Entropy", "Essence Extraction", "Essence Infusion", "Essence Sap", "Essence Surge", "Ethereal Feast", "Exceptional Performance", "Executioner", "Expeditious Munitions", "Expertise", "Explosive Elements", "Explosive Impact", "Explosive Runes", "Faith and Steel", "Fangs of Frost", "Fangs of the Viper", "Farsight", "Fatal Blade", "Fatal Toxins", "Fearsome Force", "Feller of Foes", "Fending", "Fervour", "Field Medicine", "Finesse", "Fingers of Frost", "Fire Walker", "Flash Freeze", "Flaying", "Fleetfoot", "Force Shaper", "Forceful Skewering", "Forces of Nature", "Foresight", "Forethought", "Freedom of Movement", "Frenetic", "From the Shadows", "Frost Walker", "Fury Bolts", "Fusillade", "Galvanic Hammer", "Gladiator's Perseverance", "Golem Commander", "Golem's Blood", "Goliath", "Graceful Assault", "Grave Intentions", "Gravepact", "Growth and Decay", "Hard Knocks", "Harpooner", "Harrier", "Harvester of Foes", "Hasty Reconstruction", "Hatchet Master", "Heart and Soul", "Heart of Darkness", "Heart of Flame", "Heart of Ice", "Heart of Oak", "Heart of the Warrior", "Heart of Thunder", "Heartseeker", "Hearty", "Heavy Draw", "Hematophagy", "Herbalism", "Hex Master", "High Explosives", "Hired Killer", "Holy Dominion", "Holy Fire", "Hunter's Gambit", "Indomitable Army", "Inexorable", "Influence", "Infused", "Infused Flesh", "Insightfulness", "Inspiring Bond", "Instability", "Instinct", "Intuition", "Inveterate", "Ironwood", "Juggernaut", "Kinetic Impacts", "King of the Hill", "Lava Lash", "Leadership", "Lethality", "Life Raker", "Light Eater", "Light of Divinity", "Lightning Walker", "Longshot", "Lord of the Dead", "Lust for Carnage", "Magmatic Strikes", "Malicious Intent", "Mana Flows", "Mark the Prey", "Marked for Death", "Martial Experience", "Master Fletcher", "Master of Blades", "Master of the Arena", "Master Sapper", "Measured Fury", "Melding", "Mental Rapidity", "Merciless Skewering", "Might", "Mind Drinker", "Mystic Bulwark", "Mysticism", "Natural Authority", "Natural Remedies", "Nightstalker", "Nimbleness", "One with Evil", "One With Nature", "One with the River", "Overcharge", "Overcharged", "Pain Forger", "Panopticon", "Path of the Hunter", "Path of the Savant", "Path of the Warrior", "Physique", "Piercing Shots", "Poisonous Fangs", "Potency of Will", "Powerful Bond", "Practical Application", "Precision", "Presage", "Primal Manifestation", "Primal Spirit", "Primeval Force", "Prism Weave", "Prismatic Skin", "Prodigal Perfection", "Profane Chemistry", "Proficiency", "Prowess", "Purity of Flesh", "Quick Recovery", "Quickstep", "Rampart", "Ravenous Horde", "Razor's Edge", "Redemption", "Reflexes", "Relentless", "Replenishing Remedies", "Resourcefulness", "Retaliation", "Retribution", "Revelry", "Revenge of the Hunted", "Ribcage Crusher", "Righteous Army", "Righteous Decree", "Robust", "Runesmith", "Saboteur", "Sacrifice", "Safeguard", "Sanctity", "Sanctuary", "Sanctum of Thought", "Savage Wounds", "Savagery", "Searing Heat", "Season of Ice", "Sentinel", "Serpent Stance", "Serpentine Spellslinger", "Shaman's Dominion", "Shamanistic Fury", "Shaper", "Silent Steps", "Skittering Runes", "Skull Cracking", "Slaughter", "Sleight of Hand", "Smashing Strikes", "Snowforged", "Soul of Steel", "Soul Siphon", "Soul Thief", "Sovereignty", "Spiked Bulwark", "Spinecruncher", "Spirit Void", "Spiritual Aid", "Spiritual Command", "Split Shot", "Stamina", "Static Blows", "Steadfast", "Steelwood Stance", "Storm Weaver", "Strong Arm", "Successive Detonations", "Surveillance", "Survivalist", "Swagger", "Swift Venoms", "Taste for Blood", "Tempest Blast", "Testudo", "Thick Skin", "Thief's Craft", "Thrill Killer", "Throatseeker", "Tireless", "Titanic Impacts", "Tolerance", "Totemic Zeal", "Toxic Strikes", "Tribal Fury", "Trick Shot", "Trickery", "True Strike", "Twin Terrors", "Undertaker", "Unfaltering", "Unnatural Calm", "Unstable Munitions", "Utmost Intellect", "Utmost Might", "Utmost Swiftness", "Vampirism", "Vanquisher", "Versatility", "Veteran Soldier", "Vigour", "Vitality Void", "Void Barrier", "Volatile Mines", "Wandslinger", "Warrior Training", "Warrior's Blood", "Wasting", "Watchtowers", "Weapon Artistry", "Weathered Hunter", "Whirling Barrier", "Whispers of Doom", "Will of Blades", "Window of Opportunity", "Winter Spirit", "Wisdom of the Glade", "Wrecking Ball", "Written in Blood" };
        
        //has the notable indices file been generated yet
        bool indexed = false;
        for (int i = 1; i <= 5; i++)
        {
            string output_file = string.Empty;
            int jewel_type_in = i;
            int jewel_min = 0;
            int jewel_max = 0;
            int jewel_increment = 1;
            switch (jewel_type_in)
            {
                case 1:
                    jewel_min = 100;
                    jewel_max = 8000;
                    output_file = @"Glorious Vanity";
                    break;
                case 2:
                    jewel_min = 10000;
                    jewel_max = 18000;
                    output_file = @"Lethal Pride";
                    break;
                case 3:
                    jewel_min = 500;
                    jewel_max = 8000;
                    output_file = @"Brutal Restraint";
                    break;
                case 4:
                    jewel_min = 2000;
                    jewel_max = 10000;
                    output_file = @"Militant Faith";
                    break;
                case 5:
                    jewel_min = 2000;
                    jewel_max = 160000;
                    jewel_increment = 20;
                    output_file = @"Elegant Hubris";
                    break;
                default:
                    return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(output_file))))
                Directory.CreateDirectory(Path.GetDirectoryName(output_file));
            if (File.Exists(output_file))
                File.Delete(output_file);

            if (!DataManager.Initialize())
                Program.ExitWithError("Failed to initialize the [yellow]data manager[/].");

            //force the enumeration with ToList now
            var sortedNotables = Notables_List.Select(DataManager.GetPassiveSkillByFuzzyValue).ToList();
            //this probably should be sorted by graphid, but we hadnt decided on something at the time and its arbitrary anyway so :shrug:
            //feel free to change it. should be fine since itll also change the CSV file used to translate
            //if we havent build the index csv yet...
            sortedNotables.Sort((x, y) => x.Index.CompareTo(y.Index));
            var placeholder = new List<Tuple<int, PassiveSkill>>();
            for (int k = 0; k < sortedNotables.Count; k++)
            {
                placeholder.Add(new Tuple<int, PassiveSkill>(k, sortedNotables[k]));
            }
            var notableDict = placeholder.ToDictionary(x => x.Item2.Index, x => x.Item1);

            Stopwatch sw = Stopwatch.StartNew();
            //glorious vanity logic
            if (i == 1)
            {
                //build the csv
                if (File.Exists("glorious vanity indices.csv"))
                    File.Delete("glorious vanity indices.csv");
                var sortedNodes = nodeArray.ToList();
                sortedNodes.Sort();
                StringBuilder sb = new StringBuilder("PassiveSkillGraphId,Name,Datafile Parsing Index\n");
                for (int k = 0; k < sortedNodes.Count; k++)
                {
                    var node = DataManager.GetPassiveSkillById(sortedNodes[k]);
                    sb.AppendLine(node.GraphIdentifier + "," + (node.Name.Contains(',') ? ("\"" + node.Name + "\"") : node.Name) + "," + k);
                }
                File.WriteAllText("glorious vanity indices.csv", sb.ToString());
                //build the datafile header
                int maxSeed = (jewel_max - jewel_min) / jewel_increment + 1;
                byte[] header = new byte[maxSeed * nodeArray.Length];
                byte[][] data = new byte[maxSeed * nodeArray.Length][];
                //nested parallell tasks, in case your cpu wasnt on fire yet
                Parallel.ForEach(nodeArray, nodeId =>
                {
                    var node = DataManager.GetPassiveSkillById(nodeId);
                    Parallel.For(jewel_min / jewel_increment, jewel_max / jewel_increment + 1, i =>
                    {
                        //which jewel seed is this
                        i *= jewel_increment;
                        int jewel_seed = i;
                        int jewel_index = (jewel_seed - jewel_min) / jewel_increment;
                        int jewel_type = jewel_type_in;
                        //modify the tree using that jewel
                        TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewel_seed, (uint)jewel_type);
                        if (timelessJewelFromInput == null)
                            Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                        //determine how the particular node was modified
                        AlternateTreeManager alternateTreeManager = new AlternateTreeManager(node, timelessJewelFromInput);
                        //GV will always replace nodes
                        List<byte> indices = new List<byte>();
                        List<byte> rolls = new List<byte>();
                        var skillInfo = alternateTreeManager.ReplacePassiveSkill();
                        //handle might/legacy of the vaal
                        if (skillInfo.AlternatePassiveSkill.Index == 76 || skillInfo.AlternatePassiveSkill.Index == 77)
                        {
                            //do we want to add the indicator for this being Legacy of the Vaal/Might of the Vaal or just shit out the stats?
                            //indices.Add((byte)(skillInfo.AlternatePassiveSkill.Index + 93 + 1));
                            for (int k = 0; k < skillInfo.AlternatePassiveAdditionInformations.Count; k++)
                            {
                                //add the additions
                                indices.Add((byte)skillInfo.AlternatePassiveAdditionInformations.ElementAt(k).AlternatePassiveAddition.Index);
                                rolls.Add((byte)skillInfo.AlternatePassiveAdditionInformations.ElementAt(k).StatRolls[0U]);
                            }
                        }
                        //handle all others
                        else
                        {
                            indices.Add((byte)(skillInfo.AlternatePassiveSkill.Index + 93 + 1));
                            for (int k = 0; k < skillInfo.StatRolls.Count; k++)
                            {
                                rolls.Add((byte)skillInfo.StatRolls[(uint)k]);
                            }
                        }
                        //save the data
                        var dataEntry = new List<byte>(indices);
                        dataEntry.AddRange(rolls);
                        header[sortedNodes.IndexOf(nodeId) * maxSeed + jewel_index] = (byte)dataEntry.Count;
                        data[sortedNodes.IndexOf(nodeId) * maxSeed + jewel_index] = dataEntry.ToArray();
                    });
                });
                //write the data
                using (Stream file = File.OpenWrite(output_file))
                {
                    file.Write(header, 0, header.Length);
                    for (int k = 0; k < data.Length; k++)
                    {
                        file.Write(data[k], 0, data[k].Length);
                    }
                }
            }
            //non-glorious vanity logic
            else
            {
                if (!indexed)
                {
                    //make the csv
                    if (File.Exists("node indices.csv"))
                        File.Delete("node indices.csv");

                    StringBuilder sb = new StringBuilder("PassiveSkillGraphId,Name,Datafile Parsing Index\n");
                    for (int k = 0; k < sortedNotables.Count; k++)
                    {
                        sb.AppendLine(sortedNotables[k].GraphIdentifier + "," + (sortedNotables[k].Name.Contains(',') ? ("\"" + sortedNotables[k].Name + "\"") : sortedNotables[k].Name) + "," + k);
                    }
                    File.WriteAllText("node indices.csv", sb.ToString());
                    indexed = true;
                }
                int maxSeed = (jewel_max - jewel_min) / jewel_increment + 1;
                byte[] data = new byte[maxSeed * Notables_List.Length];
                //nested parallell tasks, in case your cpu wasnt on fire yet
                Parallel.For(jewel_min / jewel_increment, jewel_max / jewel_increment + 1, i =>
                {
                    //what jewel seed is this
                    i *= jewel_increment;
                    int jewel_seed = i;
                    int jewel_index = (jewel_seed - jewel_min) / jewel_increment;
                    int jewel_type = jewel_type_in;
                    //apply it to the tree
                    TimelessJewel timelessJewelFromInput = GetTimelessJewel((uint)jewel_seed, (uint)jewel_type);
                    if (timelessJewelFromInput == null)
                        Program.ExitWithError("Failed to get the [yellow]timeless jewel[/] from input.");
                    Parallel.ForEach(sortedNotables, Notable_In =>
                    {
                        //figure out how it affects this specific notable
                        AlternateTreeManager alternateTreeManager = new AlternateTreeManager(Notable_In, timelessJewelFromInput);
                        bool flag = alternateTreeManager.IsPassiveSkillReplaced();
                        byte passiveSkillIndex = 0;
                        if (flag)
                        {
                            //replacements get stat rid + 94
                            passiveSkillIndex = (byte)(alternateTreeManager.ReplacePassiveSkill().AlternatePassiveSkill.Index + 93 + 1);
                        }
                        else
                        {
                            //additions get stat rid
                            passiveSkillIndex = (byte)alternateTreeManager.AugmentPassiveSkill().First().AlternatePassiveAddition.Index;
                            //previously templar jewels were weirdly hardcoded to say "no stat" here which is where the stat index 249 case came from
                            //readd a check if that 249 thing is desirable
                        }
                        data[notableDict[Notable_In.Index] * maxSeed + jewel_index] = passiveSkillIndex;
                    });
                });
                //write to the file
                using (Stream file = File.OpenWrite(output_file))
                {
                    file.Write(data, 0, data.Length);
                }
            }
            sw.Stop();
            Console.WriteLine($"{output_file} took {sw.Elapsed.TotalSeconds} seconds");
        }
        Console.WriteLine($"All files done. Find them at {Settings.BaseDirectoryPath}");
    }

    private static TimelessJewel GetTimelessJewel(uint seed, uint jewelType)
    {
        Dictionary<uint, Dictionary<string, TimelessJewelConqueror>> timelessJewelConquerors = new Dictionary<uint, Dictionary<string, TimelessJewelConqueror>>()
        {
            {
                1, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Xibaqua", new TimelessJewelConqueror(1, 0) },
                    { "[springgreen3]Zerphi (Legacy)[/]", new TimelessJewelConqueror(2, 0) },
                    { "Ahuana", new TimelessJewelConqueror(2, 1) },
                    { "Doryani", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                2, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Kaom", new TimelessJewelConqueror(1, 0) },
                    { "Rakiata", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Kiloava (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Akoya", new TimelessJewelConqueror(3, 1) }
                }
            },
            {
                3, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Deshret (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Balbala", new TimelessJewelConqueror(1, 1) },
                    { "Asenath", new TimelessJewelConqueror(2, 0) },
                    { "Nasima", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                4, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "[springgreen3]Venarius (Legacy)[/]", new TimelessJewelConqueror(1, 0) },
                    { "Maxarius", new TimelessJewelConqueror(1, 1) },
                    { "Dominus", new TimelessJewelConqueror(2, 0) },
                    { "Avarius", new TimelessJewelConqueror(3, 0) }
                }
            },
            {
                5, new Dictionary<string, TimelessJewelConqueror>()
                {
                    { "Cadiro", new TimelessJewelConqueror(1, 0) },
                    { "Victario", new TimelessJewelConqueror(2, 0) },
                    { "[springgreen3]Chitus (Legacy)[/]", new TimelessJewelConqueror(3, 0) },
                    { "Caspiro", new TimelessJewelConqueror(3, 1) }
                }
            }
        };
        TimelessJewelConqueror timelessJewelConqueror = timelessJewelConquerors[jewelType]
            .First()
            .Value;
        AlternateTreeVersion alternateTreeVersion = DataManager.AlternateTreeVersions
            .First(q => (q.Index == jewelType));
        return new TimelessJewel(alternateTreeVersion, timelessJewelConqueror, (uint)seed);
    }

    private static void WaitForExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Press [yellow]any key[/] to exit.");

        try
        {
            Console.ReadKey();
        }
        catch { }

        Environment.Exit(0);
    }

    private static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red]Error[/]: {error}");
    }

    private static void ExitWithError(string error)
    {
        PrintError(error);
        WaitForExit();
    }

}
