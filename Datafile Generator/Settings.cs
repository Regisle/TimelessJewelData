// Decompiled with JetBrains decompiler
// Type: TimelessEmulator.Settings
// Assembly: TimelessEmulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F88AF474-1AC6-4934-AFAD-2E743166E28C
// Assembly location: F:\Downloads\TimelessEmulator\TimelessEmulator.dll

using System;
using System.IO;

namespace DatafileGenerator
{
  public static class Settings
  {
    private const string DATA_DIRECTORY_NAME = "data";
    private const string ALTERNATE_PASSIVE_ADDITIONS_FILE_NAME = "alternate_passive_additions.json";
    private const string ALTERNATE_PASSIVE_SKILLS_FILE_NAME = "alternate_passive_skills.json";
    private const string ALTERNATE_TREE_VERSIONS_FILE_NAME = "alternate_tree_versions.json";
    private const string PASSIVE_SKILLS_FILE_NAME = "passive_skills.json";
    public static readonly string BaseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string DataDirectoryPath = Path.Combine(Settings.BaseDirectoryPath, "data");
    public static readonly string AlternatePassiveAdditionsFilePath = Path.Combine(Settings.DataDirectoryPath, "alternate_passive_additions.json");
    public static readonly string AlternatePassiveSkillsFilePath = Path.Combine(Settings.DataDirectoryPath, "alternate_passive_skills.json");
    public static readonly string AlternateTreeVersionsFilePath = Path.Combine(Settings.DataDirectoryPath, "alternate_tree_versions.json");
    public static readonly string PassiveSkillsFilePath = Path.Combine(Settings.DataDirectoryPath, "passive_skills.json");
  }
}
