/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Meta.WitAi.Composer.Data.Info;
using Meta.WitAi;
using Meta.WitAi.Json;

namespace Meta.Voice.Composer.Data
{
    /// <summary>
    /// This class will examine a wit export (in Zip format) and extract the
    /// relevant composer data.
    /// </summary>
    [UsedImplicitly]
    public class ComposerParser : ExportParser
    {
        #region ComposerSpecificExtraction
        private readonly List<PathValues> _sharedVariables = new List<PathValues>();
        private readonly List<PathValues> _serverVariables = new List<PathValues>();
        private readonly ArrayList _clientVariables = new ArrayList();
        private readonly HashSet<string> _actions = new HashSet<string>();

        private const string ComposerFolderName = "/composer/";
        private const string ResponseFieldsName = "response_fields";
        private const string ContextFieldsName = "context_fields";
        private const string ModulesName = "modules";
        private const string TypeName = "type";
        private const string TextName = "text";
        private const string PathName = "path";

        private static ComposerExtension _extensions;
        private static IContextNodeParser[] _parserExtensions;

        /// <summary>
        /// A simple static holder class for any IContextNodeParser extensions
        /// </summary>
        private class ComposerExtension : PluggableBase<IContextNodeParser> {}

        /// <summary>
        /// The types of modules / nodes in Composer which we may want to traverse.
        /// </summary>
        private static class ModuleType
        {
            public const string Response = "response";
            public const string Decision = "decision";
            public const string Context = "context";
        }

        /// <summary>
        /// Parses the provided zip export and extracts the context map values
        /// used within the composer graphs.
        /// </summary>
        public ComposerGraph[] ExtractComposerInfo(ZipArchive zip)
        {
            var ext = new ComposerExtension();
            _parserExtensions = ext.GetAll<IContextNodeParser>();
            return ExtractCanvases(GetJsonFileNames(ComposerFolderName, zip), zip);
        }

        /// <summary>
        /// Parses the given zip archives and adds them to the
        /// </summary>
        /// <param name="jsonCanvases"></param>
        /// <returns>The graphs of all the provided canvases</returns>
        private ComposerGraph[] ExtractCanvases(List<ZipArchiveEntry> jsonCanvases, ZipArchive zip)
        {
            var canvases = new ComposerGraph[jsonCanvases.Count];
            for (var i = 0; i < jsonCanvases.Count; i++)
            {
                var jsonNode = ExtractJson(zip, jsonCanvases[i].Name);
                canvases[i].contextMap = ParseModules(jsonNode);
                canvases[i].actions = _actions.ToArray();
                Array.Sort(canvases[i].actions);
                var name = System.IO.Path.GetFileNameWithoutExtension(jsonCanvases[i].Name);
                name = name.Substring(0, 1).ToUpper() + name.Substring(1, name.Length - 1); //capitalize 1st letter
                canvases[i].canvasName = name;
            }
            return canvases;
        }

        /// <summary>
        ///  List-based class to process nodes more easily
        /// </summary>
        public class PathValues
        {
            public readonly string Path;
            public readonly List<string> Values = new List<string>();
            public PathValues(string path)
            {
                Path = path;
            }

            public PathValues(string path, string firstValue) :this(path)
            {
                Values.Add(firstValue);
            }

            public ComposerGraphValues ConvertToStruct()
            {
                var graphValues = new ComposerGraphValues
                {
                    path = Path,
                    values = new string[Values.Count]
                };
                Values.CopyTo(graphValues.values);
                return graphValues;
            }
        }

        /// <summary>
        /// Parses the Composer Map for context map variables
        /// </summary>
        private ContextMapPaths ParseModules(WitResponseNode json)
        {
            foreach (var module in json[ModulesName].Childs)
            {
                switch (module[TypeName].Value)
                {
                    case ModuleType.Response:
                        GatherMapValuesFromResponse(module);
                        break;

                    case ModuleType.Decision:
                        GatherMapValuesFromDecision(module);
                        break;

                    case ModuleType.Context:
                        GatherMapValuesFromContext(module);
                        continue;
                }
            }
            ContextMapPaths result = ConvertToContextMapValues();
            return result;
        }

        /// <summary>A public method that allows external extensions to add
        /// values to the server variable list.
        /// </summary>
        public void AddServerVariable(PathValues values)
        {
            if (!_serverVariables.Contains(values))
            {
                _serverVariables.Add(values);
            }
        }

        /// <summary>
        /// Parses a Response module in the composer graph, adding any context map values to the correct list.
        /// </summary>
        /// <param name="module">the JSON representation of the module</param>
        private void GatherMapValuesFromResponse(WitResponseNode module)
        {
            const string actionName = "action";

            // gather the usages of context map parameters
            // if present, will look like "here there, {name}, wanna play {activity}?"
            var responseText = module[ResponseFieldsName][TextName].ToString();
            var matches = Regex.Match(responseText, $@"{{(.*)}}");
            while(matches.Success) {
                var path = matches.Groups[1].Value;
                matches = matches.NextMatch();
                if (IsPathAlreadyDiscovered(path))
                    continue;

                if (MoveToShared(path))
                    continue;
                _clientVariables.Add(path);
            }

            //gather response actions
            string action = module[ResponseFieldsName][actionName].ToString();
            if (String.IsNullOrEmpty(action)) return;
            _actions.Add(action.Replace("\"",""));
        }


        /// <summary>
        /// Parses a Decision module in the composer graph, adding any context map values to the correct list.
        /// </summary>
        /// <param name="module"></param>
        private void GatherMapValuesFromDecision(WitResponseNode module)
        {
            const string decisionFieldsName = "decision_fields";
            const string conditionNodesName = "condition_nodes";
            const string contextWithValueFieldsName = "context_with_value_fields";

            var nodes = module[decisionFieldsName]?[conditionNodesName];
            for (var i = 0; i < nodes?.Count; i++)
            {
                var path = nodes[i][contextWithValueFieldsName]?[PathName].Value;
                if (IsPathAlreadyDiscovered(path))
                    continue;
                if (MoveToShared(path))
                    continue;

                _clientVariables.Add(path);
            }
        }
        /// <summary>
        /// Moves a path/value item from server to shared list.
        /// </summary>
        /// <returns>true if the item was found and moved; false otherwise.</returns>
        private  bool MoveToShared(string path)
        {
            var found = _serverVariables.Find((item) => item.Path == path);
            if (found != null)
            {
                _serverVariables.Remove(found);
                _sharedVariables.Add(found);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Whether the given path has already been saved in one of the given lists
        /// </summary>
        private bool IsPathAlreadyDiscovered(string path)
        {
            return string.IsNullOrEmpty(path) ||
                _clientVariables.Contains(path) ||
                _serverVariables.Exists((readable) => readable.Path == path) ||
                _sharedVariables.Exists((readable) => readable.Path == path);
        }

        /// <summary>
        /// Parses the given context node and places any context map variables in the correct list.
        /// </summary>
        private void GatherMapValuesFromContext(WitResponseNode module)
        {
            const string contextTypeName = "context_type";
            const string setFieldsName = "set_fields";
            const string saveFieldsName = "save_fields";
            const string copyFieldsName = "copy_fields";
            const string saveName = "save";
            const string setName = "set";
            const string copyName = "copy";
            const string entityName = "entity";
            const string valueName = "value";
            const string toName = "to";

            string path, value;
            var saves = module[ContextFieldsName]?[saveFieldsName];
            var sets = module[ContextFieldsName]?[setFieldsName];
            var contextType = module[ContextFieldsName]?[contextTypeName].Value;
            switch (contextType)
            {
                case saveName:
                    path = saves?[PathName].ToString();
                    value = saves?[entityName].ToString();
                    break;

                case setName:
                    path = module[ContextFieldsName]?[setFieldsName]?[PathName].ToString();
                    value = sets?[valueName].ToString();
                    break;

                case copyName:
                    path = module[ContextFieldsName]?[copyFieldsName]?[toName].ToString();
                    value = sets?[valueName].ToString();
                    break;

                default:
                    foreach (var extension in _parserExtensions)
                    {
                        if (extension.HandlesType(contextType))
                        {
                            extension.ProcessNode(module, this);
                        }
                    }
                    return;
            }
            path = path?.Replace("\"", "");
            value = value?.Replace("\"", "");

            //check if we have the same path and value in readwrite
            PathValues foundWritable = _sharedVariables.Find((item) => item.Path == path);
            if (foundWritable != null && !foundWritable.Values.Contains(value))
            {
                foundWritable?.Values.Add(value);
                return;
            }

            //check if we have the same path and value in basic read
            var foundReadable = _serverVariables.Find((item) => item.Path == path);
            if (foundReadable != null && !foundReadable.Values.Contains(value))
            {
                foundReadable?.Values.Add(value);
                return;
            }

            var newReadable = new PathValues(path, value);
            if (_clientVariables.Contains(path)) //move to readWrite if would be in both.
            {
                _clientVariables.Remove(path);
                _sharedVariables.Add(newReadable);
                return;
            }
            if(!_serverVariables.Exists((readable) => readable.Path == path))
                _serverVariables.Add(newReadable);
        }

        /// <summary>
        /// Takes all three lists of variables and aggregate them into a single object
        /// </summary>
        private ContextMapPaths ConvertToContextMapValues()
        {
            var result = new ContextMapPaths();
            result.client = (string[])_clientVariables.ToArray(typeof(string));

            result.server = new ComposerGraphValues[_serverVariables.Count];
            for (var i = 0; i < _serverVariables.Count; i++) result.server[i] = _serverVariables[i].ConvertToStruct();

            result.shared = new ComposerGraphValues[_sharedVariables.Count];
            for (var i = 0; i < _sharedVariables.Count; i++) result.shared[i] = _sharedVariables[i].ConvertToStruct();

            _sharedVariables.Clear();
            _serverVariables.Clear();
            _clientVariables.Clear();

            return result;
        }
        #endregion
    }
}
