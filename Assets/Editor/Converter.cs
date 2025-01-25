using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[CustomEditor(typeof(DataContainer))]
public class GoogleSheetCSVReader : Editor
{
    // Variables pour la configuration
    public string DocumentID = "1mTGrjxJ5IirMeOxi_u9FLyKzFA3aBl4mUmEgaW3BE9g";
    private List<string> sheetGids = new List<string>(); // Liste des GIDs de feuilles
    private List<bool> selectedSheets; // Feuilles sélectionnées par l'utilisateur

    private List<string> availableColumns = new List<string>();
    private List<bool> selectedColumns; // Colonnes sélectionnées

    public override void OnInspectorGUI()
    {
        var container = (DataContainer)target;

        GUILayout.Label("Google Sheet Importer", EditorStyles.boldLabel);

        // Champ pour entrer l'ID du document Google
        DocumentID = EditorGUILayout.TextField("Document ID", DocumentID);

        // Champ pour ajouter manuellement des `gid` de feuilles
        GUILayout.Label("Ajouter un GID de feuille :");
        if (GUILayout.Button("Ajouter une feuille"))
        {
            sheetGids.Add(""); // Ajoute une nouvelle entrée vide pour un nouveau GID
        }

        // Afficher les GID de feuilles à importer
        for (int i = 0; i < sheetGids.Count; i++)
        {
            GUILayout.BeginHorizontal();
            sheetGids[i] = EditorGUILayout.TextField("Sheet GID", sheetGids[i]);

            // Supprimer une feuille
            if (GUILayout.Button("Supprimer"))
            {
                sheetGids.RemoveAt(i);
                if (selectedSheets != null && i < selectedSheets.Count)
                {
                    selectedSheets.RemoveAt(i);
                }
            }
            GUILayout.EndHorizontal();
        }

        // Afficher les feuilles et permettre la sélection
        if (sheetGids.Count > 0)
        {
            GUILayout.Label("Sélectionnez les feuilles à importer :", EditorStyles.boldLabel);
            if (selectedSheets == null || selectedSheets.Count != sheetGids.Count)
            {
                selectedSheets = new List<bool>(new bool[sheetGids.Count]);
            }

            for (int i = 0; i < sheetGids.Count; i++)
            {
                selectedSheets[i] = EditorGUILayout.Toggle($"Sheet GID: {sheetGids[i]}", selectedSheets[i]);
            }
        }

        // Afficher les colonnes disponibles et permettre la sélection
        if (availableColumns.Count > 0)
        {
            GUILayout.Label("Sélectionnez les colonnes à importer :", EditorStyles.boldLabel);
            if (selectedColumns == null || selectedColumns.Count != availableColumns.Count)
            {
                selectedColumns = new List<bool>(new bool[availableColumns.Count]);
            }

            for (int i = 0; i < availableColumns.Count; i++)
            {
                selectedColumns[i] = EditorGUILayout.Toggle(availableColumns[i], selectedColumns[i]);
            }
        }

        // Bouton pour importer les données
        if (GUILayout.Button("IMPORT"))
        {
            ImportGoogleSheetAsync(DocumentID, container);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    // Méthode async pour importer les données de Google Sheets
    private async void ImportGoogleSheetAsync(string documentID, DataContainer container)
    {
        for (int i = 0; i < sheetGids.Count; i++)
        {
            if (selectedSheets[i])
            {
                string gid = sheetGids[i];
                string csvURL = $"https://docs.google.com/spreadsheets/d/{documentID}/export?format=csv&gid={gid}";

                string csvData = await GetCSVDataAsync(csvURL);

                if (!string.IsNullOrEmpty(csvData))
                {
                    ParseCSV(csvData, container, gid);
                }
            }
        }
    }

    // Fonction asynchrone pour obtenir les données CSV
    private async Task<string> GetCSVDataAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching CSV data: " + request.error);
                return null;
            }

            return request.downloadHandler.text;
        }
    }

    // Fonction pour parser le CSV
    private void ParseCSV(string csvData, DataContainer container, string gid)
    {
        if (string.IsNullOrEmpty(csvData)) return;
        string[] rows = csvData.Split('\n');
        List<Dictionary<string, string>> parsedData = new List<Dictionary<string, string>>();
        if (rows.Length < 1) return; // Pas de données

        string[] headers = rows[0].Split(','); // Premières lignes = noms de colonnes

        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            Dictionary<string, string> rowData = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length && j < columns.Length; j++)
            {
                if (selectedColumns[j])
                {
                    rowData[headers[j]] = columns[j];
                }
            }

            parsedData.Add(rowData);
        }

        container.StoreSheetData(gid, parsedData);
    }
}
