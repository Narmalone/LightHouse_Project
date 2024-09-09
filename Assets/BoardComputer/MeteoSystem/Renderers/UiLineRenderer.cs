using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiLineRenderer : Graphic
{
    public Vector2Int gridSize;  // Taille de la grille (par exemple 10x10)
    public List<Vector2> points; // Liste des points à rendre
    public UiGridRenderer grid;  // Référence à la grille
    public bool IsCenter = true; // Booléen pour gérer le centrage

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

        // Calcul des offsets pour centrer ou non la grille
        float xOffset = 0f;
        float yOffset = 0f;

        if (IsCenter)
        {
            xOffset = -width / 2f;
            yOffset = -height / 2f;
        }

        float angle = 0;

        // Dessine les segments de ligne pour chaque point
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 point1 = points[i];
            Vector2 point2 = points[i + 1];

            // Ajoute les offsets en fonction de IsCenter
            point1 = new Vector2(point1.x * unitWidth + xOffset, point1.y * unitHeight + yOffset);
            point2 = new Vector2(point2.x * unitWidth + xOffset, point2.y * unitHeight + yOffset);

            angle = GetAngle(point1, point2);
            CreateLineSegment(point1, point2, angle, vh);
        }
    }

    // Crée un segment de ligne entre deux points
    private void CreateLineSegment(Vector3 point1, Vector3 point2, float angle, VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Calcul de l'offset pour l'épaisseur de la ligne
        Quaternion rotation = Quaternion.Euler(0, 0, angle + 90);
        Vector3 thicknessOffset = rotation * new Vector3(thickness / 2, 0);

        // Ajouter les quatre vertices nécessaires pour dessiner le rectangle qui représente la ligne
        vertex.position = point1 - thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = point1 + thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = point2 + thicknessOffset;
        vh.AddVert(vertex);

        vertex.position = point2 - thicknessOffset;
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
