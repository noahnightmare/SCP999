using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Loader;
using PlayerRoles;
using SCP999.Configs;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using Utf8Json.Internal.DoubleConversion;
using YamlDotNet.Serialization;

namespace SCP999
{
    public class Config : IConfig
    {
        // Basic format for CustomRoles - creates a directory if not made, and hides role configs that are in another directory
        [YamlIgnore]
        public Roles RoleConfigs { get; private set; } = null!;

        [Description("Whether or not debug messages should be shown.")]
        public bool Debug { get; set; } = true;

        [Description("Whether or not this plugin is enabled.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Directory for all configs related to SCP-999")]
        public string ConfigsFolder { get; set; } = Path.Combine(Paths.Configs, "SCP-999");

        [Description("Directory for all configs related to SCP-999")]
        public string ConfigsFile { get; set; } = "config.yml";

        public void LoadConfigs()
        {
            if (!Directory.Exists(ConfigsFolder)) Directory.CreateDirectory(ConfigsFolder);

            string filePath = Path.Combine(ConfigsFolder, ConfigsFile);

            if (!File.Exists(filePath))
            {
                RoleConfigs = new Roles();
                File.WriteAllText(filePath, Loader.Serializer.Serialize(RoleConfigs));
                Log.Warn("SCP 999 Config folder does not exist! Creating...");
            }
            else
            {
                RoleConfigs = Loader.Deserializer.Deserialize<Roles>(File.ReadAllText(filePath));
                File.WriteAllText(filePath, Loader.Serializer.Serialize(RoleConfigs));
            }
        }
    }
}