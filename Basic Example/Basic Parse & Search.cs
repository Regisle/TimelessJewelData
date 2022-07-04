//notables are a dictionary of notable index and weight
//stats are a dictionary of stat index and weight
//definitions for both can be found at https://github.com/Regisle/TimelessJewelData
//if Elegant Hubris & Militant Faith aren't working, try swapping their file names
//there was a mixup with jewel type indices earlier that had them named backward the first time they were uploaded
//and you might have the outdated names still
private static IEnumerable<Tuple<int, double>> ExecuteNonGVSearch(Dictionary<int, double> notables, Dictionary<int, double> stats, double weight, int jewelType)
{
    //DisplayInputs(notables, stats, weight, jewelType); pretty printed the inputs
    int maxSeedIndex = 0;
    int minSeed = 0;
    int maxSeed = 0;
    int seedIncrement = 1;
    switch (jewelType)
    {
        case 1:
            //Console.WriteLine("skipping GV");
            //minSeed = 100;
            //maxSeed = 8000;
            return new List<Tuple<int, double>>();
        case 2:
            minSeed = 10000;
            maxSeed = 18000;
            break;
        case 3:
            minSeed = 500;
            maxSeed = 8000;
            break;
        case 4:
            minSeed = 2000;
            maxSeed = 10000;
            break;
        case 5:
            minSeed = 2000;
            maxSeed = 160000;
            seedIncrement = 20;
            break;
        default:
            break;
    }
    maxSeedIndex = (maxSeed - minSeed) / seedIncrement + 1;
    FileStream? stream = GetStreamForJewel(jewelType);
    if (stream == null)
    {
        Console.WriteLine("failed to open data file");
        return new List<Tuple<int, double>>();
    }
    //each thread accesses a unique index, so no need for a concurrent bag
    var jewels = new List<Tuple<int, int, byte>>[maxSeedIndex];
    Parallel.For(0, jewels.Length, i =>
    {
        jewels[i] = new List<Tuple<int, int, byte>>();
    });
    Console.WriteLine("reading data file...");
    stream.Seek(0, SeekOrigin.End);
    var fileSize = stream.Position;
    stream.Seek(0, SeekOrigin.Begin);
    byte[] data = new byte[fileSize];
    stream.Read(data, 0, data.Length);
    stream.Close();
    foreach (var notable in notables.Keys)
    {
        int notableOffset = notable * maxSeedIndex;
        Parallel.For(0, maxSeedIndex, i =>
        {
            jewels[i].Add(new Tuple<int, int, byte>(i * seedIncrement + minSeed, notable, data[notableOffset + i]));
        });
    }
    Console.WriteLine("performing filter...");
    var searchResult = jewels.Select(jewelData => new Tuple<int, double>(jewelData.First().Item1, jewelData.Sum(x => stats.ContainsKey(x.Item3) ? notables[x.Item2] * stats[x.Item3] : 0))).Where(x => x.Item2 >= weight).ToList();
    searchResult.Sort((y, x) => x.Item2 == y.Item2 ? x.Item1.CompareTo(y.Item1) : y.Item2.CompareTo(x.Item2));
    Console.WriteLine("jewel seeds & weight:\n" + string.Join("\n", searchResult.Select(x => $"(seed: {string.Format("{0,6}", x.Item1)}\tweight: {x.Item2})")));
    return searchResult;
}



private static IEnumerable<Tuple<int, double>> ExecuteGVSearch(Dictionary<int, double> notables, Dictionary<int, double> stats, double weight)
{
    //DisplayInputs(notables, stats, weight, 1); pretty printed the inputs
    int nodeCount = 1678;
    int minSeed = 100;
    int maxSeed = 8000;
    int seedIncrement = 1;
    int maxSeedIndex = (maxSeed - minSeed) / seedIncrement + 1;
    FileStream? stream = GetStreamForJewel(1);
    if (stream == null)
    {
        Console.WriteLine("failed to open data file");
        return new List<Tuple<int, double>>();
    }
    //each thread accesses a unique index, so no need for a concurrent bag
    var jewels = new List<Tuple<int, int, byte, byte>>[maxSeedIndex];
    Parallel.For(0, jewels.Length, i =>
    {
        jewels[i] = new List<Tuple<int, int, byte, byte>>();
    });
    Console.WriteLine("reading data file...");
    //read the header
    byte[] header = new byte[nodeCount * maxSeedIndex];
    //long[] offsets = new long[nodeCount * maxSeedIndex];
    byte[][] data = new byte[nodeCount * maxSeedIndex][];
    stream.Read(header, 0, header.Length);
    //offsets[0] = header.Length;
    data[0] = new byte[header[0]];
    stream.Read(data[0], 0, data[0].Length);
    for (int i = 1; i < header.Length; i++)
    {
        //offsets[i] = offsets[i - 1] + header[i - 1];
        data[i] = new byte[header[i]];
        stream.Read(data[i], 0, data[i].Length);
    }
    stream.Close();
    foreach (var notable in notables.Keys)
    {
        int notableOffset = notable * maxSeedIndex;
        Parallel.For(0, maxSeedIndex, i =>
        {
		    //currently ignores the second stat roll on a 2 stat notable replacement
			//if you wanted to extract it, check if Length == 3 and add the ability to process the second stat's weight separately from the first's
            int halfway = data[notableOffset + i].Length / 2;
            for (int k = 0; k < halfway; k++)
            {
                jewels[i].Add(new Tuple<int, int, byte, byte>(i * seedIncrement + minSeed, notable, data[notableOffset + i][k], data[notableOffset + i][k + halfway]));
            }
        });
    }
    Console.WriteLine("performing filter...");
    var searchResult = jewels.Select(jewelData => new Tuple<int, double>(jewelData.First().Item1, jewelData.Sum(x => stats.ContainsKey(x.Item3) ? notables[x.Item2] * stats[x.Item3] * x.Item4 : 0))).Where(x => x.Item2 >= weight).ToList();
    searchResult.Sort((y, x) => x.Item2 == y.Item2 ? x.Item1.CompareTo(y.Item1) : y.Item2.CompareTo(x.Item2));
    Console.WriteLine("jewel seeds & weight:\n" + string.Join("\n", searchResult.Select(x => $"(seed: {string.Format("{0,6}", x.Item1)}\tweight: {x.Item2})")));
    return new List<Tuple<int, double>>();
}




private static FileStream? GetStreamForJewel(int jewelType)
{
    string fileName;
    switch (jewelType)
    {
        case 1:
            fileName = @"Glorious Vanity";
            break;
        case 2:
            fileName = @"Lethal Pride";
            break;
        case 3:
            fileName = @"Brutal Restraint";
            break;
        case 4:
            fileName = @"Militant Faith";
            break;
        case 5:
            fileName = @"Elegant Hubris";
            break;
        default:
            return null;
    }
    try
    {
        return File.OpenRead(fileName);
    }
    catch
    {
        return null;
    }
}