using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SignPadPicker
{
    public class SignPadLoader
    {
        public static List<ISignPadPlugin> Plugins { get; set; }

        public static ISignPadPlugin GetPlugin(string name)
        {
            ISignPadPlugin plugin = Plugins.Where(p => p.Name == name).FirstOrDefault();

            return plugin ?? throw new Exception($"No plugin found with name '{name}'");
        }

        public static ISignPadPlugin GetAnyPlugin()
        {
            ISignPadPlugin plugin = Plugins.Where(p => p.IsAvailable).FirstOrDefault();

            return plugin ?? throw new Exception($"No plugin found.");
        }

        public void LoadPlugins(string path)
        {
            Plugins = new List<ISignPadPlugin>();

            LoadPluginAssemblyFile(path);

            LoadPluginInstance();
        }

        void LoadPluginAssemblyFile(string path) =>
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
    }
}
