using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiLineRenderer : Graphic
{
    public Vector2Int gridSize;  // Taille de la grille (par exemple 10x10)
    public List<Vector2> points; // Liste des points à rendre
    public UiGridRenderer grid;  // Référence à la grille

    public float thickness = 10f; // Épaisseur de la ligne

    private float width;   // Largeur de l'élément UI
    private float height;  // Hauteur de l'élément UI
    private float unitWidth;  // Largeur d'une unité de la grille
    private float unitHeight; // Hauteur d'une unité de la grille

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); // Réinitialisation des vertices

        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        unitWidth = width / gridSize.x;
        unitHeight = height / gridSize.y;

        if (points.Count < 2)
        {
            return;
        }

        float angle = 0;

        // Dessine les segments de ligne pour chaque point
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 point1 = points[i];
            Vector2 point2 = points[i + 1];

            angle = GetAngle(point1, point2);
            CreateLineSegment(point1, point2, angle, vh);
        }
    }

    // Crée un segment de ligne entre deux points
    private void CreateLineSegment(Vector2 point1, Vector2 point2, float angle, VertexHelper vh)
    {
        // Créer des vertex pour chaque extrémité du segment de ligne
        Vector3 p1 = new Vector3(unitWidth * point1.x, unitHeight * point1.y);
        Vector3 p2 = new Vector3(unitWidth * point2.x, unitHeight * point2.y);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Déplace le point 1 selon l'angle calculé
        Quaternion rotation = Quaternion.Euler(0, 0, angle + 90);
        Vector3 thicknessOffset = rotation * new Vector3(thickness / 2, 0);

        // Ajouter les quatre vertices nécessaires pour dessiner le rectangle qui représente la ligne
        vertex.position = p1 - thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = p1 + thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = p2 + thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = p2 - thicknessOffset;
        vh.AddVert(vertex);

        // Ajoute deux triangles pour former un rectangle (un segment de ligne)
        int startIndex = vh.currentVertCount - 4;
        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    // Calcule l'angle entre deux points
    public float GetAngle(Vector2 me, Vector2 target)
    {
        return Mathf.Atan2(target.y - me.y, target.x - me.x) * Mathf.Rad2Deg;
    }

    // Mise à jour des vertices si la grille change
    private void Update()
    {
        if (grid != null && gridSize != grid.gridSize)
        {
            gridSize = grid.gridSize;
            SetVerticesDirty();
        }
    }
}
