﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace UnityEditor.Rendering.HighDefinition
{
    internal class LightUnitSliderUIDrawer
    {
        private class LightUnitSlider
        {
            public LightUnitSlider(LightUnit unit, LightUnitUILevel[] levels, string cautionTooltip)
            {
                m_Unit = unit;

                // Load builtin caution icon.
                m_CautionContent = EditorGUIUtility.IconContent("console.warnicon.sml");
                m_CautionContent.tooltip = cautionTooltip;

                foreach (var l in levels)
                {
                    AddLevel(l);
                }

                s_MarkerContent = new GUIContent(string.Empty);
            }

            private void AddLevel(LightUnitUILevel level)
            {
                m_Levels.Add(level);

                // Update the slider ranges.
                if (level.range.y > m_RangeMax)
                    m_RangeMax = level.range.y;
                else if (level.range.x < m_RangeMin)
                    m_RangeMin = level.range.x;
            }

            private void CurrentLevelIcon(float value, out GUIContent level, out Vector2 range)
            {
                foreach (var l in m_Levels)
                {
                    if (value >= l.range.x && value <= l.range.y)
                    {
                        level = l.content;
                        range = l.range;
                        return;
                    }
                }

                // If value out of range, indicate caution. (For now assume caution feedback is last)
                level = m_CautionContent;
                range = Vector2.positiveInfinity;
            }

            public void Draw(Rect rect, SerializedProperty value)
            {
                // Fetch the rects
                GetRects(rect, out var sliderRect, out var iconRect);

                // Slider
                DoSlider(sliderRect, value, m_RangeMin, m_RangeMax);

                // Markers
                foreach (var l in m_Levels)
                {
                    DoSliderMarker(sliderRect, l, m_RangeMax);
                }

                // Icon
                CurrentLevelIcon(value.floatValue, out var iconContent, out var range);
                DoIcon(iconRect, iconContent, range.y);

                // Place tooltip on slider thumb.
                DoThumbTooltip(sliderRect, value.floatValue, value.floatValue / m_RangeMax, iconContent.tooltip);
            }

            private void DoSliderMarker(Rect rect, LightUnitUILevel level, float rangeMax)
            {
                const float width  = 3f;
                const float height = 2f;

                float x = level.range.y / rangeMax;

                var markerRect = rect;
                markerRect.width  = width;
                markerRect.height = height;

                // Vertically align with slider.
                markerRect.y += (EditorGUIUtility.singleLineHeight / 2f) - 1;

                // Horizontally place on slider.
                markerRect.x = rect.x + rect.width * x;

                // Clamp to the slider edges.
                const float halfWidth = width * 0.5f;
                float min = rect.x + halfWidth;
                float max = (rect.x + rect.width) - halfWidth;
                markerRect.x = Mathf.Clamp(markerRect.x, min, max);

                // Center the marker on value.
                markerRect.x -= halfWidth;

                // Draw marker by manually drawing the rect, and an empty label with the tooltip.
                EditorGUI.DrawRect(markerRect, Color.white);

                // Consider enlarging this tooltip rect so that it's easier to discover?
                s_MarkerContent.tooltip = FormatTooltip(m_Unit, level.content.tooltip, level.range.y);
                EditorGUI.LabelField(markerRect, s_MarkerContent);
            }

            private void DoThumbTooltip(Rect rect, float value, float normalizedValue, string tooltip)
            {
                const float size = 10f;
                const float halfSize = size * 0.5f;

                var thumbMarkerRect = rect;
                thumbMarkerRect.width  = size;
                thumbMarkerRect.height = size;

                // Vertically align with slider
                thumbMarkerRect.y += halfSize - 1f;

                // Horizontally place tooltip on the wheel,
                thumbMarkerRect.x  = rect.x + (rect.width - size) * normalizedValue;

                s_MarkerContent.tooltip = FormatTooltip(m_Unit, tooltip, value);
                EditorGUI.LabelField(thumbMarkerRect, s_MarkerContent);
            }

            private void DoIcon(Rect rect, GUIContent icon, float range)
            {
                var oldColor = GUI.color;
                GUI.color = Color.clear;
                EditorGUI.DrawTextureTransparent(rect, icon.image);
                GUI.color = oldColor;

                EditorGUI.LabelField(rect, new GUIContent(string.Empty, FormatTooltip(m_Unit, icon.tooltip, range)));
            }

            private LightUnit m_Unit;
            private GUIContent s_MarkerContent;
            private GUIContent m_CautionContent;
            private float m_RangeMin = float.MaxValue;
            private float m_RangeMax = float.MinValue;
            private List<LightUnitUILevel> m_Levels = new List<LightUnitUILevel>();
        }

        private static readonly Dictionary<LightUnit, LightUnitSlider> s_LightUnitSliderMap = new Dictionary<LightUnit, LightUnitSlider>();

        static LightUnitSliderUIDrawer()
        {
            var luxSlider = new LightUnitSlider(LightUnit.Lux, LightUnitValuesTable.k_LuxValueTable, "Higher than Sunlight");
            s_LightUnitSliderMap.Add(LightUnit.Lux, luxSlider);

            var lumenSlider = new LightUnitSlider(LightUnit.Lumen, LightUnitValuesTable.k_LumenValueTable, "Very High Intensity Light");
            s_LightUnitSliderMap.Add(LightUnit.Lumen, lumenSlider);
        }

        public void OnGUI(LightUnit unit, SerializedProperty value)
        {
            OnGUI(unit, value, EditorGUILayout.GetControlRect());
        }

        public void OnGUI(LightUnit unit, SerializedProperty value, Rect rect)
        {
            if (!s_LightUnitSliderMap.TryGetValue(unit, out var lightUnitSlider))
                return;

            // Disable indentation (breaks tooltips otherwise).
            var prevIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw
            lightUnitSlider.Draw(rect, value);

            // Restore indentation
            EditorGUI.indentLevel = prevIndentLevel;
        }

        private static void GetRects(Rect baseRect, out Rect sliderRect, out Rect iconRect)
        {
            const int k_IconSeparator = 6;

            sliderRect = baseRect;
            sliderRect.width -= EditorGUIUtility.singleLineHeight + k_IconSeparator;

            iconRect = baseRect;
            iconRect.x += sliderRect.width + k_IconSeparator;
            iconRect.width = EditorGUIUtility.singleLineHeight;
        }

        private static void DoSlider(Rect rect, SerializedProperty value, float leftValue, float rightValue)
        {
            // TODO: Look into compiling a lambda to access internal slider function for logarithmic sliding.
            value.floatValue = GUI.HorizontalSlider(rect, value.floatValue, leftValue, rightValue, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb);
        }

        private static string FormatTooltip(LightUnit unit, string baseTooltip, float value)
        {
            string formatValue;

            // Massage the value for readability (with respect to the UX request).
            if (value >= Single.PositiveInfinity)
                formatValue = "###K";
            else if (value >= 100000)
                formatValue = (value / 1000).ToString("#,0K");
            else if (value >= 10000)
                formatValue = (value / 1000).ToString("0.#") + "K";
            else
                formatValue = value.ToString("#.0");

            return baseTooltip + " " + formatValue + " " + unit;
        }
    }
}
