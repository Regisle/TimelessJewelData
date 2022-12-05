using System;

namespace DatafileGenerator;

public static class GeneratorSettings
{
    public static readonly string ApplicationName = "DatafileGenerator";
    public static readonly Version ApplicationVersion = new Version(1, 2);

    public static string AlternatePassiveAdditionsFilePath;
    public static string AlternatePassiveSkillsFilePath;
    public static string PassiveSkillsFilePath;
}
