using UnityEngine;
using UnityEditor;

namespace Ludumdare43.Editor
{
    public class SortingOrderEditor : EditorWindow
    {
        [SerializeField]
        int currentSelectSortingLayerIndex = 0;

        [MenuItem("Custom/SortingOrderEditor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SortingOrderEditor));
        }

        SpriteRenderer[] spriteRenderers;

        void OnGUI()
        {
            GUIHandler();
        }

        void GUIHandler()
        {
            titleContent.text = "Sorting Order";
            string[] sortingLayerNames = new string[SortingLayer.layers.Length];

            for (int i = 0; i < sortingLayerNames.Length; i++) {
                int id = SortingLayer.layers[i].id;
                sortingLayerNames[i] = SortingLayer.IDToName(id);
            }

            GUILayout.Label ("Setting", EditorStyles.boldLabel);
            currentSelectSortingLayerIndex = EditorGUILayout.Popup("Target Layer", currentSelectSortingLayerIndex, sortingLayerNames);

            EditorGUILayout.Space();

            if (GUILayout.Button("Update"))
                UpdateSortingOrder(currentSelectSortingLayerIndex);

            if (GUILayout.Button("Reset"))
                ResetSortingOrder(currentSelectSortingLayerIndex);
        }

        void UpdateSortingOrder(int sortingLayerID)
        {
            bool isConfirm = EditorUtility.DisplayDialog("Update Sorting Order", "Are you sure to update a sorting layer on selected layer?", "Yes", "No");
            if (!isConfirm)
                return;

            spriteRenderers = GetSpriteRendererAtTheScene();

            foreach (SpriteRenderer obj in spriteRenderers) {
                if (obj.sortingLayerID != SortingLayer.layers[currentSelectSortingLayerIndex].id)
                    continue;
                Undo.RecordObject(obj, "Update sorting order...");
                obj.sortingOrder = (int)(obj.gameObject.transform.position.y * -100.0f);
            }
        }

        void ResetSortingOrder(int sortingLayerID)
        {
            bool isConfirm = EditorUtility.DisplayDialog("Reset Sorting Order", "Are you sure to reset a sorting layer on selected layer?", "Yes", "No");
            if (!isConfirm)
                return;

            spriteRenderers = GetSpriteRendererAtTheScene();

            foreach (SpriteRenderer obj in spriteRenderers) {
                if (obj.sortingLayerID != SortingLayer.layers[currentSelectSortingLayerIndex].id)
                    continue;
                Undo.RecordObject(obj, "Reset sorting order...");
                obj.sortingOrder = 0;
            }
        }

        SpriteRenderer[] GetSpriteRendererAtTheScene()
        {
            return Object.FindObjectsOfType(typeof(SpriteRenderer)) as SpriteRenderer[];
        }
    }
}
