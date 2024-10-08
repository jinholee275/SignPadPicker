﻿using SignPadPicker.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SignPadPicker
{
    public class SignPadLoader
    {
        public static List<ISignPadPlugin> Plugins { get; set; } = new List<ISignPadPlugin>();

        public static ISignPadPlugin GetPlugin(string name)
            => Plugins.FirstOrDefault(p => p.Name == name)
            ?? throw new NoPluginFoundException($"No plugin found with name '{name}'");

        private static IEnumerable<ISignPadPlugin> findPlugins(IEnumerable<string> names)
            => names.SelectMany(n => Plugins.Where(p => p.Name.Equals(n)));

        public static ISignPadPlugin GetPlugin(IEnumerable<string> names = null, IEnumerable<string> excepts = null, bool onlyPhysicalDevice = false)
        {
            IEnumerable<ISignPadPlugin> plugins = names == null
                ? Plugins : findPlugins(names);

            if (excepts != null)
            {
                plugins = plugins.Except(findPlugins(excepts));
            }

            if (onlyPhysicalDevice)
            {
                plugins = plugins.Except(Plugins.Where(p => !p.IsPhysicalDevice));
            }

            return plugins.FirstOrDefault(p => p.IsAvailable) ?? throw new SignPadNotAvailableException();
        }

        public void LoadPlugins(string path)
        {
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
            .Select(file =>
                Assembly.LoadFile(Path.GetFullPath(file)))
            .SelectMany(a => a.GetTypes())
            .Where(p => typeof(ISignPadPlugin).IsAssignableFrom(p) && p.IsClass)
            .Where(type => !Plugins.Select(p => p.GetType()).Contains(type))
            .ToList()
            .ForEach(type => Plugins.Add((ISignPadPlugin) Activator.CreateInstance(type)));
    }
}
