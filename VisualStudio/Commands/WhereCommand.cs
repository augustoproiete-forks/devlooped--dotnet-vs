﻿using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using vswhere;
using System.Collections.Generic;

namespace VisualStudio
{
    class WhereCommand : Command<WhereCommandDescriptor>
    {
        readonly WhereService whereService;

        public WhereCommand(WhereCommandDescriptor descriptor, WhereService whereService) : base(descriptor) =>
            this.whereService = whereService;

        public override async Task ExecuteAsync(TextWriter output)
        {
            var instances = (await whereService.GetAllInstancesAsync(
                Descriptor.Options,
                Descriptor.WorkloadsArguments.Concat(Descriptor.ExtraArguments))).ToList();

            foreach (var instance in instances)
            {
                var properties = GetProperties(instance);

                if (string.IsNullOrEmpty(Descriptor.Property))
                {
                    output.WriteLine();
                    output.WriteLine($"{ instance.DisplayName} - Version { instance.Catalog.ProductDisplayVersion}");

                    foreach (var prop in properties)
                        output.WriteLine($"{prop.PropertyName}: {prop.PropertyValue}");
                }
                else
                {
                    Console.WriteLine(
                        properties
                            .Where(x => x.PropertyName == Descriptor.Property)
                            .Select(x => x.PropertyValue)
                            .FirstOrDefault() ?? string.Empty);
                }
            }
        }

        IEnumerable<(string PropertyName, object PropertyValue)> GetProperties(VisualStudioInstance instance)
        {
            var props = GetProperties(instance, "Catalog", "Properties").ToList();
            props.AddRange(GetProperties(instance.Catalog).Select(x => ($"Catalog.{x.PropertyName}", x.PropertyValue)));
            props.AddRange(GetProperties(instance.Properties).Select(x => ($"Properties.{x.PropertyName}", x.PropertyValue)));

            return props;
        }

        IEnumerable<(string PropertyName, object PropertyValue)> GetProperties<T>(T instance, params string[] skipProps) =>
            instance.GetType().GetProperties()
                .Where(x => skipProps == null || !skipProps.Contains(x.Name))
                .Select(x => (x.Name, x.GetValue(instance)));
    }
}