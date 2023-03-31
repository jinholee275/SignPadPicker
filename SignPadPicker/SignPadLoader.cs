using SignPadPicker.Exceptions;
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
            ?? throw new NoPluginFoundException($"No plugin found with name '{name}'");

        public static ISignPadPlugin GetPlugin(IEnumerable<string> names = null)
            => (names?.Select(name => GetPlugin(name)) ?? Plugins)
            .FirstOrDefault(p => p.IsAvailable)
            ?? throw new SignPadNotAvailableException();

        public void LoadPlugins(string path)
        {
            Plugins = new List<ISignPadPlugin>();
            LoadPluginAssemblyFile(path);
        }

        private void LoadPluginAssemblyFile(string path)
            => Directory
            .GetFiles(path)
            .ToList()
            .Where(file =>
                Path.GetFileName(file).StartsWith("SignPadPicker.") &&
                Path.GetFileName(file).EndsWith("Adaptor.dll"))
            .ToList()
            .Select(file => Assembly.LoadFile(Path.GetFullPath(file)))
            .SelectMany(a => a.GetTypes())
            .Where(p => typeof(ISignPadPlugin).IsAssignableFrom(p) && p.IsClass)
            .ToList()
            .ForEach(type => Plugins.Add((ISignPadPlugin) Activator.CreateInstance(type)));
    }
}
