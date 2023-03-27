using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SignPadPicker
{
    public class SignPadLoader
    {
        public static List<ISignPadPlugin> Plugins { get; set; }

        public static ISignPadPlugin GetPlugin(string name)
            => Plugins
            .FirstOrDefault(p => p.Name == name)
            ?? throw new Exception($"No plugin found with name '{name}'");

        public static ISignPadPlugin GetAnyPlugin(IEnumerable<string> names = null)
            => (names.Select(name => GetPlugin(name)) ?? Plugins)
            .FirstOrDefault(p => CheckPluginIsAvailable(p))
            ?? throw new Exception($"No plugin found.");

        public void LoadPlugins(string path)
        {
            Plugins = new List<ISignPadPlugin>();

            LoadPluginAssemblyFile(path);

            LoadPluginInstance();
        }

        private void LoadPluginAssemblyFile(string path) =>
            Directory.GetFiles(path).ToList()
                .Where(file =>
                {
                    string fileName = Path.GetFileName(file);
                    return fileName.StartsWith("SignPadPicker.") && fileName.EndsWith("Adaptor.dll");
                })
                .ToList()
                .ForEach(file => Assembly.LoadFile(Path.GetFullPath(file)));

        private void LoadPluginInstance() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => typeof(ISignPadPlugin).IsAssignableFrom(p) && p.IsClass)
                .ToList()
                .ForEach(type => Plugins.Add((ISignPadPlugin)Activator.CreateInstance(type)));

        private static bool CheckPluginIsAvailable(ISignPadPlugin plugin)
        {
            try { return plugin.IsAvailable; }
            catch { return false; }
        }
    }
}
