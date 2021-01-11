# dx-highlighter
A Deus Ex/System Shock inspired pulsing bounds item highlighter

# How to Use

1. Add dx_highlighter_canvas prefab to your scene;
2. Reference DXHighlighter, located on that canvas object, in your script;
3. When you want an entire GameObject highlighted, call `highlighter.Highlight(myObject);`. Alternatively, you can add individual Renderers with `highlighter.AddRenderer(myRenderer);`
4. To "unhighlight" everything, call `highlighter.ClearAll()`, or to manually remove renderers `highlighter.RemoveRenderer(myRenderer);`
