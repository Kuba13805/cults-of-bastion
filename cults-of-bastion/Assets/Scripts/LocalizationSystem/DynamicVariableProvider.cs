using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace LocalizationSystem
{
    public class DynamicVariableProvider : IVariableGroup, ISource, IEnumerable<KeyValuePair<string, IVariable>>
    {
        private readonly Dictionary<string, object> _sources = new();

        public void AddSource(string key, object source)
        {
            _sources[key] = source;
        }

        public void AddMultipleSources(Dictionary<string, object> sources)
        {
            foreach (var source in sources)
            {
                AddSource(source.Key, source.Value);
            }
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var key = selectorInfo.SelectorText;
            if (TryGetValue(key, out var variable))
            {
                selectorInfo.Result = variable.GetSourceValue(selectorInfo);
                return true;
            }

            return false;
        }

        public bool TryGetValue(string key, out IVariable value)
        {
            var parts = key.Split('.');
            if (parts.Length == 0 || !_sources.TryGetValue(parts[0], out var currentObject))
            {
                value = null;
                return false;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                if (currentObject == null)
                {
                    value = null;
                    return false;
                }

                var property = currentObject.GetType().GetProperty(parts[i]);
                if (property == null)
                {
                    value = null;
                    return false;
                }

                currentObject = property.GetValue(currentObject);
            }

            value = new ObjectVariable(currentObject);
            return true;
        }

        public IEnumerator<KeyValuePair<string, IVariable>> GetEnumerator()
        {
            foreach (var sourceKey in _sources.Keys)
            {
                var source = _sources[sourceKey];
                foreach (var property in source.GetType().GetProperties())
                {
                    yield return new KeyValuePair<string, IVariable>($"{sourceKey}.{property.Name}", new ObjectVariable(property.GetValue(source)));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ObjectVariable : IVariable
    {
        private readonly object _value;

        public ObjectVariable(object value)
        {
            _value = value;
        }

        public object GetValue(string format, IFormatProvider formatProvider) => _value;

        public object GetSourceValue(ISelectorInfo selectorInfo) => _value;
    }
}
