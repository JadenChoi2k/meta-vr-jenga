/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using Meta.Voice.Hub.Interfaces;
using UnityEngine.Networking;

namespace Meta.Voice.Hub.Markdown
{
    
    [CustomEditor(typeof(MarkdownPage))]
    public class MarkdownInspector : Editor, IOverrideSize
    {
        private GUIStyle _linkStyle;
        private GUIStyle _normalTextStyle;
        private GUIStyle _imageLabelStyle;
        private Dictionary<string, Texture2D> _cachedImages = new Dictionary<string, Texture2D>();

        private Vector2 scrollView;

        public float OverrideWidth { get; set; } = -1;
        public float OverrideHeight { get; set; } = -1;
        
        private readonly Regex urlRegex = new Regex(@"(https?://[^\s]+)", RegexOptions.Compiled);
        private readonly Regex imageRegex = new Regex(@"!\[(.*?)\]\((.*?)\)", RegexOptions.Compiled);
        private readonly Regex splitRegex = new Regex(@"(!\[.*?\]\(.*?\))|(https?://[^\s]+)", RegexOptions.Compiled);
        private readonly Regex numberedContent = new Regex(@"^(\d+.) (.*)", RegexOptions.Compiled | RegexOptions.Multiline);
        private readonly Regex blockRegex = new Regex(@"^>\s*(.*)", RegexOptions.Compiled | RegexOptions.Multiline);
        
        private const float Padding = 55;
        
        private float RenderedWindowWidth => (OverrideWidth > 0 ? OverrideWidth : EditorGUIUtility.currentViewWidth) - Padding;
        
        private List<Action> drawingCallbacks = new List<Action>();
        private bool repaint;

        public override void OnInspectorGUI()
        {
            if (drawingCallbacks.Count == 0)
            {
                Initialize();

                if (drawingCallbacks.Count == 0)
                {
                    base.OnInspectorGUI();
                }
            }

            for (int i = 0; i < drawingCallbacks.Count; i++)
            {
                drawingCallbacks[i].Invoke();
            }
            
            if(repaint) Refresh();
        }

        protected void Initialize()
        {
            repaint = false;
            drawingCallbacks.Clear();
            var markdownPage = ((MarkdownPage)target);
            if (!markdownPage)
            {
                return;
            }

            var markdownFile = markdownPage.MarkdownFile;
            if (!markdownFile)
            {
                return;
            }

            var text = markdownFile.text;

            if (_linkStyle == null)
            {
                _linkStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_normalTextStyle == null)
            {
                _normalTextStyle = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            Event currentEvent = Event.current;

            Draw(() =>
            {
                scrollView = GUILayout.BeginScrollView(scrollView);
                GUILayout.BeginVertical(GUILayout.Width(RenderedWindowWidth));
            });
            
            string[] parts = splitRegex.Split(text);
            foreach (string part in parts)
            {
                if (imageRegex.IsMatch(part))
                {
                    Match imageMatch = imageRegex.Match(part);

                    if (imageMatch.Success)
                    {
                        string imagePath = imageMatch.Groups[2].Value;

                        if (!_cachedImages.ContainsKey(imagePath))
                        {
                            if (urlRegex.IsMatch(imagePath))
                            {
                                LoadImageFromUrl(imagePath);
                            }
                            else
                            {
                                var path = AssetDatabase.GetAssetPath(markdownPage);
                                var dir = Path.GetDirectoryName(path);
                                Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/" + imagePath);
                                if (!image)
                                {
                                    // Get the path of target markdown file
                                    string markdownPath = AssetDatabase.GetAssetPath(markdownFile);
                                    // Get the directory of the markdown file
                                    string markdownDir = System.IO.Path.GetDirectoryName(markdownPath);
                                    image = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(markdownDir,
                                        imagePath));
                                }

                                _cachedImages[imagePath] = image;
                            }
                        }

                        Draw(() =>
                        {
                            if (_cachedImages.TryGetValue(imagePath, out Texture2D img) && img)
                            {
                                float aspectRatio = 1;
                                float width = img.width;
                                float height = img.height;
                                if (img.width > RenderedWindowWidth - Padding)
                                {
                                    width = RenderedWindowWidth - Padding;
                                    aspectRatio = img.width / (float) img.height;
                                    height = width / aspectRatio;
                                }

                                if (null == _imageLabelStyle)
                                {
                                    _imageLabelStyle = new GUIStyle(GUI.skin.label)
                                    {
                                        alignment = TextAnchor.MiddleCenter,
                                        imagePosition = ImagePosition.ImageAbove
                                    };
                                }

                                GUIContent content = new GUIContent(img);
                                Rect imageLabelRect = GUILayoutUtility.GetRect(content, _imageLabelStyle,
                                    GUILayout.Height(height), GUILayout.Width(width));

                                if (GUI.Button(imageLabelRect, content, _imageLabelStyle))
                                {
                                    ImageViewer.ShowWindow(img, Path.GetFileNameWithoutExtension(imagePath));
                                }
                            }
                        });
                    }
                }
                else if (urlRegex.IsMatch(part))
                {
                    Draw(() =>
                    {
                        EditorGUILayout.BeginHorizontal();

                        GUILayout.Space(EditorGUI.indentLevel * 15);
                        GUILayout.Label("<color=blue>" + part + "</color>", _linkStyle,
                            GUILayout.MaxWidth(RenderedWindowWidth));

                        Rect linkRect = GUILayoutUtility.GetLastRect();
                        if (currentEvent.type == EventType.MouseDown && linkRect.Contains(currentEvent.mousePosition))
                        {
                            Application.OpenURL(part);
                        }

                        EditorGUILayout.EndHorizontal();
                    });
                }
                else
                {
                    ProcessSections(part, blockRegex, ProcessBlock);
                }
            }

            Draw(() =>
            {
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            });
        }

        private void Draw(Action action)
        {
            drawingCallbacks.Add(action);
        }

        private void DrawText(string text)
        {
            Draw(() =>
            {
                EditorGUILayout.TextArea(ParseMarkdown(text.Trim()), _normalTextStyle,
                        GUILayout.MaxWidth(RenderedWindowWidth));
            });
        }

        private void ProcessBlock(string text, Match block)
        {
            ProcessSections(text, numberedContent, ProcessOrderedList);
            if (null != block)
            {
                Draw(() =>
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(RenderedWindowWidth));
                    GUILayout.Space(8);
                    GUILayout.BeginVertical();
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                });
                
                DrawText(ParseMarkdown(block.Groups[1].Value));
                
                Draw(() =>
                {
                    GUILayout.EndVertical();
                    GUILayout.EndVertical();
                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                });
            }
        }

        private void ProcessOrderedList(string text, Match numberedContentMatch)
        {
            if(!string.IsNullOrEmpty(text.Trim())) DrawText(text);
            if (null != numberedContentMatch)
            {
                Draw(() =>
                {
                    GUILayout.BeginHorizontal(GUILayout.MaxWidth(RenderedWindowWidth));
                    GUILayout.Space(8);
                    GUILayout.Label(numberedContentMatch.Groups[1].Value, _normalTextStyle);
                });
                DrawText(numberedContentMatch.Groups[2].Value);
                Draw(() =>
                {
                    GUILayout.Space(8);
                    GUILayout.EndHorizontal();
                });
            }
        }

        void ProcessSections(string input, Regex regex, Action<string, Match> onProcessSection)
        {
            MatchCollection matches = regex.Matches(input);

            int start = 0;

            foreach (Match match in matches)
            {
                // Get the section before the match
                string sectionBefore = input.Substring(start, match.Index - start);

                // Process the section before the match and the match itself
                onProcessSection(sectionBefore, match.Success ? match : null);

                // Update the start position for the next iteration
                start = match.Index + match.Length;
            }

            // Process the section after the last match
            string sectionAfter = input.Substring(start);
            onProcessSection(sectionAfter, null);
        }


        public static string ParseMarkdown(string markdown)
        {
            // Headers
            markdown = Regex.Replace(markdown, @"^######\s(.*?)$", "<size=14><b>$1</b></size>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^#####\s(.*?)$", "<size=16><b>$1</b></size>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^####\s(.*?)$", "<size=18><b>$1</b></size>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^###\s(.*?)$", "<size=20><b>$1</b></size>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^##\s(.*?)$", "<size=22><b>$1</b></size>", RegexOptions.Multiline);
            markdown = Regex.Replace(markdown, @"^#\s(.*?)$", "<size=24><b>$1</b></size>", RegexOptions.Multiline);

            // Bold
            markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "<b>$1</b>", RegexOptions.Multiline);

            // Italic
            markdown = Regex.Replace(markdown, @"\*(.*?)\*", "<i>$1</i>", RegexOptions.Multiline);

            // Code blocks
            markdown = Regex.Replace(markdown, @"(?s)```(.*?)```", m =>
            {
                var codeLines = m.Groups[1].Value.Trim().Split('\n');
                string result = string.Empty;
                foreach (var line in codeLines)
                {
                    result += $"  <color=#a1b56c>{line}</color>\n";
                }

                return result;
            }, RegexOptions.Multiline);
            
            // Blockquotes
            markdown = Regex.Replace(markdown, @"^>\s?(.*?)$", "<color=#a1b56c>$1</color>", RegexOptions.Multiline);

            // Raw Urls
            markdown = Regex.Replace(markdown, @"(https?:\/\/[^\s""'<>]+)",
                "<link><color=#a1b56c><u>$1</u></color></link>", RegexOptions.Multiline);

            // Unordered lists
            markdown = Regex.Replace(markdown, @"^\s*\*\s(.*?)$", "• $1", RegexOptions.Multiline);

            // Ordered lists
            markdown = Regex.Replace(markdown, @"^(\d+)\.\s(.*?)$", "$1. $2", RegexOptions.Multiline);

            return markdown;
        }

        private void LoadImageFromUrl(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            request.SendWebRequest().completed += operation =>
            {
                if (request.responseCode == 200)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    _cachedImages[url] = texture;
                    DelayedRefresh();
                }
                else
                {
                    Debug.LogError($"Failed to load image from URL [Error {request.responseCode}]: {url}");
                }
            };
        }

        private void DelayedRefresh()
        {
            repaint = true;
        }

        private void Refresh()
        {
            repaint = true;
            Repaint();
        }
    }
}
