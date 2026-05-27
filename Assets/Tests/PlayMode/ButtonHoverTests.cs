using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit tests for ButtonHover.
/// Ensures visual reset logic behaves correctly for UI hover effects.
/// </summary>
public class ButtonHoverTests {
    private GameObject _obj;
    private ButtonHover _hover;
    private Image _image;
    private RectTransform _rect;

    /// <summary>
    /// Creates a test UI object and injects required dependencies.
    /// </summary>
    [SetUp]
    public void Setup() {
        _obj = new GameObject("HoverButton");

        _rect = _obj.AddComponent<RectTransform>();
        _image = _obj.AddComponent<Image>();

        _hover = _obj.AddComponent<ButtonHover>();

        _hover.GetType().GetField("hoverOverlay",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_hover, _image);

        _hover.GetType().GetField("buttonRoot",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(_hover, _rect);
    }

    /// <summary>
    /// Ensures ResetVisualState restores default scale values.
    /// </summary>
    [Test]
    public void ResetVisualState_Sets_DefaultValues() {
        _image.color = Color.red;
        _rect.localScale = Vector3.one * 2f;

        var method = typeof(ButtonHover)
            .GetMethod("ResetVisualState",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(_hover, null);

        Assert.AreEqual(Vector3.one, _rect.localScale);
    }
}