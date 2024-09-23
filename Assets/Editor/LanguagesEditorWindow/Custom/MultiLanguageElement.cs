using System.Collections.Generic;
using UnityEngine.UIElements;

public class MultiLanguageElement : VisualElement
{
    private Label languageLabel;
    private TextField valueTextField;

    public MultiLanguageElement()
    {
        languageLabel = new Label();
        valueTextField = new TextField();

        Add(languageLabel);
        Add(valueTextField);

        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        style.paddingBottom = 5;
        style.top = 5;
        style.left = 5;
        style.right = 5;
    }

    public void SetData(MultiLanguage element)
    {
        languageLabel.text = element.Language.ToString();
        valueTextField.value = element.Value;
    }
}


public class MultiLanguageGroupElement : VisualElement
{
    private ListView languageListView;

    public MultiLanguageGroupElement()
    {
        languageListView = new ListView();
        languageListView.itemHeight = 20;
        languageListView.makeItem = () => new MultiLanguageElement();
        languageListView.bindItem = (element, index) =>
        {
            MultiLanguageElement languageElement = (MultiLanguageElement)element;
            languageElement.SetData((MultiLanguage)languageListView.itemsSource[index]);
        };

        Add(languageListView);
    }

    public void SetData(List<MultiLanguage> languages)
    {
        languageListView.itemsSource = languages;
    }
}