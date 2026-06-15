using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AYellowpaper.SerializedCollections.Editor.Data;
using AYellowpaper.SerializedCollections.Editor.States;
using AYellowpaper.SerializedCollections.KeysGenerators;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    public class SerializedDictionaryInstanceDrawer
    {
        private const float MinKeyValueLabelWidth = 40f;
        private ListState _activeState;
        private GUIContent _detailsContent;
        private readonly SerializedDictionaryAttribute _dictionaryAttribute;

        private readonly FieldInfo _fieldInfo;
        private readonly FieldInfo _keyFieldInfo;
        private bool _autoIndexIntKeys;
        private readonly IReadOnlyList<KeyListGeneratorData> _keyGeneratorsWithWindow;
        private readonly IReadOnlyList<KeyListGeneratorData> _keyGeneratorsWithoutWindow;
        private readonly GUIStyle _keyValueStyle;
        private GUIContent _label;
        private int _lastListSize = -1;
        private readonly List<int> _pagedIndices;
        private readonly PagingElement _pagingElement;
        private readonly PropertyData _propertyData;
        private bool _propertyListSettingsInitialized;
        private readonly SearchField _searchField;
        private GUIContent _shortDetailsContent;
        private bool _showSearchBar;
        private readonly SingleEditingData _singleEditingData;
        private Rect _totalRect;
        private readonly ReorderableList _unexpandedList;

        public SerializedDictionaryInstanceDrawer(SerializedProperty property, FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
            ListProperty = property.FindPropertyRelative(SerializedDictionaryDrawer.SerializedListName);

            _keyValueStyle = new GUIStyle(EditorStyles.toolbarButton);
            _keyValueStyle.padding = new RectOffset(0, 0, 0, 0);
            _keyValueStyle.border = new RectOffset(0, 0, 0, 0);
            _keyValueStyle.alignment = TextAnchor.MiddleCenter;

            DefaultState = new DefaultListState(this);
            SearchState = new SearchListState(this);
            _activeState = DefaultState;

            _dictionaryAttribute = _fieldInfo.GetCustomAttribute<SerializedDictionaryAttribute>();

            _propertyData = SCEditorUtility.GetPropertyData(ListProperty);
            _propertyData.GetElementData(SCEditorUtility.KeyFlag).Settings.DisplayName =
                _dictionaryAttribute?.KeyName ?? "Key";
            _propertyData.GetElementData(SCEditorUtility.ValueFlag).Settings.DisplayName =
                _dictionaryAttribute?.ValueName ?? "Value";
            SavePropertyData();

            _pagingElement = new PagingElement();
            _pagedIndices = new List<int>();
            UpdatePaging();

            ReorderableList = MakeList();
            _unexpandedList = MakeUnexpandedList();
            _searchField = new SearchField();

            var listField = _fieldInfo.FieldType.GetField(SerializedDictionaryDrawer.SerializedListName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var entryType = listField.FieldType.GetGenericArguments()[0];
            _keyFieldInfo = entryType.GetField(SerializedDictionaryDrawer.KeyName);
            _autoIndexIntKeys = _keyFieldInfo.FieldType == typeof(int);

            _singleEditingData = new SingleEditingData();

            var keyGenerators = KeyListGeneratorCache.GetPopulatorsForType(_keyFieldInfo.FieldType);
            _keyGeneratorsWithWindow = keyGenerators.Where(x => x.NeedsWindow).ToList();
            _keyGeneratorsWithoutWindow = keyGenerators.Where(x => !x.NeedsWindow).ToList();

            UpdateAfterInput();
        }

        internal ReorderableList ReorderableList { get; }
        internal SerializedProperty ListProperty { get; }
        internal string SearchText { get; private set; } = string.Empty;
        internal SearchListState SearchState { get; private set; }
        internal DefaultListState DefaultState { get; }

        public void OnGUI(Rect position, GUIContent label)
        {
            _totalRect = position;
            _label = new GUIContent(label);

            EditorGUI.BeginChangeCheck();
            DoList(position);
            if (EditorGUI.EndChangeCheck()) ListProperty.serializedObject.ApplyModifiedProperties();
        }

        public float GetPropertyHeight(GUIContent label)
        {
            if (!ListProperty.isExpanded)
                return SerializedDictionaryDrawer.TopHeaderClipHeight;

            return ReorderableList.GetHeight();
        }

        private void DoList(Rect position)
        {
            if (ListProperty.isExpanded)
                ReorderableList.DoList(position);
            else
                using (new GUI.ClipScope(new Rect(0, position.y, position.width + position.x,
                           SerializedDictionaryDrawer.TopHeaderClipHeight)))
                {
                    _unexpandedList.DoList(position.WithY(0));
                }
        }

        private void ProcessState()
        {
            var newState = _activeState.OnUpdate();
            if (newState != null && newState != _activeState)
            {
                _activeState.OnExit();
                _activeState = newState;
                newState.OnEnter();
            }
        }

        private SerializedProperty GetElementProperty(SerializedProperty property, bool fieldFlag)
        {
            return property.FindPropertyRelative(fieldFlag == SerializedDictionaryDrawer.KeyFlag
                ? SerializedDictionaryDrawer.KeyName
                : SerializedDictionaryDrawer.ValueName);
        }

        internal static float CalculateHeightOfElement(SerializedProperty property, bool drawKeyAsList,
            bool drawValueAsList)
        {
            var keyProperty = property.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
            var valueProperty = property.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);
            return Mathf.Max(SCEditorUtility.CalculateHeight(keyProperty, drawKeyAsList),
                SCEditorUtility.CalculateHeight(valueProperty, drawValueAsList));
        }

        private void UpdateAfterInput()
        {
            InitializeSettingsIfNeeded();
            ProcessState();
            CheckIfNewDictionary();
            CheckPaging();
            var elementsPerPage = EditorUserSettings.Get().ElementsPerPage;
            var pageCount = Mathf.Max(1, Mathf.CeilToInt((float)DefaultState.ListSize / elementsPerPage));
            ToggleSearchBar(_propertyData.AlwaysShowSearch ? true : SCEditorUtility.ShouldShowSearch(pageCount));
        }

        // TODO: This works for now, but isn't perfect. This checks if the serialized dictionary was reassigned with new(), simply by comparing the count. Should be instead done by reference equality in the future
        private void CheckIfNewDictionary()
        {
            if (_singleEditingData.IsValid && _singleEditingData.LookupTable.GetCount() != _activeState.ListSize)
            {
                var dictionary =
                    SCEditorUtility.GetPropertyValue(ListProperty, ListProperty.serializedObject.targetObject);
                _singleEditingData.LookupTable = GetLookupTable(dictionary);
                _singleEditingData.LookupTable.RecalculateOccurences();
            }
        }

        private void InitializeSettingsIfNeeded()
        {
            void InitializeSettings(bool fieldFlag)
            {
                var dictionaryType = FindGenericBaseType(typeof(SerializedDictionary<,>), _fieldInfo.FieldType);
                var genericArgs = dictionaryType.GetGenericArguments();
                var firstProperty = ListProperty.GetArrayElementAtIndex(0);
                var keySettings = CreateDisplaySettings(GetElementProperty(firstProperty, fieldFlag),
                    genericArgs[fieldFlag == SCEditorUtility.KeyFlag ? 0 : 1]);
                var settings = _propertyData.GetElementData(fieldFlag).Settings;
                settings.DisplayType = keySettings.displayType;
                settings.HasListDrawerToggle = keySettings.canToggleListDrawer;
            }

            if (!_propertyListSettingsInitialized && ListProperty.minArraySize > 0)
            {
                _propertyListSettingsInitialized = true;
                InitializeSettings(SCEditorUtility.KeyFlag);
                InitializeSettings(SCEditorUtility.ValueFlag);
                SavePropertyData();
            }
        }

        private static Type FindGenericBaseType(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) return toCheck;
                toCheck = toCheck.BaseType;
            }

            return null;
        }

        private void CheckPaging()
        {
            // TODO: Is there a better solution to check for Revert/delete/add?
            if (_lastListSize != _activeState.ListSize)
            {
                _lastListSize = _activeState.ListSize;
                UpdateSingleEditing();
                UpdatePaging();
            }
        }

        private void SavePropertyData()
        {
            SCEditorUtility.SavePropertyData(ListProperty, _propertyData);
        }

        private void UpdateSingleEditing()
        {
            if (ListProperty.serializedObject.isEditingMultipleObjects && _singleEditingData.IsValid)
            {
                _singleEditingData.Invalidate();
            }
            else if (!ListProperty.serializedObject.isEditingMultipleObjects && !_singleEditingData.IsValid)
            {
                var dictionary =
                    SCEditorUtility.GetPropertyValue(ListProperty, ListProperty.serializedObject.targetObject);
                _singleEditingData.LookupTable = GetLookupTable(dictionary);
            }
        }

        private IKeyable GetLookupTable(object dictionary)
        {
            var propInfo = dictionary.GetType().GetProperty(SerializedDictionaryDrawer.LookupTableName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (IKeyable)propInfo.GetValue(dictionary);
        }

        private void UpdatePaging()
        {
            var elementsPerPage = EditorUserSettings.Get().ElementsPerPage;
            _pagingElement.PageCount = Mathf.Max(1, Mathf.CeilToInt((float)_activeState.ListSize / elementsPerPage));

            _pagedIndices.Clear();
            _pagedIndices.Capacity = Mathf.Max(elementsPerPage, _pagedIndices.Capacity);

            var startIndex = (_pagingElement.Page - 1) * elementsPerPage;
            var endIndex = Mathf.Min(startIndex + elementsPerPage, _activeState.ListSize);
            for (var i = startIndex; i < endIndex; i++)
                _pagedIndices.Add(i);

            var shortDetailsString = _activeState.ListSize + " " + (_pagedIndices.Count == 1 ? "Element" : "Elements");
            var detailsString = _pagingElement.PageCount > 1
                ? $"{_pagedIndices[0] + 1}..{_pagedIndices.Last() + 1} / {_activeState.ListSize} Elements"
                : shortDetailsString;
            _detailsContent = new GUIContent(detailsString);
            _shortDetailsContent = new GUIContent(shortDetailsString);
        }

        private ReorderableList MakeList()
        {
            var list = new ReorderableList(_pagedIndices, typeof(int), true, true, true, true);
            list.onAddCallback += OnAdd;
            list.onRemoveCallback += OnRemove;
            list.onReorderCallbackWithDetails += OnReorder;
            list.drawElementCallback += OnDrawElement;
            list.elementHeightCallback += OnGetElementHeight;
            list.drawHeaderCallback += OnDrawHeader;
            list.drawNoneElementCallback += OnDrawNoneElement;
            return list;
        }

        private ReorderableList MakeUnexpandedList()
        {
            var list = new ReorderableList(SerializedDictionaryDrawer.NoEntriesList, typeof(int));
            list.drawHeaderCallback = OnDrawUnexpandedHeader;
            return list;
        }

        private void ToggleSearchBar(bool flag)
        {
            _showSearchBar = flag;
            ReorderableList.headerHeight = SerializedDictionaryDrawer.TopHeaderClipHeight +
                                           SerializedDictionaryDrawer.KeyValueHeaderHeight + (_showSearchBar
                                               ? SerializedDictionaryDrawer.SearchHeaderHeight
                                               : 0);
            if (!_showSearchBar)
            {
                if (_searchField.HasFocus())
                    GUI.FocusControl(null);
                SearchText = string.Empty;
            }
        }

        private void OnDrawNoneElement(Rect rect)
        {
            EditorGUI.LabelField(rect, EditorGUIUtility.TrTextContent(_activeState.NoElementsText));
        }

        private (DisplayType displayType, bool canToggleListDrawer) CreateDisplaySettings(SerializedProperty property,
            Type type)
        {
            var hasCustomEditor = SCEditorUtility.HasDrawerForProperty(property, type);
            var isGenericWithChildren =
                property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            var isArray = property.isArray && property.propertyType != SerializedPropertyType.String;
            var canToggleListDrawer = isArray || (isGenericWithChildren && hasCustomEditor);
            var displayType = DisplayType.PropertyNoLabel;
            if (canToggleListDrawer)
                displayType = DisplayType.Property;
            else if (!isArray && isGenericWithChildren && !hasCustomEditor)
                displayType = DisplayType.List;
            return (displayType, canToggleListDrawer);
        }

        private void DoPaging(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            _pagingElement.OnGUI(rect);
            if (EditorGUI.EndChangeCheck())
            {
                ReorderableList.ClearSelection();
                UpdatePaging();
            }
        }

        private void OnDrawHeader(Rect rect)
        {
            var topRect = rect.WithHeight(SerializedDictionaryDrawer.TopHeaderHeight);
            var adjustedTopRect = topRect.WithXAndWidth(_totalRect.x + 1, _totalRect.width - 1);

            DoMainHeader(adjustedTopRect.CutLeft(topRect.x - adjustedTopRect.x));
            if (_showSearchBar)
            {
                adjustedTopRect = adjustedTopRect.AppendDown(SerializedDictionaryDrawer.SearchHeaderHeight);
                DoSearch(adjustedTopRect);
            }

            DoKeyValueRect(adjustedTopRect.AppendDown(SerializedDictionaryDrawer.KeyValueHeaderHeight));

            UpdateAfterInput();
        }

        private void OnDrawUnexpandedHeader(Rect rect)
        {
            EditorGUI.BeginProperty(rect, _label, ListProperty);
            ListProperty.isExpanded = EditorGUI.Foldout(rect.WithX(rect.x - 5), ListProperty.isExpanded, _label, true);

            var detailsStyle = EditorStyles.miniLabel;
            var detailsRect = rect.AppendRight(0).AppendLeft(detailsStyle.CalcSize(_shortDetailsContent).x);
            GUI.Label(detailsRect, _shortDetailsContent, detailsStyle);

            EditorGUI.EndProperty();

            UpdateAfterInput();
        }

        private void SortByKeyValue()
        {
            var listProp = ListProperty;
            var count = listProp.arraySize;

            // Build a list of (index, keyValue, key, value) tuples
            var entries = new List<(int index, int keyValue, object key, object value)>();
            for (var i = 0; i < count; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                var valueProp = element.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);

                var keyObj = keyProp.boxedValue;
                var valueObj = valueProp.boxedValue;

                // Only sort if key is int, otherwise skip
                if (keyObj is int keyInt)
                    entries.Add((i, keyInt, keyObj, valueObj));
                else
                    // If not int, just use 0 as a fallback (will be grouped at the start)
                    entries.Add((i, 0, keyObj, valueObj));
            }

            // Sort by key value (int)
            entries.Sort((a, b) => a.keyValue.CompareTo(b.keyValue));

            // Create sorted lists
            var keyList = new List<object>(count);
            var valueList = new List<object>(count);
            foreach (var entry in entries)
            {
                keyList.Add(entry.key);
                valueList.Add(entry.value);
            }

            // Reassign the sorted keys and values to the serialized property
            for (var i = 0; i < count; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                var valueProp = element.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);

                keyProp.boxedValue = keyList[i];
                valueProp.boxedValue = valueList[i];
            }

            // Apply changes and update the UI
            ListProperty.serializedObject.ApplyModifiedProperties();
            UpdateAfterInput();
        }
        private void create_lookup_enum(){
            
        }
        private void SortByValueName()
        {
            // Get the serialized list property
            var listProp = ListProperty;
            var count = listProp.arraySize;

            // Build a list of (index, value, key) tuples
            var entries = new List<(int index, string valueName, object key, object value)>();
            for (var i = 0; i < count; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                var valueProp = element.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);

                var keyObj = keyProp.boxedValue;
                var valueObj = valueProp.boxedValue;
                var valueName = valueObj != null ? valueObj.ToString() : "";

                entries.Add((i, valueName, keyObj, valueObj));
            }

            // Sort by value name
            entries.Sort((a, b) => string.Compare(a.valueName, b.valueName, StringComparison.OrdinalIgnoreCase));

            // Create a copy of the current list
            var keyList = new List<object>(count);
            var valueList = new List<object>(count);
            foreach (var entry in entries)
            {
                keyList.Add(entry.key);
                valueList.Add(entry.value);
            }

            // Reassign the sorted keys and values to the serialized property
            for (var i = 0; i < count; i++)
            {
                var element = listProp.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                var valueProp = element.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);

                // Set the key and value using boxedValue (Unity 2022+)
                keyProp.boxedValue = keyList[i];
                valueProp.boxedValue = valueList[i];
            }

            // Apply changes and update the UI
            ListProperty.serializedObject.ApplyModifiedProperties();
            UpdateAfterInput();
        }

        private void DoMainHeader(Rect rect)
        {
            var lastTopRect = rect.AppendRight(0).WithHeight(EditorGUIUtility.singleLineHeight);

            lastTopRect = lastTopRect.AppendLeft(20);
            DoOptionsButton(lastTopRect);
            lastTopRect = lastTopRect.AppendLeft(5);

            if (_pagingElement.PageCount > 1)
            {
                lastTopRect = lastTopRect.AppendLeft(_pagingElement.GetDesiredWidth());
                DoPaging(lastTopRect);
            }

            var detailsStyle = EditorStyles.miniLabel;
            lastTopRect = lastTopRect.AppendLeft(detailsStyle.CalcSize(_detailsContent).x, 5);
            GUI.Label(lastTopRect, _detailsContent, detailsStyle);

            if (!_singleEditingData.IsValid)
            {
                lastTopRect = lastTopRect.AppendLeft(lastTopRect.height + 5);
                var guicontent = EditorGUIUtility.TrIconContent(EditorGUIUtility.Load("d_console.infoicon") as Texture,
                    "Conflict checking, duplicate key removal and populators not supported in multi object editing mode.");
                GUI.Label(lastTopRect, guicontent);
            }

            EditorGUI.BeginProperty(rect, _label, ListProperty);
            ListProperty.isExpanded = EditorGUI.Foldout(rect.WithXAndWidth(rect.x - 5, lastTopRect.x - rect.x),
                ListProperty.isExpanded, _label, true);
            EditorGUI.EndProperty();
        }

        private void DoOptionsButton(Rect rect)
        {
            var screenRect = GUIUtility.GUIToScreenRect(rect);
            if (GUI.Button(rect, EditorGUIUtility.IconContent("pane options@2x"), EditorStyles.iconButton))
            {
                var gm = new GenericMenu();
                SCEditorUtility.AddGenericMenuItem(gm, false, ListProperty.minArraySize > 0, new GUIContent("Clear"),
                    () => QueueAction(ClearList));
                SCEditorUtility.AddGenericMenuItem(gm, false, true, new GUIContent("Remove Conflicts"),
                    () => QueueAction(RemoveConflicts));
                SCEditorUtility.AddGenericMenuItem(gm, false, _keyGeneratorsWithWindow.Count > 0,
                    new GUIContent("Bulk Edit..."), () => OpenKeysGeneratorSelectorWindow(screenRect));
                if (_keyGeneratorsWithoutWindow.Count > 0)
                {
                    gm.AddSeparator(string.Empty);
                    foreach (var generatorData in _keyGeneratorsWithoutWindow)
                        SCEditorUtility.AddGenericMenuItem(gm, false, true, new GUIContent(generatorData.Name),
                            OnPopulatorDataSelected, generatorData);
                }

                gm.AddSeparator(string.Empty);
                SCEditorUtility.AddGenericMenuItem(gm, _propertyData.AlwaysShowSearch, true,
                    new GUIContent("Always Show Search"), ToggleAlwaysShowSearchPropertyData);
                gm.AddSeparator(string.Empty);
                SCEditorUtility.AddGenericMenuItem(gm, false, true, new GUIContent("Sort By Value Name"),
                    () => QueueAction(SortByValueName));
                gm.AddItem(new GUIContent("Preferences..."), false,
                    () => SettingsService.OpenUserPreferences(EditorUserSettingsProvider.PreferencesPath));
                SCEditorUtility.AddGenericMenuItem(gm, false, true, new GUIContent("Sort By Key (int)"),
                    () => QueueAction(SortByKeyValue));
                gm.AddItem(new GUIContent("allow key edit"), false,
                () => _autoIndexIntKeys = !_autoIndexIntKeys);
                gm.DropDown(rect);
            }
        }

        private void OnPopulatorDataSelected(object userData)
        {
            var data = (KeyListGeneratorData)userData;
            var so = (KeyListGenerator)ScriptableObject.CreateInstance(data.GeneratorType);
            so.hideFlags = HideFlags.DontSave;
            ApplyPopulatorQueued(so, ModificationType.Add);
        }

        private void OpenKeysGeneratorSelectorWindow(Rect rect)
        {
            var window = ScriptableObject.CreateInstance<KeyListGeneratorSelectorWindow>();
            window.Initialize(_keyGeneratorsWithWindow, _keyFieldInfo.FieldType);
            window.ShowAsDropDown(rect, new Vector2(400, 200));
            window.OnApply += ApplyPopulatorQueued;
        }

        private void ToggleAlwaysShowSearchPropertyData()
        {
            _propertyData.AlwaysShowSearch = !_propertyData.AlwaysShowSearch;
            SavePropertyData();
        }

        private void DoKeyValueRect(Rect rect)
        {
            var width = GetDesiredKeyLabelWidth(rect.width, 22);
            var leftRect = rect.WithWidth(width);
            var rightRect = leftRect.AppendRight(rect.width - width);

            if (_propertyData != null)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    _keyValueStyle.Draw(leftRect,
                        EditorGUIUtility.TrTextContent(_propertyData.GetElementData(SerializedDictionaryDrawer.KeyFlag)
                            .Settings.DisplayName), false, false, false, false);
                    _keyValueStyle.Draw(rightRect,
                        EditorGUIUtility.TrTextContent(_propertyData
                            .GetElementData(SerializedDictionaryDrawer.ValueFlag).Settings.DisplayName), false, false,
                        false, false);
                }

                var changeSizeRect = leftRect.AppendRight(5);
                changeSizeRect.x -= 2;
                EditorGUI.BeginChangeCheck();
                var newWidth = SCEditorUtility.DoHorizontalScale(changeSizeRect,
                    _propertyData.KeyLabelWidth > 0f ? _propertyData.KeyLabelWidth : width);
                if (EditorGUI.EndChangeCheck())
                {
                    _propertyData.KeyLabelWidth = Mathf.Max(newWidth, MinKeyValueLabelWidth);
                    SavePropertyData();
                }
            }

            if (ListProperty.minArraySize > 0)
            {
                DoDisplayTypeToggle(leftRect, SerializedDictionaryDrawer.KeyFlag);
                DoDisplayTypeToggle(rightRect, SerializedDictionaryDrawer.ValueFlag);
            }

            EditorGUI.DrawRect(rect.AppendDown(1, -1), SerializedDictionaryDrawer.BorderColor);
        }

        private float GetDesiredKeyLabelWidth(float maxWidth, float offset = 0f)
        {
            var desiredWidth = _propertyData is { KeyLabelWidth: > 0 }
                ? _propertyData.KeyLabelWidth
                : EditorGUIUtility.labelWidth;
            return Mathf.Clamp(desiredWidth + offset, MinKeyValueLabelWidth, maxWidth - MinKeyValueLabelWidth);
        }

        private void DoSearch(Rect rect)
        {
            EditorGUI.DrawRect(rect.AppendLeft(1), SerializedDictionaryDrawer.BorderColor);
            EditorGUI.DrawRect(rect.AppendRight(1, -1), SerializedDictionaryDrawer.BorderColor);
            EditorGUI.DrawRect(rect.AppendDown(1, -1), SerializedDictionaryDrawer.BorderColor);

            SearchText = _searchField.OnToolbarGUI(rect.CutTop(2).CutHorizontal(6), SearchText);
        }

        private void ApplyPopulatorQueued(KeyListGenerator populator, ModificationType modificationType)
        {
            var array = populator.GetKeys(_keyFieldInfo.FieldType).OfType<object>().ToArray();
            QueueAction(() => ApplyPopulator(array, modificationType));
        }

        private void QueueAction(EditorApplication.CallbackFunction action)
        {
            EditorApplication.delayCall += action;
        }

        private void ApplyPopulator(IEnumerable<object> elements, ModificationType modificationType)
        {
            foreach (var targetObject in ListProperty.serializedObject.targetObjects)
            {
                Undo.RecordObject(targetObject, "Populate");
                var dictionary = SCEditorUtility.GetPropertyValue(ListProperty, targetObject);
                var lookupTable = GetLookupTable(dictionary);

                if (modificationType == ModificationType.Add)
                    AddElements(lookupTable, elements);
                else if (modificationType == ModificationType.Remove)
                    RemoveElements(lookupTable, elements);
                else if (modificationType == ModificationType.Confine)
                    ConfineElements(lookupTable, elements);

                lookupTable.RecalculateOccurences();
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
            }

            ListProperty.serializedObject.Update();
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private static void AddElements(IKeyable lookupTable, IEnumerable<object> elements)
        {
            foreach (var key in elements)
            {
                var occurences = lookupTable.GetOccurences(key);
                if (occurences.Count > 0)
                    continue;
                lookupTable.AddKey(key);
            }
        }

        private static void ConfineElements(IKeyable lookupTable, IEnumerable<object> elements)
        {
            var keysToRemove = lookupTable.Keys.OfType<object>().ToHashSet();
            foreach (var key in elements)
                keysToRemove.Remove(key);

            RemoveElements(lookupTable, keysToRemove);
        }

        private static void RemoveElements(IKeyable lookupTable, IEnumerable<object> elements)
        {
            var indicesToRemove =
                elements.SelectMany(x => lookupTable.GetOccurences(x)).OrderByDescending(index => index);
            foreach (var index in indicesToRemove) lookupTable.RemoveAt(index);
        }

        private void ClearList()
        {
            ListProperty.ClearArray();
            ListProperty.serializedObject.ApplyModifiedProperties();
        }

        private void RemoveConflicts()
        {
            foreach (var targetObject in ListProperty.serializedObject.targetObjects)
            {
                Undo.RecordObject(targetObject, "Remove Conflicts");
                var dictionary = SCEditorUtility.GetPropertyValue(ListProperty, targetObject);
                var lookupTable = GetLookupTable(dictionary);

                var duplicateIndices = new List<int>();

                foreach (var key in lookupTable.Keys)
                {
                    var occurences = lookupTable.GetOccurences(key);
                    for (var i = 1; i < occurences.Count; i++)
                        duplicateIndices.Add(occurences[i]);
                }

                foreach (var indexToRemove in duplicateIndices.OrderByDescending(x => x))
                    lookupTable.RemoveAt(indexToRemove);

                lookupTable.RecalculateOccurences();
                PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
            }

            ListProperty.serializedObject.Update();
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        private void DoDisplayTypeToggle(Rect contentRect, bool fieldFlag)
        {
            var displayData = _propertyData.GetElementData(fieldFlag);

            if (displayData.Settings.HasListDrawerToggle)
            {
                var rightRectToggle = new Rect(contentRect);
                rightRectToggle.x += rightRectToggle.width - 18;
                rightRectToggle.width = 18;
                EditorGUI.BeginChangeCheck();
                var newValue = GUI.Toggle(rightRectToggle, displayData.IsListToggleActive,
                    SerializedDictionaryDrawer.DisplayTypeToggleContent, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    displayData.IsListToggleActive = newValue;
                    SavePropertyData();
                }
            }
        }

        private float OnGetElementHeight(int index)
        {
            var actualIndex = _pagedIndices[index];
            var element = _activeState.GetPropertyAtIndex(actualIndex);
            return CalculateHeightOfElement(element,
                _propertyData.GetElementData(SerializedDictionaryDrawer.KeyFlag).EffectiveDisplayType ==
                DisplayType.List,
                _propertyData.GetElementData(SerializedDictionaryDrawer.ValueFlag).EffectiveDisplayType ==
                DisplayType.List);
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            const int lineLeftSpace = 2;
            const int lineWidth = 1;
            const int lineRightSpace = 12;
            const int totalSpace = lineLeftSpace + lineWidth + lineRightSpace;

            var actualIndex = _pagedIndices[index];

            var kvp = _activeState.GetPropertyAtIndex(actualIndex);
            var keyRect = rect.WithSize(GetDesiredKeyLabelWidth(rect.width) - lineLeftSpace,
                EditorGUIUtility.singleLineHeight);
            var lineRect = keyRect.WithXAndWidth(keyRect.x + keyRect.width + lineLeftSpace, lineWidth)
                .WithHeight(rect.height);
            var valueRect = keyRect.AppendRight(rect.width - keyRect.width - totalSpace, totalSpace);

            var keyProperty = kvp.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
            var valueProperty = kvp.FindPropertyRelative(SerializedDictionaryDrawer.ValueName);

            var prevColor = GUI.color;
            if (_singleEditingData.IsValid)
            {
                var keyObject = _keyFieldInfo.GetValue(_singleEditingData.LookupTable.GetKeyAt(actualIndex));
                var occurences = _singleEditingData.LookupTable.GetOccurences(keyObject);


                if (occurences.Count > 1) GUI.color = occurences[0] == actualIndex ? Color.yellow : Color.red;
                if (!SerializedCollectionsUtility.IsValidKey(keyObject)) GUI.color = Color.red;
            }

            var keyDisplayData = _propertyData.GetElementData(SerializedDictionaryDrawer.KeyFlag);
            if (_autoIndexIntKeys)
            {
                EditorGUI.BeginDisabledGroup(true);
                DrawGroupedElement(keyRect, 20, keyProperty, keyDisplayData.EffectiveDisplayType);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                DrawGroupedElement(keyRect, 20, keyProperty, keyDisplayData.EffectiveDisplayType);
            }

            EditorGUI.DrawRect(lineRect, new Color(36 / 255f, 36 / 255f, 36 / 255f));
            GUI.color = prevColor;
            var valueObject = valueProperty.boxedValue;
            var valueOccurrences = _singleEditingData.LookupTable.GetValueOccurences(valueObject);
            if (valueOccurrences.Count > 1) GUI.color = valueOccurrences[0] == actualIndex ? Color.yellow : Color.red;
            var valueDisplayData = _propertyData.GetElementData(SerializedDictionaryDrawer.ValueFlag);
            DrawGroupedElement(valueRect, lineRightSpace, valueProperty, valueDisplayData.EffectiveDisplayType);
            GUI.color = prevColor;
        }

        private void DrawGroupedElement(Rect rect, int spaceForProperty, SerializedProperty property,
            DisplayType displayType)
        {
            using (new LabelWidth(rect.width * 0.4f))
            {
                var height = SCEditorUtility.CalculateHeight(property.Copy(), displayType);
                var groupRect = rect.CutLeft(-spaceForProperty).WithHeight(height);
                GUI.BeginGroup(groupRect);

                var elementRect = new Rect(spaceForProperty, 0, rect.width, height);
                _activeState.DrawElement(elementRect, property, displayType);

                DrawInvisibleProperty(rect.WithWidth(spaceForProperty), property);

                GUI.EndGroup();
            }
        }

        internal static void DrawInvisibleProperty(Rect rect, SerializedProperty property)
        {
            const int propertyOffset = 5;

            GUI.BeginClip(rect.CutLeft(-propertyOffset));
            EditorGUI.BeginProperty(rect, GUIContent.none, property);
            EditorGUI.EndProperty();
            GUI.EndClip();
        }

        internal static void DrawElement(Rect rect, SerializedProperty property, DisplayType displayType,
            Action<SerializedProperty> BeforeDrawingCallback = null,
            Action<SerializedProperty> AfterDrawingCallback = null)
        {
            switch (displayType)
            {
                case DisplayType.Property:
                    BeforeDrawingCallback?.Invoke(property);
                    EditorGUI.PropertyField(rect, property, true);
                    AfterDrawingCallback?.Invoke(property);
                    break;
                case DisplayType.PropertyNoLabel:
                    BeforeDrawingCallback?.Invoke(property);
                    EditorGUI.PropertyField(rect, property, GUIContent.none, true);
                    AfterDrawingCallback?.Invoke(property);
                    break;
                case DisplayType.List:
                    var childRect = rect.WithHeight(0);
                    foreach (var prop in SCEditorUtility.GetChildren(property.Copy()))
                    {
                        childRect = childRect.AppendDown(EditorGUI.GetPropertyHeight(prop, true));
                        BeforeDrawingCallback?.Invoke(prop);
                        EditorGUI.PropertyField(childRect, prop, true);
                        AfterDrawingCallback?.Invoke(prop);
                    }

                    break;
            }
        }

        private void OnAdd(ReorderableList list)
        {
            // Always insert at the end (bottom) of the list
                    int targetIndex = 0;
            int actualTargetIndex = 0;
            if (ListProperty.arraySize == 0)
            {
                _activeState.InserElementAt(0);
            }
            else
            {

                    targetIndex = list.selectedIndices.Count > 0 && list.selectedIndices[0] >= 0
                ? list.selectedIndices[0]
                : _pagedIndices.Count - 1;
                actualTargetIndex = targetIndex < _pagedIndices.Count
                ? _pagedIndices[targetIndex] + 1
                : ListProperty.arraySize;
                if (actualTargetIndex > ListProperty.arraySize) actualTargetIndex = ListProperty.arraySize;
                _activeState.InserElementAt(actualTargetIndex);
            }


            // --- Increment key value if it's an int ---
            // Get the new element's property
            var newElement = ListProperty.GetArrayElementAtIndex(actualTargetIndex);
            var keyProp = newElement.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);

            // Only increment if key is int
            if (keyProp.propertyType == SerializedPropertyType.Integer)
            {
                // Find the max int key in the list (excluding the new element)
                var maxKey = int.MinValue;
                for (var i = 0; i < ListProperty.arraySize - 1; i++)
                {
                    var element = ListProperty.GetArrayElementAtIndex(i);
                    var kProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                    if (kProp.propertyType == SerializedPropertyType.Integer)
                    {
                        var val = kProp.intValue;
                        if (val > maxKey)
                            maxKey = val;
                    }
                }

                // Set the new key to maxKey + 1 (or 0 if list was empty)
                keyProp.intValue = maxKey == int.MinValue ? 0 : maxKey + 1;
            }

            if (_autoIndexIntKeys)
                ReindexIntKeysToListOrder();
        }

        private void OnReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            UpdatePaging();
            ListProperty.MoveArrayElement(_pagedIndices[oldIndex], _pagedIndices[newIndex]);
            if (_autoIndexIntKeys)
                ReindexIntKeysToListOrder();
        }

        private void OnRemove(ReorderableList list)
        {
            _activeState.RemoveElementAt(_pagedIndices[list.index]);
            if (_autoIndexIntKeys)
                ReindexIntKeysToListOrder();
            UpdatePaging();
        }

        private void ReindexIntKeysToListOrder()
        {
            if (!_autoIndexIntKeys) return;

            for (var i = 0; i < ListProperty.arraySize; i++)
            {
                var element = ListProperty.GetArrayElementAtIndex(i);
                var keyProp = element.FindPropertyRelative(SerializedDictionaryDrawer.KeyName);
                if (keyProp.propertyType == SerializedPropertyType.Integer)
                    keyProp.intValue = i;
            }
        }

        private class SingleEditingData
        {
            public IKeyable LookupTable;
            public bool IsValid => LookupTable != null;

            public void Invalidate()
            {
                LookupTable = null;
            }
        }
    }
}