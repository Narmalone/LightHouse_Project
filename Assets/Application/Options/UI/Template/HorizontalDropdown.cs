using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class HorizontalDropdown : VisualElement
{
    [UxmlAttribute]
    public string label { get; set; } = "Label";

    [UxmlAttribute]
    public List<string> choices { get; set; } = new() { "Option1", "Option2", "Option3" };

    private Label valueLabel;
    private int currentIndex = 0;

    private Button leftButton;
    private Button rightButton;
    private VisualElement dotsContainer;

    public string Value => choices.Count > 0 ? choices[currentIndex] : "";

    public HorizontalDropdown()
    {
        RegisterCallback<AttachToPanelEvent>(_ => Build());
    }

    private void Build()
    {
        Clear();

        // Layout horizontal principal : [Label] [Right Side]
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.justifyContent = Justify.FlexStart;
        style.paddingTop = 4;
        style.paddingBottom = 4;
        style.paddingLeft = 4;
        style.paddingRight = 4;

        if (choices == null || choices.Count == 0)
            choices = new List<string> { "Option1", "Option2", "Option3" };

        // === Label à gauche ===
        var nameLabel = new Label(label);
        nameLabel.style.marginRight = 8;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        Add(nameLabel);

        // === Conteneur vertical (nav + dots) ===
        var rightStack = new VisualElement();
        rightStack.style.flexDirection = FlexDirection.Column;
        rightStack.style.alignItems = Align.Center;
        rightStack.style.justifyContent = Justify.Center;
        Add(rightStack);

        // === Ligne flèches + texte ===
        var navRow = new VisualElement();
        navRow.style.flexDirection = FlexDirection.Row;
        navRow.style.alignItems = Align.Center;
        rightStack.Add(navRow);

        leftButton = new Button(() =>
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                UpdateDisplay();
            }
        })
        { text = "<" };
        leftButton.style.marginRight = 4;
        navRow.Add(leftButton);

        valueLabel = new Label(choices[currentIndex]);
        valueLabel.style.minWidth = 80;
        valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        valueLabel.style.marginLeft = 4;
        valueLabel.style.marginRight = 4;
        navRow.Add(valueLabel);

        rightButton = new Button(() =>
        {
            if (currentIndex < choices.Count - 1)
            {
                currentIndex++;
                UpdateDisplay();
            }
        })
        { text = ">" };
        rightButton.style.marginLeft = 4;
        navRow.Add(rightButton);

        // === Ligne des petits points ===
        dotsContainer = new VisualElement();
        dotsContainer.style.flexDirection = FlexDirection.Row;
        dotsContainer.style.justifyContent = Justify.Center;
        dotsContainer.style.marginTop = 2;
        rightStack.Add(dotsContainer);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (choices == null || choices.Count == 0) return;

        valueLabel.text = choices[currentIndex];
        leftButton.SetEnabled(currentIndex > 0);
        rightButton.SetEnabled(currentIndex < choices.Count - 1);

        dotsContainer.Clear();
        for (int i = 0; i < choices.Count; i++)
        {
            var dot = new Label(i == currentIndex ? "●" : "○");
            dot.style.marginLeft = 2;
            dot.style.marginRight = 2;
            dot.style.fontSize = 10;
            dotsContainer.Add(dot);
        }
    }
}
